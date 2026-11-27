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

        static float[] Age(float[] input) {
            var output = new float[input.Length];
            float held = 0;
            for (int i = 0; i < input.Length; ++i) {
                if (i % 2 == 0) held = input[i];   // 半采样率保持
                float x = 0.35f * input[i] + 0.65f * held;
                output[i] = MathF.Round(x * 2048f) / 2048f; // 12 位量化
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
