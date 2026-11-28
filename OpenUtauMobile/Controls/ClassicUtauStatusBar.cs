using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using OpenUtau.Core.Util;
using OpenUtauMobile.ViewModels;

namespace OpenUtauMobile.Controls;

/// <summary>
/// UTAU 经典风格的底部状态栏：曲速、拍号、选中音符数量以及首个选中音符的歌词与音高。
/// 仅在 UTAU 主题下显示。
/// </summary>
public class ClassicUtauStatusBar : UserControl
{
    private readonly TextBlock _text = new() { Margin = new Thickness(6, 2) };
    private EditorViewModel? _viewModel;

    public ClassicUtauStatusBar()
    {
        IsVisible = ClassicUtauStyle.Enabled;
        // 凹陷边框，模仿旧版 Windows 状态栏
        Content = new Border
        {
            BorderThickness = new Thickness(0, 1, 0, 0),
            BorderBrush = Brushes.Gray,
            Child = _text,
        };
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        Detach();
        _viewModel = DataContext as EditorViewModel;
        if (_viewModel == null) return;
        _viewModel.PropertyChanged += OnChanged;
        _viewModel.PianoRollViewModel.SelectedNotes.CollectionChanged += OnSelectionChanged;
        Refresh();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Detach();
        base.OnDetachedFromVisualTree(e);
    }

    private void Detach()
    {
        if (_viewModel == null) return;
        _viewModel.PropertyChanged -= OnChanged;
        _viewModel.PianoRollViewModel.SelectedNotes.CollectionChanged -= OnSelectionChanged;
        _viewModel = null;
    }

    private void OnChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(EditorViewModel.ProjectBpm) or nameof(EditorViewModel.ProjectTimeSignature))
            Refresh();
    }

    private void OnSelectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Refresh();

    private void Refresh()
    {
        if (_viewModel == null || !IsVisible) return;
        var selected = _viewModel.PianoRollViewModel.SelectedNotes;
        string note = selected.Count == 0
            ? "-"
            : $"{selected[0].lyric} {MusicMath.GetToneName(selected[0].tone)}";
        _text.Text = $"Tempo {_viewModel.ProjectBpm}   {_viewModel.ProjectTimeSignature}   Selected {selected.Count}   Note {note}";
    }
}
