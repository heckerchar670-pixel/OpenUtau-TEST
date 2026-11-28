using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenUtau.Core;
using OpenUtau.Core.Render;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Classic {
    /// <summary>
    /// 怀旧渲染器：在内置 Worldline 结果上叠加旧式 UTAU 音色。
    /// </summary>
    public class NostalgiaRenderer : IRenderer {
        readonly IRenderer inner = new WorldlineRenderer(version: 10);

        public USingerType SingerType => inner.SingerType;
        public bool SupportsRenderPitch => inner.SupportsRenderPitch;
        public bool SupportsRealCurve => inner.SupportsRealCurve;
        public bool SupportsPhonemeEnvelope => inner.SupportsPhonemeEnvelope;
        public bool SupportsExpression(UExpressionDescriptor descriptor) => inner.SupportsExpression(descriptor);
        public RenderResult Layout(RenderPhrase phrase) => inner.Layout(phrase);
        public RenderPhraseGroup[]? GetPhraseGroups(OpenUtau.Core.Pipeline.PhraseSource source) => inner.GetPhraseGroups(source);
        public (double HeadMs, double TailMs) PhrasePadding(USinger singer, IEnumerable<UPhoneme> phonemes)
            => inner.PhrasePadding(singer, phonemes);

        public async Task<RenderResult> Render(RenderPhrase phrase, Progress progress, int trackNo,
            CancellationTokenSource cancellation, bool isPreRender = false, RenderPhraseEvents? renderEvents = null) {
            var result = await inner.Render(phrase, progress, trackNo, cancellation, isPreRender, renderEvents);
            if (result.samples != null) {
                // 复制后处理，避免改动缓存数据
                result.samples = Age(result.samples);
            }
            return result;
        }

        // 以 44.1kHz 为基准的参数；全部确定性计算，保证缓存结果一致
        const float SampleRate = 44100f;
        const float CutoffHz = 9000f;      // 旧版重采样器的高频衰减
        const int FrameSamples = 220;      // 约 5 毫秒一帧，模拟旧引擎按帧合成的台阶感
        const float FrameJitter = 0.015f;  // 帧间音量的细微起伏
        const float NoiseFloor = 0.0007f;  // 底噪（约 -63 dB）

        static float[] Age(float[] input) {
            var output = new float[input.Length];
            var delay = new float[97]; // 约 2.2 毫秒梳状滤波，产生轻微金属质感
            float lowpass = 0;
            float coefficient = 1f - MathF.Exp(-2f * MathF.PI * CutoffHz / SampleRate);
            float held = 0;
            uint seed = 2463534242;     // xorshift 噪声种子
            float frameGain = 1f;
            for (int i = 0; i < input.Length; ++i) {
                seed ^= seed << 13; seed ^= seed >> 17; seed ^= seed << 5;
                float noise = (seed / (float)uint.MaxValue - 0.5f) * 2f;
                if (i % FrameSamples == 0) {
                    frameGain = 1f + FrameJitter * noise;
                }
                lowpass += coefficient * (input[i] - lowpass);          // 先限带，再半采样率保持，混叠更接近旧引擎
                if (i % 2 == 0) held = lowpass;
                float x = held + 0.12f * delay[i % delay.Length];
                delay[i % delay.Length] = held;
                x = x * frameGain + NoiseFloor * noise;
                x = MathF.Tanh(x * 1.1f) * 0.92f;                       // 轻微饱和
                output[i] = MathF.Round(x * 2048f) / 2048f;             // 12 位量化
            }
            return output;
        }

        public RenderPitchResult LoadRenderedPitch(RenderPhrase phrase) => inner.LoadRenderedPitch(phrase);
        public List<RenderRealCurveResult> LoadRenderedRealCurves(RenderPhrase phrase) => inner.LoadRenderedRealCurves(phrase);
        public UExpressionDescriptor[] GetSuggestedExpressions(USinger singer, URenderSettings renderSettings)
            => inner.GetSuggestedExpressions(singer, renderSettings);
        public override string ToString() => Renderers.NOSTALGIA;
    }
}
