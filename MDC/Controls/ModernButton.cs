using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MDC.Controls
{
    /// <summary>
    /// 简单的圆角按钮，用自绘实现统一的深色音乐控制器风格。
    /// </summary>
    internal sealed class ModernButton : Button
    {
        private bool hovered;
        private bool pressed;

        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
            ForeColor = Color.White;
            BackColor = Color.Transparent;
            Height = 48;
        }

        public Color ButtonColor { get; set; } = Color.FromArgb(44, 53, 70);

        public Color HoverColor { get; set; } = Color.FromArgb(60, 72, 94);

        public Color PressedColor { get; set; } = Color.FromArgb(33, 40, 54);

        public Color DisabledTextColor { get; set; } = Color.FromArgb(120, 130, 145);

        public int Radius { get; set; } = 18;

        protected override void OnMouseEnter(System.EventArgs e)
        {
            hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(System.EventArgs e)
        {
            hovered = false;
            pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            pressed = true;
            Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.Clear(FindOpaqueBackColor());

            var fill = pressed ? PressedColor : hovered ? HoverColor : ButtonColor;
            using (var path = RoundedRect(ClientRectangle, Radius))
            using (var brush = new SolidBrush(fill))
            {
                pevent.Graphics.FillPath(brush, path);
            }

            TextRenderer.DrawText(
                pevent.Graphics,
                Text,
                Font,
                ClientRectangle,
                Enabled ? ForeColor : DisabledTextColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
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

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var diameter = radius * 2;
            var path = new GraphicsPath();

            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter - 1, bounds.Top, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter - 1, bounds.Bottom - diameter - 1, diameter, diameter, 0, 90);
            path.AddArc(bounds.Left, bounds.Bottom - diameter - 1, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
