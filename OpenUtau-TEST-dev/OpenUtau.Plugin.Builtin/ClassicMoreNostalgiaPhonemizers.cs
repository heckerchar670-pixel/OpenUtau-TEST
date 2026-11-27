using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using OpenUtau.Api;

namespace OpenUtau.Plugin.Builtin {
    /// <summary>
    /// 按拼写规则把各语言近似转写为日语假名。
    /// </summary>
    public abstract class ClassicRuleNostalgiaPhonemizer : ClassicNostalgiaPhonemizer {
        // 每条规则为（正则, 替换）；大写字母是内部辅音代码
        protected abstract (string, string)[] Rules { get; }

        protected override List<string> ToKana(string lyric) {
            var s = lyric.ToLowerInvariant();
            if (!Regex.IsMatch(s, "[a-zß-ÿ]")) return new List<string>();
            foreach (var (from, to) in Rules) {
                s = Regex.Replace(s, from, to);
            }
            return ToMorae(s);
        }
    }

    [Phonemizer("CLASSIC Italian to Japanese Phonemizer", "CLASSIC IT", language: "IT")]
    public class ClassicItalianNostalgiaPhonemizer : ClassicRuleNostalgiaPhonemizer {
        protected override (string, string)[] Rules => new[] {
            ("[àá]", "a"), ("[èé]", "e"), ("[ìí]", "i"), ("[òó]", "o"), ("[ùú]", "u"),
            ("sci(?=[aou])", "S"), ("sc(?=[ei])", "S"), ("gn", "N"), ("gli(?=[aeou])", "ry"), ("gli", "ri"),
            ("ch", "k"), ("gh", "G"), ("ci(?=[aou])", "C"), ("c(?=[ei])", "C"),
            ("gi(?=[aou])", "J"), ("g(?=[ei])", "J"), ("h", ""), ("qu", "ku"), ("c|q", "k"),
            ("v", "b"), ("l", "r"), ("j", "y"), ("z", "s"), ("x", "ks"), ("G", "g"),
        };
    }

    [Phonemizer("CLASSIC Portuguese to Japanese Phonemizer", "CLASSIC PT", language: "PT")]
    public class ClassicPortugueseNostalgiaPhonemizer : ClassicRuleNostalgiaPhonemizer {
        protected override (string, string)[] Rules => new[] {
            ("ã", "an"), ("õ", "on"), ("[áàâ]", "a"), ("[éê]", "e"), ("í", "i"), ("[óô]", "o"), ("[úü]", "u"),
            ("ç", "s"), ("lh", "ry"), ("nh", "N"), ("ch", "S"), ("rr", "H"), ("ss", "s"),
            ("qu(?=[ei])", "k"), ("gu(?=[ei])", "G"), ("c(?=[ei])", "s"), ("g(?=[ei])", "J"), ("j", "J"),
            ("h", ""), ("H", "h"), ("qu", "ku"), ("c|q", "k"), ("m$", "n"),
            ("v", "b"), ("l", "r"), ("x", "S"), ("G", "g"),
        };
    }

    [Phonemizer("CLASSIC German to Japanese Phonemizer", "CLASSIC DE", language: "DE")]
    public class ClassicGermanNostalgiaPhonemizer : ClassicRuleNostalgiaPhonemizer {
        protected override (string, string)[] Rules => new[] {
            ("äu|eu", "oi"), ("[äö]", "e"), ("ü", "u"), ("ß", "s"), ("j", "y"),
            ("tsch", "C"), ("sch", "S"), ("^s(?=[pt])", "S"), ("ch", "h"), ("ck", "k"),
            ("ei", "ai"), ("ie", "i"), ("qu", "kb"), ("v", "f"), ("w", "b"),
            ("c|q", "k"), ("x", "ks"), ("l", "r"), ("z", "s"),
        };
    }

    [Phonemizer("CLASSIC Korean to Japanese Phonemizer", "CLASSIC KO", language: "KO")]
    public class ClassicKoreanNostalgiaPhonemizer : ClassicNostalgiaPhonemizer {
        static readonly string[] Initials = "g,k,n,d,t,r,m,b,p,s,s,,J,J,C,k,t,p,h".Split(',');
        static readonly string[] Vowels = "a,e,ya,ye,o,e,yo,ye,o,wa,we,we,yo,u,wo,we,wi,yu,u,ui,i".Split(',');
        static readonly string[] Finals = ",k,k,k,n,n,n,t,r,r,r,r,r,r,r,r,m,p,p,t,t,n,t,t,k,t,p,t".Split(',');

        protected override List<string> ToKana(string lyric) {
            var text = new StringBuilder();
            foreach (char ch in lyric.Normalize()) {
                int code = ch - 0xAC00;
                if (code < 0 || code >= 11172) continue; // 仅处理谚文音节
                var initial = Initials[code / 588];
                var vowel = Vowels[code % 588 / 28];
                if (initial.Length > 0 && vowel[0] == 'y') {
                    vowel = initial is "J" or "C" ? vowel.Substring(1) : "i" + vowel;
                }
                text.Append(initial).Append(vowel).Append(Finals[code % 28]);
            }
            return ToMorae(text.ToString());
        }
    }
}
