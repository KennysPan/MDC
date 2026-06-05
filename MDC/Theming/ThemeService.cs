using Microsoft.Win32;

namespace MDC.Theming
{
    /// <summary>
    /// Windows 主题读取辅助类。SMTC 控制不依赖它，所以读取失败时只影响界面颜色兜底。
    /// </summary>
    internal static class ThemeService
    {
        private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string AppsUseLightThemeValue = "AppsUseLightTheme";

        public static ThemePalette ResolvePalette(ThemeMode mode)
        {
            if (mode == ThemeMode.Light)
            {
                return ThemePalette.Create(true);
            }

            if (mode == ThemeMode.Dark)
            {
                return ThemePalette.Create(false);
            }

            return ThemePalette.Create(IsSystemLightTheme());
        }

        public static ThemeMode NextMode(ThemeMode mode)
        {
            switch (mode)
            {
                case ThemeMode.System:
                    return ThemeMode.Dark;
                case ThemeMode.Dark:
                    return ThemeMode.Light;
                default:
                    return ThemeMode.System;
            }
        }

        public static string GetModeText(ThemeMode mode)
        {
            switch (mode)
            {
                case ThemeMode.Dark:
                    return "深色";
                case ThemeMode.Light:
                    return "浅色";
                default:
                    return "跟随系统";
            }
        }

        private static bool IsSystemLightTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath))
                {
                    var value = key?.GetValue(AppsUseLightThemeValue);
                    return value is int number && number > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
