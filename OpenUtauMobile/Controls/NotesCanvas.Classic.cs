using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;

namespace OpenUtauMobile.Controls;

public partial class NotesCanvas
{
    private Point? _heldPoint;
    private IPointer? _heldPointer;

    /// <summary>
    /// 仅跟踪触控笔或手指按住位置，不改变原有手势处理。
    /// </summary>
    private void InitClassicHold()
    {
        AddHandler(PointerPressedEvent, (_, e) =>
        {
            if (!ClassicUtauStyle.Enabled || e.Pointer.Type == PointerType.Mouse || _heldPointer != null) return;
            _heldPointer = e.Pointer;
            _heldPoint = e.GetPosition(this);
            InvalidateVisual();
        }, RoutingStrategies.Tunnel, true);
        AddHandler(PointerMovedEvent, (_, e) =>
        {
            if (e.Pointer != _heldPointer) return;
            _heldPoint = e.GetPosition(this);
            InvalidateVisual();
        }, RoutingStrategies.Tunnel, true);
        AddHandler(PointerReleasedEvent, (_, e) => ReleaseClassicHold(e.Pointer),
            RoutingStrategies.Tunnel, true);
        AddHandler(PointerCaptureLostEvent, (_, e) => ReleaseClassicHold(e.Pointer),
            RoutingStrategies.Direct | RoutingStrategies.Bubble, true);
    }

    private void ReleaseClassicHold(IPointer pointer)
    {
        if (pointer != _heldPointer) return;
        _heldPointer = null;
        _heldPoint = null;
        InvalidateVisual();
    }

    /// <summary>
    /// 经典音符：矩形、青色底线、像素歌词及上方音量数值。
    /// </summary>
    private void RenderClassicNote(UNote note, DrawingContext context)
    {
        if (ViewModel == null || Part == null) return;
        Point leftTop = ViewModel.TickPitchToPoint(note.position + Part.position, note.AdjustedTone);
        Size size = ViewModel.TickToneToSize(note.duration, 1);
        Rect rect = new(Math.Round(leftTop.X) + 0.5, Math.Round(leftTop.Y) + 0.5,
            Math.Max(1, Math.Round(size.Width)), Math.Max(1, Math.Round(size.Height)));
        bool held = _heldPoint is { } p && rect.Contains(p);

        using (context.PushOpacity(note.Error ? 0.35 : 1))
        {
            context.DrawRectangle(held ? ClassicUtauStyle.Held : ClassicUtauStyle.NoteFill,
                ClassicUtauStyle.NotePen, rect);
            context.DrawRectangle(ClassicUtauStyle.Cyan, null,
                new Rect(rect.X, rect.Bottom - 2, rect.Width, 3));
        }

        if (size.Height < 10) return;
        double fontSize = size.Height >= 32 ? 32 : 16;
        if (size.Height < 16) fontSize = size.Height;
        if (size.Width >= fontSize)
        {
            ClassicUtauStyle.DrawText(context, note.lyric, rect.X + 2,
                rect.Y + (rect.Height - fontSize * 1.4) / 2, fontSize, ClassicUtauStyle.Text);
        }

        if (size.Width >= 24)
        {
            // 取首个音素的音量；无音素时显示默认值 100
            var values = note.GetExpression(DocManager.Inst.Project,
                DocManager.Inst.Project.tracks[Part.trackNo], OpenUtau.Core.Format.Ustx.VOL);
            float volume = values.Count > 0 ? values[0].Item1 : 100;
            ClassicUtauStyle.DrawText(context, volume.ToString("0"), rect.X + 3, rect.Y - 18, 12,
                ClassicUtauStyle.Cyan);
        }
    }
}
