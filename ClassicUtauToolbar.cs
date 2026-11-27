using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using OpenUtauMobile.ViewModels;

namespace OpenUtauMobile.Controls;

/// <summary>
/// UTAU 主题专用的经典工具栏。
/// </summary>
public class ClassicUtauToolbar : UserControl
{
    private static readonly int[] SnapValues = [-1, 4, 8, 16, 32, 64];
    private readonly Button _tempo, _snap, _select, _pen, _pitch, _play, _stop, _undo, _redo;
    private EditorViewModel? _viewModel;

    public ClassicUtauToolbar()
    {
        // 自动换行容器，适配手机竖屏宽度
        WrapPanel panel = new() { Margin = new Thickness(2) };

        Button Add(string text, Action? click = null)
        {
            Button button = new()
            {
                Content = text,
                Height = 32,
                MinWidth = 36,
                Padding = new Thickness(6, 0),
                Margin = new Thickness(1),
                FontSize = 12,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
            };
            if (click != null) button.Click += (_, _) => click();
            panel.Children.Add(button);
            return button;
        }

        _tempo = Add("Tempo");
        _snap = Add("Quantize", NextSnap);
        _select = Add("↖", () => SetMode(PianoRollEditMode.Hand));
        _pen = Add("Pen", () => SetMode(PianoRollEditMode.Note));
        _pitch = Add("Pitch", () => SetMode(PianoRollEditMode.PitchPen));
        Add("－", () => Zoom(0.8));
        Add("＋", () => Zoom(1.25));
        _play = Add("▶");
        _stop = Add("■");
        _undo = Add("Undo");
        _redo = Add("Redo");
        Add("Lyric", () => _ = _viewModel?.PianoRollViewModel.ShowBulkLyricEditPopupAsync());
        Content = panel;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        IsVisible = ClassicUtauStyle.Enabled;
        AttachViewModel(DataContext as EditorViewModel);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        AttachViewModel(null);
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        AttachViewModel(DataContext as EditorViewModel);
    }

    private void AttachViewModel(EditorViewModel? viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelChanged;
            _viewModel.PianoRollViewModel.PropertyChanged -= OnViewModelChanged;
        }

        _viewModel = viewModel;
        if (viewModel == null) return;
        viewModel.PropertyChanged += OnViewModelChanged;
        viewModel.PianoRollViewModel.PropertyChanged += OnViewModelChanged;
        _tempo.Command = viewModel.EditBpmCommand;
        _play.Command = viewModel.PlayPauseCommand;
        _stop.Command = viewModel.StopCommand;
        _undo.Command = viewModel.UndoCommand;
        _redo.Command = viewModel.RedoCommand;
        Refresh();
    }

    private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EditorViewModel.ProjectBpm) or nameof(EditorViewModel.SnapDiv)
            or nameof(PianoRollViewModel.EditMode))
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (_viewModel == null) return;
        _tempo.Content = $"Tempo {_viewModel.ProjectBpm}";
        int div = _viewModel.SnapDiv;
        _snap.Content = "Quantize " + (div == 0 ? "Off" : div == -2 ? "Auto3" : div < 0 ? "Auto" : $"L{div}");
        PianoRollEditMode mode = _viewModel.PianoRollViewModel.EditMode;
        // 当前编辑模式半透明显示为按下状态
        _select.Opacity = mode == PianoRollEditMode.Hand ? 0.55 : 1;
        _pen.Opacity = mode == PianoRollEditMode.Note ? 0.55 : 1;
        _pitch.Opacity = mode == PianoRollEditMode.PitchPen ? 0.55 : 1;
    }

    private void SetMode(PianoRollEditMode mode)
    {
        if (_viewModel != null) _viewModel.PianoRollViewModel.EditMode = mode;
    }

    private void NextSnap()
    {
        if (_viewModel == null) return;
        int index = Array.IndexOf(SnapValues, _viewModel.SnapDiv);
        _viewModel.SnapDiv = SnapValues[(index + 1) % SnapValues.Length];
    }

    private void Zoom(double factor)
    {
        if (_viewModel == null) return;
        PianoRollViewModel pianoRoll = _viewModel.PianoRollViewModel;
        pianoRoll.TickWidth = Math.Clamp(pianoRoll.TickWidth * factor,
            ViewConstants.PianoRollTickWidthMin, ViewConstants.PianoRollTickWidthMax);
    }
}
