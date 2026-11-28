using System;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;

namespace OpenUtauMobile.Themes.OpenUtauMobile.Tokens.Foundation;

/// <summary>已使用的过渡时长</summary>
public static class MotionTokens
{
    // Lunai: transiciones mas cortas (respuesta inmediata, estilo DAW); los demas temas no cambian
    private static bool Studio => LunaiThemes.Current == LunaiThemes.Lunai;
    public static TimeSpan DurationShort => TimeSpan.FromMilliseconds(Studio ? 120 : 150);
    public static TimeSpan DurationExit => TimeSpan.FromMilliseconds(Studio ? 130 : 167);
    public static TimeSpan DurationMedium => TimeSpan.FromMilliseconds(Studio ? 160 : 200);
    public static TimeSpan DurationProgress => TimeSpan.FromMilliseconds(500);
    public static TimeSpan DurationBase => Studio ? TimeSpan.Parse("00:00:00.240") : TimeSpan.Parse("00:00:00.300");
    public static TimeSpan DurationLargeMove => Studio ? TimeSpan.Parse("00:00:00.300") : TimeSpan.Parse("00:00:00.375");
    public static TimeSpan DurationMedium1 => Studio ? TimeSpan.Parse("00:00:00.200") : TimeSpan.Parse("00:00:00.250");
}
