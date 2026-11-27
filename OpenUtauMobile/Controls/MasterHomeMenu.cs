using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using IconPacks.Avalonia.PhosphorIcons;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;
using OpenUtauMobile.ViewModels;

namespace OpenUtauMobile.Controls;

/// <summary>
/// 旧版（master）首页菜单：四个 60x60 圆角图标按钮，下方各带一行文字。仅在 OldUI 主题下显示。
/// </summary>
public class MasterHomeMenu : UserControl
{
    // 四个等宽列，窄屏也不会被裁掉
    private readonly Grid _row = new()
    {
        ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
        Margin = new Thickness(10),
    };

    public MasterHomeMenu()
    {
        Content = _row;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        Build();
    }

    protected override void OnDataContextChanged(System.EventArgs e)
    {
        base.OnDataContextChanged(e);
        Build();
    }

    private void Build()
    {
        IsVisible = LunaiThemes.Current == LunaiThemes.Master;
        _row.Children.Clear();
        if (!IsVisible || DataContext is not HomeViewModel viewModel) return;
        Add(PackIconPhosphorIconsKind.Plus, "Home.Action.New", viewModel.NewCommand);
        Add(PackIconPhosphorIconsKind.FolderOpen, "Home.Action.Open", viewModel.OpenCommand);
        Add(PackIconPhosphorIconsKind.MicrophoneStage, "Home.Action.Singers", viewModel.SingersCommand);
        Add(PackIconPhosphorIconsKind.List, "Home.Action.Options", viewModel.OptionsCommand);
    }

    private void Add(PackIconPhosphorIconsKind kind, string textKey, ICommand command)
    {
        TextBlock label = new()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
            Margin = new Thickness(0, 4, 0, 0),
        };
        label.Bind(TextBlock.TextProperty, this.GetResourceObservable(textKey));
        Button button = new()
        {
            Content = new PackIconPhosphorIcons { Kind = kind },
            Command = command,
            Width = 60,
            Height = 60,
            Padding = new Thickness(15),
            CornerRadius = new CornerRadius(10),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
        };
        StackPanel cell = new() { Width = 60, HorizontalAlignment = HorizontalAlignment.Center, Children = { button, label } };
        Grid.SetColumn(cell, _row.Children.Count);
        _row.Children.Add(cell);
    }
}
