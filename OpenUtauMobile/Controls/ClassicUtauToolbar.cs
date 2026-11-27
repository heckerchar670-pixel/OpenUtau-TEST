using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using OpenUtau.Core;
using OpenUtau.Core.Ustx;
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
    private readonly Button _length;
    private readonly TextBlock _bank = new() { FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
    private static readonly int[] LengthValues = [0, 1, 2, 4, 8, 16, 32];

    public ClassicUtauToolbar()
    {
        // 自动换行容器，适配手机竖屏宽度
        WrapPanel panel = new() { Margin = new Thickness(2) };

        Button Add(object text, Action? click = null)
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
        _length = Add("Length Auto", NextLength);
        _select = Add(Icon("M2,0 L2,14 L6,10 L8,15 L11,14 L9,9 L14,9 Z"), () => SetMode(PianoRollEditMode.Hand));
        _pen = Add(Icon("M0,16 L0,12 L10,2 L14,6 L4,16 Z M11,1 L12,0 L16,4 L15,5 Z"), () => SetMode(PianoRollEditMode.Note));
        _pitch = Add(Icon("M0,9 H3 V5 H6 V9 H9 V13 H12 V9 H16 V11 H14 V15 H7 V11 H5 V7 H4 V11 H0 Z"), () => SetMode(PianoRollEditMode.PitchPen));
        Add(Icon("M0,6 H14 V9 H0 Z"), () => Zoom(0.8));
        Add(Icon("M5,0 H9 V5 H14 V9 H9 V14 H5 V9 H0 V5 H5 Z"), () => Zoom(1.25));
        _play = Add(Icon("M2,0 L14,7 L2,14 Z"));
        _stop = Add(Icon("M1,1 H13 V13 H1 Z"));
        _undo = Add(Icon("M0,6 L6,0 V4 H12 V6 H14 V14 H5 V11 H11 V8 H6 V12 Z"));
        _redo = Add(Icon("M14,6 L8,0 V4 H2 V6 H0 V14 H9 V11 H3 V8 H8 V12 Z"));
        Add(Icon("M9,0 H12 L16,4 V7 L12,4 H11 V14 H9 V16 H4 V14 H2 V11 H4 V9 H9 Z"), () => _ = _viewModel?.PianoRollViewModel.ShowBulkLyricEditPopupAsync());
        panel.Children.Add(_bank);
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
            or nameof(PianoRollViewModel.EditMode)
            or nameof(PianoRollViewModel.EditingVoicePart))
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
        _bank.Text = DescribeBank(_viewModel.PianoRollViewModel.EditingVoicePart);
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

    private static PathIcon Icon(string data) =>
        new() { Data = Geometry.Parse(data), Width = 14, Height = 14 };

    private void NextLength()
    {
        if (_viewModel == null) return;
        PianoRollViewModel pianoRoll = _viewModel.PianoRollViewModel;
        int index = Array.IndexOf(LengthValues, pianoRoll.ClassicNoteLength);
        int next = LengthValues[(index + 1) % LengthValues.Length];
        pianoRoll.ClassicNoteLength = next;
        _length.Content = next == 0 ? "Length Auto" : $"Length L{next}";
    }

    /// <summary>
    /// 只读检测传统声库的录音类型。
    /// </summary>
    private static string DescribeBank(UVoicePart? part)
    {
        var tracks = DocManager.Inst.Project.tracks;
        if (part == null || part.trackNo >= tracks.Count) return string.Empty;
        USinger? singer = tracks[part.trackNo].Singer;
        if (singer?.Otos == null || singer.SingerType != USingerType.Classic) return string.Empty;
        int vcv = singer.Otos.Count(o => Regex.IsMatch(o.Alias, "^[aiueon] [ぁ-ん]"));
        int vc = singer.Otos.Count(o => Regex.IsMatch(o.Alias, "^[aiueon] [a-z]{1,3}"));
        return "UTAU CLASSIC " + (vcv > 20 ? "VCV" : vc > 20 ? "CVVC" : "CV");
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
