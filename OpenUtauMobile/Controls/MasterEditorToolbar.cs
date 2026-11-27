using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;
using OpenUtauMobile.ViewModels;

namespace OpenUtauMobile.Controls;

/// <summary>
/// 按旧版（master）编辑页的按钮顺序排列 dev 现有功能，仅在 Master 主题显示。
/// </summary>
public class MasterEditorToolbar : UserControl
{
    private readonly StackPanel _row = new() { Orientation = Orientation.Horizontal, Spacing = 4 };

    public MasterEditorToolbar()
    {
        // 横向滚动适配手机竖屏；顶部留白避开分隔手柄
        Content = new ScrollViewer
        {
            Content = _row,
            Margin = new Thickness(4, 16, 4, 4),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Build();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Build();
    }

    private void Build()
    {
        IsVisible = LunaiThemes.Current == LunaiThemes.Master;
        _row.Children.Clear();
        if (!IsVisible || DataContext is not EditorViewModel viewModel) return;
        PianoRollViewModel pianoRoll = viewModel.PianoRollViewModel;

        Add("Back", viewModel.BackCommand);
        Add("Save", viewModel.SaveCommand);
        Add("Undo", viewModel.UndoCommand);
        Add("Redo", viewModel.RedoCommand);
        Add("More", viewModel.ShowMorePopupCommand);
        Add("Tracks", viewModel.ToggleTrackHeaderCommand);
        Add("BPM", viewModel.EditBpmCommand);
        Add("Beat", viewModel.EditTimeSignatureCommand);
        Add("Key", viewModel.EditKeyCommand);
        Add("+Track", viewModel.AddTrackCommand);
        AddAction("Hand", () => pianoRoll.EditMode = PianoRollEditMode.Hand);
        AddAction("Note", () => pianoRoll.EditMode = PianoRollEditMode.Note);
        AddAction("Pitch", () => pianoRoll.EditMode = PianoRollEditMode.PitchPen);
        AddAction("Zoom -", () => Zoom(pianoRoll, 0.8));
        AddAction("Zoom +", () => Zoom(pianoRoll, 1.25));
        AddAction("Lyrics", () => _ = pianoRoll.ShowBulkLyricEditPopupAsync());
        AddAction("Metronome", () => viewModel.IsMetronomeEnabled = !viewModel.IsMetronomeEnabled);
        AddAction("Loop", () => viewModel.IsRepeatEnabled = !viewModel.IsRepeatEnabled);
        Add("Play", viewModel.PlayPauseCommand);
        Add("Stop", viewModel.StopCommand);
    }

    private static void Zoom(PianoRollViewModel pianoRoll, double factor) =>
        pianoRoll.TickWidth = Math.Clamp(pianoRoll.TickWidth * factor,
            ViewConstants.PianoRollTickWidthMin, ViewConstants.PianoRollTickWidthMax);

    private Button Add(string text, ICommand? command = null)
    {
        Button button = new() { Content = text, Command = command, Height = 40, MinWidth = 48 };
        _row.Children.Add(button);
        return button;
    }

    private void AddAction(string text, Action action) => Add(text).Click += (_, _) => action();
}
