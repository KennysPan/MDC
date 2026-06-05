using System.Drawing;

namespace MDC.Theming
{
    /// <summary>
    /// 界面所有可切换颜色集中在这里，避免深色/浅色切换时遗漏某个控件。
    /// </summary>
    internal sealed class ThemePalette
    {
        public bool IsLight { get; private set; }

        public Color Background { get; private set; }

        public Color HeaderBackground { get; private set; }

        public Color PrimaryText { get; private set; }

        public Color SecondaryText { get; private set; }

        public Color MutedText { get; private set; }

        public Color Accent { get; private set; }

        public Color AccentEnd { get; private set; }

        public Color Button { get; private set; }

        public Color ButtonHover { get; private set; }

        public Color ButtonPressed { get; private set; }

        public Color PrimaryButton { get; private set; }

        public Color PrimaryButtonHover { get; private set; }

        public Color PrimaryButtonPressed { get; private set; }

        public Color DisabledText { get; private set; }

        public Color CloseButton { get; private set; }

        public Color MinButton { get; private set; }

        public Color AlbumBorder { get; private set; }

        public Color AlbumPlaceholderCenter { get; private set; }

        public Color ProgressTrack { get; private set; }

        public static ThemePalette Create(bool isLight)
        {
            return isLight ? Light() : Dark();
        }

        private static ThemePalette Dark()
        {
            return new ThemePalette
            {
                IsLight = false,
                Background = Color.FromArgb(14, 18, 28),
                HeaderBackground = Color.FromArgb(18, 23, 36),
                PrimaryText = Color.White,
                SecondaryText = Color.FromArgb(174, 184, 202),
                MutedText = Color.FromArgb(126, 137, 154),
                Accent = Color.FromArgb(103, 232, 249),
                AccentEnd = Color.FromArgb(255, 64, 129),
                Button = Color.FromArgb(44, 53, 70),
                ButtonHover = Color.FromArgb(60, 72, 94),
                ButtonPressed = Color.FromArgb(33, 40, 54),
                PrimaryButton = Color.FromArgb(229, 57, 96),
                PrimaryButtonHover = Color.FromArgb(245, 75, 116),
                PrimaryButtonPressed = Color.FromArgb(190, 42, 78),
                DisabledText = Color.FromArgb(120, 130, 145),
                CloseButton = Color.FromArgb(208, 56, 78),
                MinButton = Color.FromArgb(52, 63, 84),
                AlbumBorder = Color.FromArgb(70, 255, 255, 255),
                AlbumPlaceholderCenter = Color.FromArgb(235, 14, 18, 28),
                ProgressTrack = Color.FromArgb(44, 54, 70)
            };
        }

        private static ThemePalette Light()
        {
            return new ThemePalette
            {
                IsLight = true,
                Background = Color.FromArgb(246, 248, 252),
                HeaderBackground = Color.FromArgb(255, 255, 255),
                PrimaryText = Color.FromArgb(22, 28, 38),
                SecondaryText = Color.FromArgb(74, 85, 104),
                MutedText = Color.FromArgb(112, 123, 142),
                Accent = Color.FromArgb(0, 142, 170),
                AccentEnd = Color.FromArgb(225, 48, 91),
                Button = Color.FromArgb(226, 232, 240),
                ButtonHover = Color.FromArgb(214, 222, 234),
                ButtonPressed = Color.FromArgb(198, 208, 224),
                PrimaryButton = Color.FromArgb(225, 48, 91),
                PrimaryButtonHover = Color.FromArgb(238, 65, 109),
                PrimaryButtonPressed = Color.FromArgb(190, 35, 75),
                DisabledText = Color.FromArgb(150, 160, 176),
                CloseButton = Color.FromArgb(218, 66, 88),
                MinButton = Color.FromArgb(226, 232, 240),
                AlbumBorder = Color.FromArgb(120, 22, 28, 38),
                AlbumPlaceholderCenter = Color.FromArgb(248, 250, 252),
                ProgressTrack = Color.FromArgb(220, 226, 236)
            };
        }
    }
}
