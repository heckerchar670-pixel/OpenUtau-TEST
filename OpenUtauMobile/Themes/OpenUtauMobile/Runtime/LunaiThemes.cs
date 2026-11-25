using System;
using System.Collections.Generic;
using Avalonia.Media;
using OpenUtau.Core.Util;

namespace OpenUtauMobile.Themes.OpenUtauMobile.Runtime;

/// <summary>Los tres temas de LUNAI. Cada uno tiene version clara y oscura.</summary>
public static class LunaiThemes
{
    // Valores guardados en Preferences.Default.ThemeColorMode (0 y 1 son los antiguos).
    public const int OpenUtau = 2;
    public const int Lunai = 3;
    public const int Utau = 4;

    private static readonly Color OpenUtauSeed = Color.Parse("#4EA6EA");
    private static readonly Color LunaiSeed = Color.Parse("#7466EE");
    private static readonly Color UtauSeed = Color.Parse("#6A00D5");

    public static int Current
    {
        get
        {
            int mode = Preferences.Default.ThemeColorMode;
            return mode == OpenUtau || mode == Utau ? mode : Lunai;
        }
    }

    public static bool IsRetro => Current == Utau;

    public static Color CurrentSeed => Current switch
    {
        OpenUtau => OpenUtauSeed,
        Utau => UtauSeed,
        _ => LunaiSeed,
    };

    // Orden: fondo, superficie, contenedor, contenedorAlto, texto, texto2, borde, bordeSuave,
    // primario, sobrePrimario, contPrimario, sobreContPrimario,
    // secundario, sobreSecundario, contSecundario, sobreContSecundario, teclaBlanca, teclaNegra
    private static readonly Dictionary<string, string> Palettes = new()
    {
        ["OpenUtauLight"] = "FFFFFF FFFFFF F0F0F0 E6E6E6 202020 555555 8A8A8A D4D4D4 2E86CF FFFFFF CFE7FA 0B3A5E D6336C FFFFFF FFD9E6 6B0F31 FFFFFF FF72A2",
        ["OpenUtauDark"] = "222222 2A2A2A 333333 3D3D3D EEEEEE BBBBBB 8A8A8A 4A4A4A 4EA6EA 06263F 1E4D70 D6ECFF FF72A2 3F0A1F 6E2A44 FFD9E6 D8D8D8 B04A70",
        ["LunaiLight"] = "FAF8FF FFFFFF F0ECFC E6E0F8 1E1A2E 4E4866 7D7799 D5CEEC 5B4BD6 FFFFFF E4DFFF 1B0F6B 7A68C8 FFFFFF E8E2FF 2A1C70 FFFFFF 7466EE",
        ["LunaiDark"] = "1E1E1E 252525 2C2B31 36343E F0EEF8 BEB9D0 8B86A0 45424F 8C7CF5 140A52 3F3590 E6E1FF A9A0E0 1F1850 4A4570 ECE9FF 7466EE 2A2A2A",
        ["UtauLight"] = "F0F0F0 F0F0F0 E1E1E1 D4D0C8 000000 333333 6E6E6E ACACAC 6A00D5 FFFFFF CCCCFF 240052 C0307A FFFFFF FFCCF0 4A0A30 FFFFFF 303030",
        ["UtauDark"] = "2B2B30 34343A 3E3E46 4A4A54 F0F0F0 C8C8D0 9A9AA4 5C5C68 B98CFF 22004D 4C3A7A EADDFF FF8AD8 45002E 70305C FFDCF2 C8C8C8 18181C",
    };

    public static void Apply(Color seed, bool dark, Dictionary<string, Color> colors)
    {
        string name;
        if (seed == OpenUtauSeed) name = "OpenUtau";
        else if (seed == LunaiSeed) name = "Lunai";
        else if (seed == UtauSeed) name = "Utau";
        else return;

        string[] parts = Palettes[name + (dark ? "Dark" : "Light")].Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Color C(int index) => Color.Parse("#" + parts[index]);
        void Set(Color value, params string[] keys)
        {
            foreach (string key in keys) colors["Sem.Color." + key] = value;
        }

        Set(C(0), "Background", "SurfaceContainerLowest");
        Set(C(1), "Surface", "SurfaceBright", "SurfaceContainerLow");
        Set(C(2), "SurfaceContainer", "SurfaceVariant", "SurfaceDim");
        Set(C(3), "SurfaceContainerHigh", "SurfaceContainerHighest");
        Set(C(4), "OnBackground", "OnSurface", "InverseSurface");
        Set(C(0), "InverseOnSurface");
        Set(C(5), "OnSurfaceVariant");
        Set(C(6), "Outline");
        Set(C(7), "OutlineVariant");
        Set(C(8), "Primary", "Success");
        Set(C(9), "OnPrimary");
        Set(C(10), "PrimaryContainer", "PrimaryHover", "CenterKey", "SuccessContainer");
        Set(C(11), "OnPrimaryContainer");
        Set(C(10), "PrimaryPressed", "InversePrimary");
        Set(C(12), "Secondary", "Tertiary");
        Set(C(13), "OnSecondary", "OnTertiary");
        Set(C(14), "SecondaryContainer", "TertiaryContainer");
        Set(C(15), "OnSecondaryContainer", "OnTertiaryContainer");
        Set(C(16), "WhiteKey");
        Set(C(17), "BlackKey");
        Set(C(1), "WhiteKey.Background");
        Set(C(2), "BlackKey.Background");
        Color surface = C(1);
        Set(Color.FromArgb(0x99, surface.R, surface.G, surface.B), "GlassOverlay");
    }
}
