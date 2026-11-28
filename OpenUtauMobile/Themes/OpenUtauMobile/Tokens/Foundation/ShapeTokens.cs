using Avalonia;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;

namespace OpenUtauMobile.Themes.OpenUtauMobile.Tokens.Foundation;

/// <summary>圆角刻度</summary>
public static class ShapeTokens
{
    /// <summary>MD3 全圆角令牌；有限大半径由绘制后端按边界归一化，避免浮点转换溢出。</summary>
    public static CornerRadius CornerFull => new(LunaiThemes.IsRetro ? 0 : 9999);
    // Lunai usa radios mas contenidos (aspecto de estudio); los demas temas conservan los valores originales
    private static bool Studio => LunaiThemes.Current == LunaiThemes.Lunai;
    public static CornerRadius CornerS => new(LunaiThemes.IsRetro ? 0 : Studio ? 3 : 4);
    public static CornerRadius CornerM => new(LunaiThemes.IsRetro ? 0 : Studio ? 6 : 8);
    public static CornerRadius CornerL => new(LunaiThemes.IsRetro ? 0 : Studio ? 8 : 12);
    public static CornerRadius CornerXL => new(LunaiThemes.IsRetro ? 0 : Studio ? 10 : 16);
    public static CornerRadius CornerXXXL => new(LunaiThemes.IsRetro ? 0 : Studio ? 14 : 24);
    public static CornerRadius CornerXXXXXL => new(LunaiThemes.IsRetro ? 0 : 32);
}
