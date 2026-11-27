using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;

namespace OpenUtauMobile.Controls;

/// <summary>
/// 选择 OldUI 后覆盖整个设置页的全屏提示：请重启 OpenUtau。
/// </summary>
public class OldUiRestartOverlay : UserControl
{
    public OldUiRestartOverlay()
    {
        Button close = new()
        {
            Content = "Close OpenUtau",
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 24, 0, 0),
        };
        close.Click += (_, _) => Environment.Exit(0);

        Grid grid = new()
        {
            Children =
            {
                new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(32),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Please, restart OpenUtau",
                            FontSize = 24,
                            TextAlignment = Avalonia.Media.TextAlignment.Center,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        },
                        close,
                    },
                },
            },
        };
        grid.Bind(Panel.BackgroundProperty, this.GetResourceObservable("Sem.Color.Surface"));
        Content = grid;
    }
}
