using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using MDC.Controls;
using MDC.Services;
using MDC.Theming;
using Windows.Media.Control;

namespace MDC
{
    public partial class Form1 : Form
    {
        private const int WmNclButtonDown = 0xA1;
        private const int HtCaption = 0x2;

        private readonly MediaController mediaController = new MediaController();
        private readonly Timer refreshTimer = new Timer();

        private Panel headerPanel;
        private TableLayoutPanel contentPanel;
        private Panel detailsPanel;
        private Panel timePanel;
        private FlowLayoutPanel buttonPanel;
        private AlbumArtBox albumArtBox;
        private Label captionLabel;
        private Label titleLabel;
        private Label artistLabel;
        private Label albumLabel;
        private Label statusLabel;
        private Label sourceLabel;
        private Label elapsedLabel;
        private Label durationLabel;
        private PlaybackProgressBar progressBar;
        private ModernButton playPauseButton;
        private ModernButton previousButton;
        private ModernButton nextButton;
        private ModernButton stopButton;
        private ModernButton refreshButton;
        private ModernButton themeButton;
        private ModernButton minButton;
        private ModernButton closeButton;

        private ThemeMode themeMode = ThemeMode.System;
        private bool isRefreshing;

        public Form1()
        {
            InitializeComponent();
            BuildUi();
            WireEvents();
            ApplyTheme();
        }

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        protected override async void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // 保持 MDC 作为桌面迷你控制条浮在其他应用上方。
            TopMost = true;
            await RefreshMediaAsync();
            refreshTimer.Interval = 1500;
            refreshTimer.Start();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ApplyRoundedWindowRegion();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            refreshTimer.Stop();
            refreshTimer.Dispose();
            SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
            mediaController.Dispose();
            base.OnFormClosed(e);
        }

        private void BuildUi()
        {
            SuspendLayout();

            var header = BuildHeader();
            var content = BuildContent();

            Controls.Add(content);
            Controls.Add(header);

            ResumeLayout(false);
        }

        private Control BuildHeader()
        {
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(10, 4, 6, 4)
            };

            headerPanel.MouseDown += Header_MouseDown;

            captionLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 210,
                Text = "网易云 SMTC 控制器",
                Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            captionLabel.MouseDown += Header_MouseDown;

            closeButton = BuildWindowButton("X", Color.FromArgb(208, 56, 78));
            closeButton.Click += (sender, args) => Close();

            minButton = BuildWindowButton("_", Color.FromArgb(52, 63, 84));
            minButton.Click += (sender, args) => WindowState = FormWindowState.Minimized;

            themeButton = BuildWindowButton("跟随系统", Color.FromArgb(44, 53, 70));
            themeButton.Width = 72;

            headerPanel.Controls.Add(closeButton);
            headerPanel.Controls.Add(minButton);
            headerPanel.Controls.Add(themeButton);
            headerPanel.Controls.Add(captionLabel);
            return headerPanel;
        }

        private Control BuildContent()
        {
            contentPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(8, 6, 8, 6)
            };
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));

            albumArtBox = new AlbumArtBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left,
                Size = new Size(56, 56),
                Margin = new Padding(0, 1, 8, 0)
            };

            detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(4, 0, 6, 0)
            };

            statusLabel = new Label
            {
                AutoSize = false,
                Height = 16,
                Dock = DockStyle.Top,
                Font = new Font("Microsoft YaHei UI", 8.5F, FontStyle.Bold),
                Text = "正在连接 SMTC..."
            };

            titleLabel = new Label
            {
                AutoSize = false,
                Height = 20,
                Dock = DockStyle.Top,
                Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold),
                Text = "等待网易云音乐",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            artistLabel = BuildMetaLabel("歌手");
            albumLabel = BuildMetaLabel("专辑");
            sourceLabel = BuildMetaLabel("会话");

            progressBar = new PlaybackProgressBar
            {
                Dock = DockStyle.Top,
                Height = 24,
                Margin = new Padding(0)
            };

            timePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 14,
                BackColor = Color.Transparent,
                Visible = true
            };

            elapsedLabel = BuildTimeLabel(ContentAlignment.MiddleLeft);
            durationLabel = BuildTimeLabel(ContentAlignment.MiddleRight);
            timePanel.Controls.Add(durationLabel);
            timePanel.Controls.Add(elapsedLabel);

            BuildButtonPanel();

            detailsPanel.Controls.Add(timePanel);
            detailsPanel.Controls.Add(progressBar);
            detailsPanel.Controls.Add(titleLabel);
            detailsPanel.Controls.Add(statusLabel);

            contentPanel.Controls.Add(albumArtBox, 0, 0);
            contentPanel.Controls.Add(detailsPanel, 1, 0);
            contentPanel.Controls.Add(buttonPanel, 2, 0);
            return contentPanel;
        }

        private Control BuildButtonPanel()
        {
            buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
                Padding = new Padding(0, 12, 0, 0)
            };

            previousButton = BuildCommandButton("上一首", 62);
            playPauseButton = BuildCommandButton("播放", 62);
            nextButton = BuildCommandButton("下一首", 62);
            stopButton = BuildCommandButton("停", 34);
            refreshButton = BuildCommandButton("刷", 34);

            buttonPanel.Controls.Add(previousButton);
            buttonPanel.Controls.Add(playPauseButton);
            buttonPanel.Controls.Add(nextButton);
            return buttonPanel;
        }

        private void WireEvents()
        {
            refreshTimer.Tick += async (sender, args) => await RefreshMediaAsync();

            previousButton.Click += async (sender, args) => await RunCommandAsync(() => mediaController.PreviousAsync());
            playPauseButton.Click += async (sender, args) => await RunCommandAsync(() => mediaController.TogglePlayPauseAsync());
            nextButton.Click += async (sender, args) => await RunCommandAsync(() => mediaController.NextAsync());
            stopButton.Click += async (sender, args) => await RunCommandAsync(() => mediaController.StopAsync());
            refreshButton.Click += async (sender, args) => await RefreshMediaAsync();
            progressBar.SeekRequested += async (sender, args) => await SeekProgressAsync(args.Progress);
            themeButton.Click += (sender, args) => SwitchThemeMode();
            SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
        }

        private void SwitchThemeMode()
        {
            themeMode = ThemeService.NextMode(themeMode);
            ApplyTheme();
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (themeMode != ThemeMode.System)
            {
                return;
            }

            // Windows 主题变化事件可能不在 UI 线程触发，用 BeginInvoke 回到 WinForms 线程安全刷新。
            if (!IsDisposed && IsHandleCreated)
            {
                BeginInvoke(new Action(ApplyTheme));
            }
        }

        private void ApplyTheme()
        {
            var palette = ThemeService.ResolvePalette(themeMode);

            BackColor = palette.Background;
            headerPanel.BackColor = palette.HeaderBackground;
            contentPanel.BackColor = palette.Background;
            detailsPanel.BackColor = Color.Transparent;
            timePanel.BackColor = Color.Transparent;
            buttonPanel.BackColor = Color.Transparent;

            captionLabel.ForeColor = palette.PrimaryText;
            titleLabel.ForeColor = palette.PrimaryText;
            statusLabel.ForeColor = palette.Accent;
            artistLabel.ForeColor = palette.SecondaryText;
            albumLabel.ForeColor = palette.SecondaryText;
            sourceLabel.ForeColor = palette.SecondaryText;
            elapsedLabel.ForeColor = palette.MutedText;
            durationLabel.ForeColor = palette.MutedText;

            // 自绘控件不参与系统主题，主动把调色板同步进去。
            albumArtBox.BorderColor = palette.AlbumBorder;
            albumArtBox.PlaceholderStart = palette.AccentEnd;
            albumArtBox.PlaceholderEnd = palette.Accent;
            albumArtBox.PlaceholderCenter = palette.AlbumPlaceholderCenter;
            albumArtBox.Invalidate();

            progressBar.TrackColor = palette.ProgressTrack;
            progressBar.AccentStart = palette.AccentEnd;
            progressBar.AccentEnd = palette.Accent;
            progressBar.Invalidate();

            ApplyButtonTheme(previousButton, palette, false);
            ApplyButtonTheme(playPauseButton, palette, true);
            ApplyButtonTheme(nextButton, palette, false);
            ApplyButtonTheme(stopButton, palette, false);
            ApplyButtonTheme(refreshButton, palette, false);
            ApplyButtonTheme(themeButton, palette, false);
            ApplyButtonTheme(minButton, palette, false);
            ApplyButtonTheme(closeButton, palette, false);

            closeButton.ButtonColor = palette.CloseButton;
            closeButton.HoverColor = ControlPaint.Light(palette.CloseButton);
            closeButton.PressedColor = ControlPaint.Dark(palette.CloseButton);
            closeButton.ForeColor = Color.White;
            closeButton.Invalidate();

            minButton.ButtonColor = palette.MinButton;
            minButton.HoverColor = ControlPaint.Light(palette.MinButton);
            minButton.PressedColor = ControlPaint.Dark(palette.MinButton);
            minButton.ForeColor = palette.IsLight ? palette.SecondaryText : palette.PrimaryText;
            minButton.Invalidate();

            themeButton.Text = ThemeService.GetModeText(themeMode);
            themeButton.Invalidate();
        }

        private static void ApplyButtonTheme(ModernButton button, ThemePalette palette, bool primary)
        {
            button.ButtonColor = primary ? palette.PrimaryButton : palette.Button;
            button.HoverColor = primary ? palette.PrimaryButtonHover : palette.ButtonHover;
            button.PressedColor = primary ? palette.PrimaryButtonPressed : palette.ButtonPressed;
            button.ForeColor = primary || !palette.IsLight ? Color.White : palette.PrimaryText;
            button.DisabledTextColor = palette.DisabledText;
            button.Invalidate();
        }

        private async Task RunCommandAsync(Func<Task<bool>> command)
        {
            SetButtonsEnabled(false);

            try
            {
                // SMTC 命令是异步 WinRT 调用，等待完成后立刻刷新界面，保证状态和按钮文案同步。
                var succeeded = await command();
                statusLabel.Text = succeeded ? "命令已发送" : "播放器暂未响应";
            }
            catch (Exception ex)
            {
                statusLabel.Text = "控制失败：" + ex.Message;
            }
            finally
            {
                SetButtonsEnabled(true);
                await RefreshMediaAsync();
            }
        }

        private async Task SeekProgressAsync(double progress)
        {
            progressBar.Progress = progress;
            statusLabel.Text = "正在跳转进度...";

            try
            {
                var succeeded = await mediaController.SeekToProgressAsync(progress);
                statusLabel.Text = succeeded ? "进度已跳转" : "网易云当前未开放 SMTC 进度跳转";
            }
            catch (Exception ex)
            {
                statusLabel.Text = "进度跳转失败：" + ex.Message;
            }

            await Task.Delay(350);
            await RefreshMediaAsync();
        }

        private async Task RefreshMediaAsync()
        {
            if (isRefreshing)
            {
                return;
            }

            isRefreshing = true;
            try
            {
                var snapshot = await mediaController.RefreshAsync();
                ApplySnapshot(snapshot);
            }
            catch (Exception ex)
            {
                statusLabel.Text = "读取 SMTC 失败：" + ex.Message;
            }
            finally
            {
                isRefreshing = false;
            }
        }

        private void ApplySnapshot(MediaSnapshot snapshot)
        {
            titleLabel.Text = string.IsNullOrWhiteSpace(snapshot.Artist)
                ? snapshot.Title
                : snapshot.Title + " - " + snapshot.Artist;
            artistLabel.Text = string.Empty;
            albumLabel.Text = "专辑  " + snapshot.Album;
            sourceLabel.Text = string.IsNullOrWhiteSpace(snapshot.SourceAppId) ? "会话  未连接" : "会话  " + snapshot.SourceAppId;
            elapsedLabel.Text = FormatTime(snapshot.Position - snapshot.StartTime);
            durationLabel.Text = FormatTime(snapshot.Duration);
            progressBar.Progress = snapshot.Progress;
            progressBar.CanSeek = !string.IsNullOrWhiteSpace(snapshot.SourceAppId);
            albumArtBox.AlbumArt = snapshot.CoverImage;

            statusLabel.Text = BuildStatusText(snapshot);
            playPauseButton.Text = snapshot.IsPlaying ? "暂停" : "播放";
        }

        private static string BuildStatusText(MediaSnapshot snapshot)
        {
            switch (snapshot.PlaybackStatus)
            {
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing:
                    return "正在播放";
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused:
                    return "已暂停";
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Stopped:
                    return "已停止";
                case GlobalSystemMediaTransportControlsSessionPlaybackStatus.Changing:
                    return "正在切换";
                default:
                    return "等待网易云音乐";
            }
        }

        private static string FormatTime(TimeSpan value)
        {
            if (value <= TimeSpan.Zero)
            {
                return "00:00";
            }

            return value.TotalHours >= 1
                ? value.ToString(@"h\:mm\:ss")
                : value.ToString(@"mm\:ss");
        }

        private void SetButtonsEnabled(bool enabled)
        {
            previousButton.Enabled = enabled;
            playPauseButton.Enabled = enabled;
            nextButton.Enabled = enabled;
            stopButton.Enabled = enabled;
            refreshButton.Enabled = enabled;
        }

        private void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            // 无边框窗口需要手动把标题栏拖拽转换成系统移动窗口消息。
            ReleaseCapture();
            SendMessage(Handle, WmNclButtonDown, HtCaption, 0);
        }

        private void ApplyRoundedWindowRegion()
        {
            using (var path = RoundedRect(new Rectangle(0, 0, Width, Height), 22))
            {
                Region = new Region(path);
            }
        }

        private static Label BuildMetaLabel(string text)
        {
            return new Label
            {
                AutoSize = false,
                Height = 14,
                Dock = DockStyle.Top,
                ForeColor = Color.FromArgb(174, 184, 202),
                Font = new Font("Microsoft YaHei UI", 8F),
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
        }

        private static Label BuildTimeLabel(ContentAlignment alignment)
        {
            return new Label
            {
                AutoSize = false,
                Dock = alignment == ContentAlignment.MiddleLeft ? DockStyle.Left : DockStyle.Right,
                Width = 62,
                ForeColor = Color.FromArgb(126, 137, 154),
                Font = new Font("Consolas", 7.5F, FontStyle.Bold),
                Text = "00:00",
                TextAlign = alignment
            };
        }

        private static ModernButton BuildCommandButton(string text, int width)
        {
            return new ModernButton
            {
                Text = text,
                Width = width,
                Height = 34,
                Radius = 12,
                Font = new Font("Microsoft YaHei UI", 8F, FontStyle.Bold),
                Margin = new Padding(0, 0, 8, 0)
            };
        }

        private static ModernButton BuildWindowButton(string text, Color color)
        {
            return new ModernButton
            {
                Text = text,
                Dock = DockStyle.Right,
                Width = 26,
                Height = 22,
                Margin = new Padding(2),
                Radius = 8,
                Font = new Font("Microsoft YaHei UI", 7F, FontStyle.Bold),
                ButtonColor = color,
                HoverColor = ControlPaint.Light(color),
                PressedColor = ControlPaint.Dark(color)
            };
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
