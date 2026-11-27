using System.Collections.Generic;
using System.Linq;
using OpenUtau.Api;
using OpenUtau.Core.G2p;

namespace OpenUtau.Plugin.Builtin {
    /// <summary>
    /// 将英语歌词经 ARPAbet 近似转写为日语假名。
    /// </summary>
    [Phonemizer("CLASSIC English to Japanese Phonemizer", "CLASSIC EN", language: "EN")]
    public class ClassicEnglishNostalgiaPhonemizer : ClassicNostalgiaPhonemizer {
        static readonly Dictionary<string, string> Map = (
            "aa=a ae=a ah=a ao=o aw=au ay=ai eh=e er=a ey=ei ih=i iy=i ow=ou oy=oi uh=u uw=u " +
            "b=b ch=C d=d dh=d f=f g=g hh=h jh=J k=k l=r m=m n=n ng=n p=p r=r s=s sh=S t=t th=s " +
            "v=b w=w y=y z=z zh=J").Split(' ').Select(x => x.Split('=')).ToDictionary(x => x[0], x => x[1]);

        ArpabetG2p? g2p;

        protected override List<string> ToKana(string lyric) {
            g2p ??= new ArpabetG2p();
            var phones = g2p.Query(lyric.ToLowerInvariant());
            if (phones == null) return new List<string>();
            return ToMorae(string.Concat(phones.Select(p => Map.TryGetValue(p, out var v) ? v : string.Empty)));
        }
    }
}
