using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime.Resources;

namespace OpenUtauMobile.Controls;

/// <summary>
/// UTAU 主题专用的经典钢琴卷帘样式，底色取自主题调色板。
/// </summary>
internal static class ClassicUtauStyle
{
    public static bool Enabled => LunaiThemes.IsRetro;
    private static bool Dark => Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

    public static IBrush WhiteRow => ThemeResources.GetBrush("Sem.Color.WhiteKey.Background");
    public static IBrush BlackRow => ThemeResources.GetBrush("Sem.Color.BlackKey.Background");
    public static IBrush NoteFill => ThemeResources.GetBrush("Sem.Color.SurfaceContainerHigh");
    public static IBrush Text => ThemeResources.GetBrush("Sem.Color.OnSurface");
    public static IPen RowPen => ThemeResources.GetPen("Sem.Color.OutlineVariant", 1);
    public static IPen OctavePen => ThemeResources.GetPen("Sem.Color.Outline", 1);
    public static IPen NotePen => ThemeResources.GetPen("Sem.Color.Outline", 1);

    // 经典强调色，深色模式使用对应的变体
    private static readonly IBrush CyanLight = Brush.Parse("#0088C0"), CyanDark = Brush.Parse("#40D8FF");
    private static readonly IBrush HeldLight = Brush.Parse("#7AA8FF"), HeldDark = Brush.Parse("#2F5FD0");
    private static readonly IPen PitchLight = new Pen(Brush.Parse("#0000D0"));
    private static readonly IPen PitchDark = new Pen(Brush.Parse("#80A0FF"));
    public static IBrush Cyan => Dark ? CyanDark : CyanLight;
    public static IBrush Held => Dark ? HeldDark : HeldLight;
    public static IPen PitchPen => Dark ? PitchDark : PitchLight;
    public static readonly IPen BarPen = new Pen(Brush.Parse("#FF66FF"));
    public static readonly IPen BeatPen = new Pen(Brush.Parse("#8080F0"));

    private static readonly Typeface PixelFace =
        new(new FontFamily("avares://OpenUtauMobile/Assets/Fonts/DotGothic16-Regular.ttf#DotGothic16"));

    /// <summary>
    /// 以像素字体绘制文本，坐标取整以保持清晰。
    /// </summary>
    public static void DrawText(DrawingContext context, string text, double x, double y, double size, IBrush brush)
    {
        using TextLayout layout = new(text, PixelFace, size, brush);
        layout.Draw(context, new Point(Math.Round(x), Math.Round(y)));
    }
}
