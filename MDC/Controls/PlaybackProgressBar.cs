using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MDC.Controls
{
    /// <summary>
    /// 音乐播放器风格进度条：厚轨道、渐变填充、悬停滑块，并保留点击/拖动跳转。
    /// </summary>
    internal sealed class PlaybackProgressBar : Control
    {
        private double progress;
        private double dragStartProgress;
        private bool movedWhileDragging;
        private bool canSeek;
        private bool dragging;
        private bool hovered;

        public PlaybackProgressBar()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Height = 24;
        }

        public event EventHandler<ProgressSeekRequestedEventArgs> SeekRequested;

        public double Progress
        {
            get { return progress; }
            set
            {
                progress = Math.Max(0, Math.Min(1, value));
                Invalidate();
            }
        }

        public bool CanSeek
        {
            get { return canSeek; }
            set
            {
                canSeek = value;
                Cursor = canSeek ? Cursors.Hand : Cursors.Default;
                Enabled = true;
                Invalidate();
            }
        }

        public Color TrackColor { get; set; } = Color.FromArgb(44, 54, 70);

        public Color AccentStart { get; set; } = Color.FromArgb(255, 64, 129);

        public Color AccentEnd { get; set; } = Color.FromArgb(38, 208, 206);

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(FindOpaqueBackColor());

            var track = GetTrackBounds();
            DrawTrack(e.Graphics, track);
            DrawFill(e.Graphics, track);
            DrawThumb(e.Graphics, track);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            hovered = false;
            if (!dragging)
            {
                Invalidate();
            }

            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && CanSeek)
            {
                dragging = true;
                Capture = true;
                dragStartProgress = progress;
                movedWhileDragging = false;
                Progress = ProgressFromX(e.X);
                SeekRequested?.Invoke(this, new ProgressSeekRequestedEventArgs(progress));
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragging && CanSeek)
            {
                var nextProgress = ProgressFromX(e.X);
                movedWhileDragging = movedWhileDragging || Math.Abs(nextProgress - progress) > 0.005;
                Progress = nextProgress;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (dragging && CanSeek)
            {
                dragging = false;
                Capture = false;
                Progress = ProgressFromX(e.X);
                if (movedWhileDragging && Math.Abs(progress - dragStartProgress) > 0.005)
                {
                    SeekRequested?.Invoke(this, new ProgressSeekRequestedEventArgs(progress));
                }
            }

            base.OnMouseUp(e);
        }

        private void DrawTrack(Graphics graphics, Rectangle track)
        {
            var shadow = new Rectangle(track.X, track.Y + 2, track.Width, track.Height);
            using (var shadowPath = CapsulePath(shadow))
            using (var shadowBrush = new SolidBrush(Color.FromArgb(28, 0, 0, 0)))
            {
                graphics.FillPath(shadowBrush, shadowPath);
            }

            using (var path = CapsulePath(track))
            using (var brush = new SolidBrush(TrackColor))
            {
                graphics.FillPath(brush, path);
            }
        }

        private void DrawFill(Graphics graphics, Rectangle track)
        {
            var filledWidth = Math.Max(0, (int)Math.Round(track.Width * progress));
            if (filledWidth <= 0)
            {
                return;
            }

            var filled = new Rectangle(track.X, track.Y, filledWidth, track.Height);
            using (var path = CapsulePath(filled))
            using (var brush = new LinearGradientBrush(track, AccentStart, AccentEnd, 0F))
            {
                graphics.FillPath(brush, path);
            }
        }

        private void DrawThumb(Graphics graphics, Rectangle track)
        {
            if (!CanSeek || progress <= 0)
            {
                return;
            }

            var active = hovered || dragging;
            var thumbSize = active ? 16 : 12;
            var glowSize = active ? 26 : 0;
            var centerX = track.X + (int)Math.Round(track.Width * progress);
            var centerY = track.Y + track.Height / 2;
            centerX = Math.Max(track.Left + thumbSize / 2, Math.Min(track.Right - thumbSize / 2, centerX));

            if (glowSize > 0)
            {
                using (var glow = new SolidBrush(Color.FromArgb(54, AccentEnd)))
                {
                    graphics.FillEllipse(glow, centerX - glowSize / 2, centerY - glowSize / 2, glowSize, glowSize);
                }
            }

            using (var thumb = new SolidBrush(AccentEnd))
            {
                graphics.FillEllipse(thumb, centerX - thumbSize / 2, centerY - thumbSize / 2, thumbSize, thumbSize);
            }
        }

        private Rectangle GetTrackBounds()
        {
            return new Rectangle(2, Height / 2 - 6, Math.Max(1, Width - 4), 12);
        }

        private double ProgressFromX(int x)
        {
            var track = GetTrackBounds();
            if (track.Width <= 0)
            {
                return 0;
            }

            return Math.Max(0, Math.Min(1, (double)(x - track.X) / track.Width));
        }

        private Color FindOpaqueBackColor()
        {
            var control = Parent;
            while (control != null)
            {
                if (control.BackColor.A == 255 && control.BackColor != Color.Transparent)
                {
                    return control.BackColor;
                }

                control = control.Parent;
            }

            return Color.FromArgb(14, 18, 28);
        }

        private static GraphicsPath CapsulePath(Rectangle bounds)
        {
            var path = new GraphicsPath();
            if (bounds.Width <= bounds.Height)
            {
                path.AddEllipse(bounds);
                return path;
            }

            var diameter = bounds.Height;
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 90, 180);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270, 180);
            path.CloseFigure();
            return path;
        }
    }

    internal sealed class ProgressSeekRequestedEventArgs : EventArgs
    {
        public ProgressSeekRequestedEventArgs(double progress)
        {
            Progress = progress;
        }

        public double Progress { get; }
    }
}
