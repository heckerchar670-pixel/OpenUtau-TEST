using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace OpenUtauMobile.Controls;

/// <summary>
/// 先以低分辨率离屏绘制，再按整数倍最近邻放大。
/// </summary>
internal sealed class PixelSurface
{
    private RenderTargetBitmap? _bitmap;

    public void Render(Control owner, DrawingContext target, Action<DrawingContext> draw)
    {
        double scaling = TopLevel.GetTopLevel(owner)?.RenderScaling ?? 1;
        int block = Math.Max(1, (int)Math.Floor(scaling)); // 每个低分辨率像素占用的物理像素
        Size bounds = owner.Bounds.Size;
        if (!ClassicUtauStyle.Enabled || block == 1 || bounds.Width < 1 || bounds.Height < 1)
        {
            draw(target);
            return;
        }

        PixelSize size = new((int)Math.Ceiling(bounds.Width * scaling / block),
            (int)Math.Ceiling(bounds.Height * scaling / block));
        if (_bitmap == null || _bitmap.PixelSize != size)
        {
            _bitmap?.Dispose();
            _bitmap = new RenderTargetBitmap(size);
        }

        using (DrawingContext offscreen = _bitmap.CreateDrawingContext())
        using (offscreen.PushTransform(Matrix.CreateScale(scaling / block, scaling / block)))
        {
            draw(offscreen);
        }

        // 透明矩形保证命中测试
        target.DrawRectangle(Brushes.Transparent, null, new Rect(bounds));
        using (target.PushClip(new Rect(bounds)))
        using (target.PushRenderOptions(new RenderOptions
                   { BitmapInterpolationMode = BitmapInterpolationMode.None }))
        {
            target.DrawImage(_bitmap, new Rect(0, 0, size.Width, size.Height),
                new Rect(0, 0, size.Width * block / scaling, size.Height * block / scaling));
        }
    }
}
