using OpenUtau.Core;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;

namespace OpenUtauMobile.ViewModels;

public partial class PianoRollViewModel
{
    /// <summary>
    /// 经典工具栏选择的新音符时值（几分音符），0 表示跟随量化。
    /// </summary>
    public int ClassicNoteLength { get; set; }

    private int ApplyClassicLength(int duration) => LunaiThemes.IsRetro && ClassicNoteLength > 0
        ? DocManager.Inst.Project.resolution * 4 / ClassicNoteLength
        : duration;
}
