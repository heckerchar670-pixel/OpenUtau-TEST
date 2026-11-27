using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OpenUtau.Api;
using OpenUtau.Core.Ustx;

namespace OpenUtau.Plugin.Builtin {
    /// <summary>
    /// CLASSIC 音素器：适配 VCV、CVVC、CV 声库，并把西班牙语歌词转写为假名。不修改声库文件。
    /// </summary>
    [Phonemizer("CLASSIC Phonemizer", "CLASSIC", language: "JA")]
    public class ClassicNostalgiaPhonemizer : Phonemizer {
        // 辅音 = 按 a i u e o 排列的假名
        static readonly Dictionary<string, string[]> Table = new[] {
            "=あ,い,う,え,お", "k=か,き,く,け,こ", "g=が,ぎ,ぐ,げ,ご", "s=さ,し,す,せ,そ",
            "t=た,てぃ,とぅ,て,と", "d=だ,でぃ,どぅ,で,ど", "n=な,に,ぬ,ね,の", "h=は,ひ,ふ,へ,ほ",
            "b=ば,び,ぶ,べ,ぼ", "p=ぱ,ぴ,ぷ,ぺ,ぽ", "m=ま,み,む,め,も", "y=や,い,ゆ,いぇ,よ",
            "r=ら,り,る,れ,ろ", "w=わ,うぃ,う,うぇ,うぉ", "f=ふぁ,ふぃ,ふ,ふぇ,ふぉ",
            "C=ちゃ,ち,ちゅ,ちぇ,ちょ", "N=にゃ,に,にゅ,にぇ,にょ", "S=しゃ,し,しゅ,しぇ,しょ",
            "J=じゃ,じ,じゅ,じぇ,じょ", "z=ざ,じ,ず,ぜ,ぞ",
        }.Select(s => s.Split('=')).ToDictionary(p => p[0], p => p[1].Split(','));

        USinger? singer;

        public override void SetSinger(USinger singer) => this.singer = singer;

        protected virtual List<string> ToKana(string lyric) {
            var s = lyric.ToLowerInvariant()
                .Replace('á', 'a').Replace('é', 'e').Replace('í', 'i').Replace('ó', 'o').Replace('ú', 'u');
            s = s.Replace("ch", "C").Replace("sh", "S").Replace("ll", "y").Replace("rr", "r").Replace("ñ", "N");
            s = Regex.Replace(s, "gu(?=[ei])", "G");
            s = Regex.Replace(s, "g(?=[ei])", "j");
            s = Regex.Replace(s, "c(?=[ei])", "s");
            s = s.Replace("qu", "k").Replace("c", "k").Replace("q", "k").Replace("z", "s").Replace("h", "")
                .Replace("j", "h").Replace("G", "g").Replace("v", "b").Replace("l", "r").Replace("x", "ks")
                .Replace("ü", "u");
            return ToMorae(s);
        }

        protected static List<string> ToMorae(string s) {
            var morae = new List<string>();
            foreach (Match m in Regex.Matches(s, "([kgstdnhbpmyrwfzCNSJ]?)([aiueo])|([kgstdnhbpmyrwfzCNSJ])")) {
                if (m.Groups[2].Success) {
                    morae.Add(Table[m.Groups[1].Value]["aiueo".IndexOf(m.Groups[2].Value[0])]);
                } else {
                    // 单独辅音：n 用拨音，t、d 补 o，其余补 u
                    var c = m.Groups[3].Value;
                    morae.Add(c == "n" ? "ん" : Table[c][c == "t" || c == "d" ? 4 : 2]);
                }
            }
            return morae;
        }

        static string TailVowel(string kana) {
            if (kana.EndsWith("ん")) return "n";
            char last = kana[kana.Length - 1];
            char full = "ぁぃぅぇぉゃゅょ".IndexOf(last) is int i && i >= 0 ? "あいうえおやゆよ"[i] : last;
            foreach (var row in Table.Values) {
                for (int v = 0; v < 5; ++v) {
                    if (row[v] == full.ToString()) return "aiueo"[v].ToString();
                }
            }
            return string.Empty;
        }

        string? Find(int tone, params string[] aliases) {
            foreach (var alias in aliases) {
                if (singer != null && singer.TryGetMappedOto(alias, tone, string.Empty, out var oto)) {
                    return oto.Alias;
                }
            }
            return null;
        }

        public override Result Process(Note[] notes, Note? prev, Note? next, Note? prevNeighbour, Note? nextNeighbour, Note[] prevNeighbours) {
            var note = notes[0];
            var lyric = (string.IsNullOrEmpty(note.phoneticHint) ? note.lyric : note.phoneticHint).Normalize();
            var vowel = string.Empty;
            if (prevNeighbour != null) {
                var before = prevNeighbour.Value.lyric.Normalize();
                var kana = ToKana(before).LastOrDefault() ?? before;
                if (!string.IsNullOrEmpty(kana)) vowel = TailVowel(kana);
            }

            // 声库已有的别名优先
            bool direct = Find(note.tone, lyric, $"- {lyric}", $"{vowel} {lyric}") != null;
            var morae = direct ? new List<string>() : ToKana(lyric);
            if (morae.Count == 0) morae.Add(lyric);

            int duration = notes.Sum(n => n.duration);
            var phonemes = new List<Phoneme>();
            int position = 0;
            for (int i = 0; i < morae.Count; ++i) {
                var mora = morae[i];
                var vcv = vowel.Length > 0 ? Find(note.tone, $"{vowel} {mora}") : null;
                if (vcv == null && i > 0 && Table.FirstOrDefault(r => r.Value.Contains(mora)).Key is { Length: > 0 } c) {
                    // CVVC：在音节之间插入 VC
                    var vc = Find(note.tone, $"{vowel} {c.ToLowerInvariant()}");
                    if (vc != null) {
                        int length = Math.Min(60, duration / (morae.Count * 3));
                        phonemes.Add(new Phoneme { phoneme = vc, position = position - length });
                    }
                }
                var alias = vcv ?? (vowel.Length == 0 ? Find(note.tone, $"- {mora}", mora)
                    : Find(note.tone, mora, $"- {mora}")) ?? mora;
                phonemes.Add(new Phoneme { phoneme = alias, position = position });
                vowel = TailVowel(mora);
                // 为结尾的弱音节保留较短时值
                int rest = morae.Count - i - 1;
                if (rest > 0) {
                    bool nextIsLastWeak = rest == 1 && "すくぷふむるぐぶ".Contains(morae[i + 1])
                        && !lyric.EndsWith("u", StringComparison.OrdinalIgnoreCase);
                    position = nextIsLastWeak ? duration - Math.Min(80, (duration - position) / 3)
                        : position + (duration - position) / (rest + 1);
                }
            }
            // CVVC 跨音符过渡：在本音符末尾加入下一音节的 VC
            if (nextNeighbour != null && vowel.Length > 0) {
                var after = nextNeighbour.Value.lyric.Normalize();
                var first = Find(nextNeighbour.Value.tone, after) != null ? after
                    : ToKana(after).FirstOrDefault() ?? after;
                if (Find(note.tone, $"{vowel} {first}") == null
                    && Table.FirstOrDefault(r => r.Value.Contains(first)).Key is { Length: > 0 } nc) {
                    var vc = Find(note.tone, $"{vowel} {nc.ToLowerInvariant()}");
                    int at = duration - Math.Min(60, duration / 3);
                    if (vc != null && at > position) {
                        phonemes.Add(new Phoneme { phoneme = vc, position = at });
                    }
                }
            }
            return new Result { phonemes = phonemes.ToArray() };
        }
    }
}
