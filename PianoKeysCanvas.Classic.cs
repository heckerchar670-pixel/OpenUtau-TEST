using System;
using Avalonia;
using Avalonia.Media;
using OpenUtau.Core;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime.Resources;

namespace OpenUtauMobile.Controls;

public partial class PianoKeysCanvas
{
    /// <summary>
    /// 经典 UTAU 键盘：白色底、左侧短黑键、仅在 C 音显示像素音名。
    /// </summary>
    private void RenderClassicKeys(DrawingContext context, int toneMin, int toneMax, double w, double h)
    {
        IBrush white = ThemeResources.GetBrush("Sem.Color.WhiteKey");
        IBrush black = ThemeResources.GetBrush("Sem.Color.BlackKey");
        IPen line = ThemeResources.GetPen("Sem.Color.Outline", 1);
        double blackWidth = Math.Round(w * 0.55);
        context.DrawRectangle(white, null, new Rect(0, 0, w, h));

        for (int tone = toneMax; tone >= toneMin; tone--)
        {
            double y = Math.Round((ViewConstants.MaxTone - 1 - tone - KeyOffset) * KeyHeight);
            bool isActive = _activeTones.Contains(tone);

            if (MusicMath.IsBlackKey(tone))
            {
                // 黑键右侧的线分隔相邻的两个白键
                double middle = Math.Round(y + KeyHeight / 2) + 0.5;
                context.DrawLine(line, new Point(blackWidth, middle), new Point(w, middle));
                context.DrawRectangle(isActive ? ClassicUtauStyle.Held : black, null,
                    new Rect(0, y, blackWidth, KeyHeight));
                continue;
            }

            if (isActive)
            {
                context.DrawRectangle(ClassicUtauStyle.Held, null, new Rect(0, y, w, KeyHeight));
            }

            // B–C 与 E–F 之间画整行分隔线
            if (tone > 0 && !MusicMath.IsBlackKey(tone - 1))
            {
                double bottom = Math.Round(y + KeyHeight) + 0.5;
                context.DrawLine(line, new Point(0, bottom), new Point(w, bottom));
            }

            if (tone % 12 != 0 || KeyHeight < ViewConstants.NoteHeightMin) continue;
            string name = PianoKeyLabelFormatter.GetLabel(tone, ProjectKey, LabelMode);
            if (string.IsNullOrEmpty(name)) continue;
            double size = Math.Floor(Math.Min(14, KeyHeight - 1));
            ClassicUtauStyle.DrawCentered(context, name, w - name.Length * size * 0.5 - 4,
                new Rect(0, y, w, KeyHeight), size, black);
        }
    }
}
