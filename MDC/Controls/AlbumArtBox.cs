using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MDC.Controls
{
    /// <summary>
    /// 专辑封面展示控件；没有封面时绘制音乐应用风格的渐变占位图。
    /// </summary>
    internal sealed class AlbumArtBox : Control
    {
        private Image albumArt;

        public AlbumArtBox()
        {
            DoubleBuffered = true;
            Size = new Size(56, 56);
        }

        public Image AlbumArt
        {
            get { return albumArt; }
            set
            {
                albumArt?.Dispose();
                albumArt = value;
                Invalidate();
            }
        }

        public Color BorderColor { get; set; } = Color.FromArgb(70, 255, 255, 255);

        public Color PlaceholderStart { get; set; } = Color.FromArgb(236, 64, 122);

        public Color PlaceholderEnd { get; set; } = Color.FromArgb(35, 198, 200);

        public Color PlaceholderCenter { get; set; } = Color.FromArgb(235, 14, 18, 28);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                albumArt?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var clipPath = RoundedRect(bounds, 28))
            {
                e.Graphics.SetClip(clipPath);
                if (albumArt != null)
                {
                    e.Graphics.DrawImage(albumArt, CoverRectangle(albumArt.Size, bounds));
                }
                else
                {
                    DrawPlaceholder(e.Graphics, bounds);
                }

                e.Graphics.ResetClip();
                using (var borderPen = new Pen(BorderColor, 1))
                {
                    e.Graphics.DrawPath(borderPen, clipPath);
                }
            }
        }

        private static Rectangle CoverRectangle(Size imageSize, Rectangle bounds)
        {
            var scale = System.Math.Max((double)bounds.Width / imageSize.Width, (double)bounds.Height / imageSize.Height);
            var width = (int)(imageSize.Width * scale);
            var height = (int)(imageSize.Height * scale);
            return new Rectangle(bounds.X + (bounds.Width - width) / 2, bounds.Y + (bounds.Height - height) / 2, width, height);
        }

        private void DrawPlaceholder(Graphics graphics, Rectangle bounds)
        {
            using (var brush = new LinearGradientBrush(bounds, PlaceholderStart, PlaceholderEnd, 135F))
            {
                graphics.FillRectangle(brush, bounds);
            }

            var ringSize = System.Math.Max(20, System.Math.Min(bounds.Width, bounds.Height) - 18);
            var ringX = bounds.X + (bounds.Width - ringSize) / 2;
            var ringY = bounds.Y + (bounds.Height - ringSize) / 2;
            var centerSize = System.Math.Max(12, ringSize / 3);
            var centerX = bounds.X + bounds.Width / 2 - centerSize / 2;
            var centerY = bounds.Y + bounds.Height / 2 - centerSize / 2;

            using (var overlay = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
            {
                graphics.FillEllipse(overlay, ringX, ringY, ringSize, ringSize);
            }

            using (var center = new SolidBrush(PlaceholderCenter))
            {
                graphics.FillEllipse(center, centerX, centerY, centerSize, centerSize);
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var diameter = radius * 2;
            var path = new GraphicsPath();

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
