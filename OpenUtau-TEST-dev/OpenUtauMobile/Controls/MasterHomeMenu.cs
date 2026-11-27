using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;
using OpenUtauMobile.ViewModels;

namespace OpenUtauMobile.Controls;

/// <summary>
/// 旧版（master）首页菜单，仅在对应主题下显示，样式跟随当前主题。
/// </summary>
public class MasterHomeMenu : UserControl
{
    private readonly UniformGrid _grid = new() { Columns = 2, Margin = new Thickness(8) };

    public MasterHomeMenu()
    {
        Content = _grid;
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
        _grid.Children.Clear();
        if (!IsVisible || DataContext is not HomeViewModel viewModel) return;
        Add("New", viewModel.NewCommand);
        Add("Open", viewModel.OpenCommand);
        Add("Singers", viewModel.SingersCommand);
        Add("Options", viewModel.OptionsCommand);
    }

    private void Add(string text, ICommand command) => _grid.Children.Add(new Button
    {
        Content = text,
        Command = command,
        Height = 56,
        Margin = new Thickness(4),
        HorizontalAlignment = HorizontalAlignment.Stretch,
        HorizontalContentAlignment = HorizontalAlignment.Center,
        VerticalContentAlignment = VerticalAlignment.Center,
    });
}
