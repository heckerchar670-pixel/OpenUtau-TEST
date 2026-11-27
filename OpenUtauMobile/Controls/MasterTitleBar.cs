using System;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using OpenUtau.Core;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;
using OpenUtauMobile.ViewModels;

namespace OpenUtauMobile.Controls;

/// <summary>
/// 复刻旧版（master）编辑页标题栏：返回、工程路径、保存、撤销、重做、更多。
/// </summary>
public class MasterTitleBar : UserControl
{
    public MasterTitleBar()
    {
        IsVisible = false;
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
        // Master 主题下隐藏 dev 顶栏，其他主题保持显示
        Border? devBar = (Parent as Panel)?.Children.OfType<Border>()
            .FirstOrDefault(border => border.Classes.Contains("TopBar"));
        if (devBar != null) devBar.IsVisible = !IsVisible;
        if (!IsVisible || DataContext is not EditorViewModel viewModel) return;

        bool dark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
        IBrush ink = dark ? Brushes.White : Brushes.Black;
        Grid grid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto"),
            Height = 44,
            Background = Brush.Parse(dark ? "#303030" : "#D0D0D0"),
        };

        Button Make(object content, ICommand command) => new()
        {
            Content = content,
            Command = command,
            Width = 30,
            Height = 30,
            MinWidth = 30,
            MinHeight = 30,
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            Foreground = ink,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };

        PathIcon Icon(string data) => new() { Data = Geometry.Parse(data), Width = 20, Height = 20, Foreground = ink };

        Button back = Make(Icon("M20,11H7.83L13.42,5.41L12,4L4,12L12,20L13.41,18.59L7.83,13H20V11Z"), viewModel.BackCommand);
        back.Margin = new Thickness(6, 0);
        TextBlock path = new()
        {
            Text = DocManager.Inst.Project.FilePath,
            Foreground = ink,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
        Grid.SetColumn(path, 1);
        StackPanel right = new()
        {
            Orientation = Orientation.Horizontal, Spacing = 10, Margin = new Thickness(6, 0),
            VerticalAlignment = VerticalAlignment.Center,
        };
        right.Children.Add(Make(Icon("M17,3H3V21H21V7L17,3ZM12,19A3,3 0 1,1 12,13A3,3 0 1,1 12,19ZM15,9H5V5H15V9Z"), viewModel.SaveCommand));
        right.Children.Add(Make(Icon("M12.5,8C9.85,8 7.45,9 5.6,10.6L2,7V16H11L7.38,12.38C8.77,11.22 10.54,10.5 12.5,10.5C16.04,10.5 19.05,12.81 20.1,16L22.47,15.22C21.08,11.03 17.15,8 12.5,8Z"), viewModel.UndoCommand));
        right.Children.Add(Make(Icon("M18.4,10.6C16.55,9 14.15,8 11.5,8C6.85,8 2.92,11.03 1.54,15.22L3.9,16C4.95,12.81 7.95,10.5 11.5,10.5C13.45,10.5 15.23,11.22 16.62,12.38L13,16H22V7L18.4,10.6Z"), viewModel.RedoCommand));
        right.Children.Add(Make("…", viewModel.ShowMorePopupCommand));
        Grid.SetColumn(right, 2);

        grid.Children.Add(back);
        grid.Children.Add(path);
        grid.Children.Add(right);
        Content = grid;
    }
}
