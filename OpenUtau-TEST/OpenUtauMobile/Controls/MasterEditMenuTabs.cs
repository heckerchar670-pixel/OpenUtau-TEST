using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using OpenUtauMobile.ViewModels;

namespace OpenUtauMobile.Controls;

/// <summary>
/// 复刻旧版（master）EditMenuPopup 的伪选项卡：导入、导出、辅助工具、更多。
/// 仅在 OldUI 主题下显示，命令全部转发给 EditorMoreViewModel.ConfirmCommand。
/// </summary>
public class MasterEditMenuTabs : UserControl
{
    private readonly List<Panel> _pages = [];

    public MasterEditMenuTabs()
    {
        // 选项卡头：四个宽 100 的透明按钮，可横向滚动
        StackPanel header = new() { Orientation = Orientation.Horizontal };
        // 选项卡内容区：固定高度 500，与旧版一致
        Panel body = new() { Height = 500 };

        AddTab(header, body, "Import", "Import",
            Row("Import audio", EditorMoreAction.ImportAudio),
            Row("Import MIDI", null),
            Row("Import Track", EditorMoreAction.ImportTrack));
        AddTab(header, body, "Export", "Export",
            Row("Export audio", EditorMoreAction.ExportAudio),
            Row("Export Project", null));
        AddTab(header, body, "Auxiliary Tools", "Auxiliary Tools",
            Row("Separate Stems", null),
            Row("Detect Tempo", null));
        AddTab(header, body, "More", "More",
            Row("Save Project As", EditorMoreAction.SaveAs),
            Row("Manage Expressions", EditorMoreAction.ExpressionGraphs),
            Row("Settings", null));

        ShowPage(0);
        Content = new StackPanel
        {
            Children =
            {
                new ScrollViewer
                {
                    HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
                    Height = 50,
                    Content = header,
                },
                body,
            },
        };
    }

    private void AddTab(StackPanel header, Panel body, string key, string title, params Control[] rows)
    {
        int index = _pages.Count;
        StackPanel page = new();
        foreach (Control row in rows) page.Children.Add(row);
        Panel host = new() { Children = { new ScrollViewer { Content = page } } };
        _pages.Add(host);
        body.Children.Add(host);
        Button tab = new()
        {
            Content = title,
            Width = 100,
            Height = 50,
            Background = Brushes.Transparent,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        tab.Click += (_, _) => ShowPage(index);
        header.Children.Add(tab);
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < _pages.Count; i++) _pages[i].IsVisible = i == index;
    }

    /// <summary>
    /// 一行菜单项；旧版中没有对应功能的行保持可见但不可用。
    /// </summary>
    private static Control Row(string text, EditorMoreAction? action)
    {
        Button button = new()
        {
            Content = text,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            MinHeight = 48,
            Padding = new Thickness(16, 0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            IsEnabled = action != null,
        };
        if (action != null)
        {
            button.Bind(Button.CommandProperty, new Binding("ConfirmCommand"));
            button.CommandParameter = action.Value;
        }
        return button;
    }
}
