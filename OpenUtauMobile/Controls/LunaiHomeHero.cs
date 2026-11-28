using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Shapes;
using Avalonia.Threading;
using OpenUtauMobile.Themes.OpenUtauMobile.Runtime;
using OpenUtauMobile.ViewModels;

namespace OpenUtauMobile.Controls;

/// <summary>
/// Lunai 主题首页的大卡片：标题、副标题、轨道装饰和“Create project”按钮。
/// 仅在 Lunai 主题且竖屏时显示，进入时淡入并上移。
/// </summary>
public class LunaiHomeHero : UserControl
{
    // 曲线 cubic-bezier(0.23, 1, 0.32, 1)，与原型一致
    private static readonly Easing EaseOut = new SplineEasing(0.23, 1, 0.32, 1);
    private readonly Border _card;
    private readonly Button _create;
    private bool _played;

    public LunaiHomeHero()
    {
        // 轨道装饰：两圈细线和一个亮点（对应原型的 orbit）
        Ellipse ring1 = Ring(174, 0.14);
        Ellipse ring2 = Ring(142, 0.10);
        Ellipse dot = new() { Width = 7, Height = 7, Fill = new SolidColorBrush(Color.Parse("#C9BAFF")) };
        Canvas art = new() { IsHitTestVisible = false, Width = 174, Height = 174, HorizontalAlignment = HorizontalAlignment.Right, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(0, -20, -40, 0) };
        art.Children.Add(ring1);
        Canvas.SetLeft(ring2, 16); Canvas.SetTop(ring2, 16);
        art.Children.Add(ring2);
        Canvas.SetLeft(dot, 132); Canvas.SetTop(dot, 28);
        art.Children.Add(dot);

        _create = new Button
        {
            Content = "Create project",
            Height = 44,
            Padding = new Thickness(16, 0),
            CornerRadius = new CornerRadius(8),
            FontWeight = FontWeight.Bold,
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Color.Parse("#C1B2FF"), 0), new GradientStop(Color.Parse("#8D79E8"), 1) },
            },
            Foreground = new SolidColorBrush(Color.Parse("#211A30")),
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 14, 0, 0),
        };

        StackPanel copy = new()
        {
            Spacing = 6,
            Children =
            {
                new TextBlock { Text = "THE ART OF SYNTHESIS", FontSize = 10, FontWeight = FontWeight.Bold, LetterSpacing = 2, Foreground = new SolidColorBrush(Color.Parse("#C9BCF4")) },
                new TextBlock { Text = "Bring your voice to life.", FontSize = 26, FontWeight = FontWeight.Bold, TextWrapping = TextWrapping.Wrap, MaxWidth = 230, LineHeight = 30 },
                new TextBlock { Text = "Shape melodies. Create voices.", FontSize = 12, Foreground = new SolidColorBrush(Color.Parse("#C1B8CF")) },
                _create,
            },
        };

        _card = new Border
        {
            CornerRadius = new CornerRadius(14),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#574B6D")),
            Padding = new Thickness(18),
            ClipToBounds = true,
            Background = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                GradientStops = { new GradientStop(Color.Parse("#332B42"), 0), new GradientStop(Color.Parse("#211D29"), 1) },
            },
            Child = new Grid { Children = { art, copy } },
            // 入场动画：初始透明并下移 9，由 Play() 恢复
            Opacity = 0,
            RenderTransform = TransformOperations.Parse("translateY(9px)"),
            Transitions = new Transitions
            {
                new DoubleTransition { Property = OpacityProperty, Duration = TimeSpan.FromMilliseconds(460), Easing = EaseOut },
                new TransformOperationsTransition { Property = RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(460), Easing = EaseOut },
            },
        };
        Content = _card;
        IsVisible = LunaiThemes.Current == LunaiThemes.Lunai;
    }

    private static Ellipse Ring(double size, double alpha) => new()
    {
        Width = size,
        Height = size,
        Stroke = new SolidColorBrush(Color.FromArgb((byte)(alpha * 255), 0xC4, 0xB2, 0xFF)),
        StrokeThickness = 1,
    };

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (_played || !IsVisible) return;
        _played = true;
        // 下一帧再改属性，过渡才会触发
        Dispatcher.UIThread.Post(() =>
        {
            _card.Opacity = 1;
            _card.RenderTransform = TransformOperations.Parse("translateY(0px)");
        }, DispatcherPriority.Background);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is HomeViewModel viewModel) _create.Command = viewModel.NewCommand;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        // 横屏空间不足时隐藏
        IsVisible = LunaiThemes.Current == LunaiThemes.Lunai
            && TopLevel.GetTopLevel(this) is { } top && top.Bounds.Height > top.Bounds.Width;
    }
}
