using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace MDC.Services
{
    /// <summary>
    /// 基于 Windows SMTC(GlobalSystemMediaTransportControls) 控制系统媒体会话。
    /// 网易云音乐、Spotify、Edge 等播放器都会把播放状态暴露到这里。
    /// </summary>
    internal sealed class MediaController : IDisposable
    {
        private GlobalSystemMediaTransportControlsSessionManager manager;
        private GlobalSystemMediaTransportControlsSession activeSession;

        public async Task InitializeAsync()
        {
            if (manager != null)
            {
                return;
            }

            manager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
        }

        public async Task<MediaSnapshot> RefreshAsync()
        {
            await InitializeAsync();

            activeSession = FindPreferredSession();
            if (activeSession == null)
            {
                return MediaSnapshot.Empty;
            }

            return await CreateSnapshotAsync(activeSession);
        }

        public async Task<bool> TogglePlayPauseAsync()
        {
            var session = await EnsureSessionAsync();
            return session != null && await session.TryTogglePlayPauseAsync();
        }

        public async Task<bool> PreviousAsync()
        {
            var session = await EnsureSessionAsync();
            return session != null && await session.TrySkipPreviousAsync();
        }

        public async Task<bool> NextAsync()
        {
            var session = await EnsureSessionAsync();
            return session != null && await session.TrySkipNextAsync();
        }

        public async Task<bool> StopAsync()
        {
            var session = await EnsureSessionAsync();
            return session != null && await session.TryStopAsync();
        }

        public async Task<bool> SeekToProgressAsync(double progress)
        {
            await InitializeAsync();

            // 跳转前重新选择会话，避免旧会话缓存导致命令发给已经失效的媒体会话。
            var session = FindPreferredSession();
            activeSession = session;
            if (session == null)
            {
                return false;
            }

            var timeline = session.GetTimelineProperties();
            var startTime = timeline.StartTime > TimeSpan.Zero ? timeline.StartTime : timeline.MinSeekTime;
            var endTime = timeline.EndTime > startTime ? timeline.EndTime : timeline.MaxSeekTime;
            var duration = endTime > startTime ? endTime - startTime : TimeSpan.Zero;
            if (duration <= TimeSpan.Zero)
            {
                return false;
            }

            var clampedProgress = Math.Max(0, Math.Min(1, progress));
            var targetPosition = startTime + TimeSpan.FromTicks((long)(duration.Ticks * clampedProgress));

            // 不同播放器对 SMTC 位置基准处理不完全一致，按绝对、最小可 seek 基准、相对时长依次尝试。
            if (await session.TryChangePlaybackPositionAsync(targetPosition.Ticks))
            {
                return true;
            }

            var minSeekTarget = timeline.MinSeekTime + TimeSpan.FromTicks((long)(duration.Ticks * clampedProgress));
            if (minSeekTarget != targetPosition && await session.TryChangePlaybackPositionAsync(minSeekTarget.Ticks))
            {
                return true;
            }

            var relativeTarget = TimeSpan.FromTicks((long)(duration.Ticks * clampedProgress));
            return await session.TryChangePlaybackPositionAsync(relativeTarget.Ticks);
        }

        internal static bool LooksLikeNetEaseCloudMusic(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                return false;
            }

            var normalized = appId.ToLowerInvariant();
            return normalized.Contains("netease")
                || normalized.Contains("cloudmusic")
                || normalized.Contains("orpheus")
                || normalized.Contains("163music");
        }

        private async Task<GlobalSystemMediaTransportControlsSession> EnsureSessionAsync()
        {
            await InitializeAsync();

            if (activeSession == null)
            {
                activeSession = FindPreferredSession();
            }

            return activeSession;
        }

        private GlobalSystemMediaTransportControlsSession FindPreferredSession()
        {
            var currentSession = manager.GetCurrentSession();
            if (currentSession != null && IsUsablePlaybackSession(currentSession))
            {
                return currentSession;
            }

            var sessions = manager.GetSessions();
            var netEaseSession = sessions.FirstOrDefault(s => LooksLikeNetEaseCloudMusic(s.SourceAppUserModelId));
            if (netEaseSession != null)
            {
                return netEaseSession;
            }

            // 不同网易云版本的 AUMID 可能变化；找不到时兜底为系统当前媒体会话，仍可控制正在播放的网易云。
            return currentSession;
        }

        private static bool IsUsablePlaybackSession(GlobalSystemMediaTransportControlsSession session)
        {
            var status = session.GetPlaybackInfo().PlaybackStatus;
            return status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing
                || status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused;
        }

        private static async Task<MediaSnapshot> CreateSnapshotAsync(GlobalSystemMediaTransportControlsSession session)
        {
            var playbackInfo = session.GetPlaybackInfo();
            var timeline = session.GetTimelineProperties();
            var properties = await session.TryGetMediaPropertiesAsync();
            var startTime = timeline.StartTime > TimeSpan.Zero ? timeline.StartTime : timeline.MinSeekTime;
            var endTime = timeline.EndTime > startTime ? timeline.EndTime : timeline.MaxSeekTime;

            return new MediaSnapshot
            {
                Title = string.IsNullOrWhiteSpace(properties.Title) ? "正在等待曲目" : properties.Title,
                Artist = string.IsNullOrWhiteSpace(properties.Artist) ? "未提供歌手" : properties.Artist,
                Album = string.IsNullOrWhiteSpace(properties.AlbumTitle) ? "未提供专辑" : properties.AlbumTitle,
                SourceAppId = session.SourceAppUserModelId,
                PlaybackStatus = playbackInfo.PlaybackStatus,
                Position = timeline.Position,
                StartTime = startTime,
                Duration = endTime > startTime ? endTime - startTime : TimeSpan.Zero,
                CoverImage = await LoadCoverAsync(properties.Thumbnail)
            };
        }

        private static async Task<Image> LoadCoverAsync(IRandomAccessStreamReference thumbnail)
        {
            if (thumbnail == null)
            {
                return null;
            }

            try
            {
                using (var randomAccessStream = await thumbnail.OpenReadAsync())
                using (var managedStream = randomAccessStream.AsStreamForRead())
                using (var buffer = new MemoryStream())
                {
                    // 复制到内存后再创建 Image，避免 WinRT 流释放后影响绘制。
                    await managedStream.CopyToAsync(buffer);
                    buffer.Position = 0;
                    return Image.FromStream(buffer);
                }
            }
            catch
            {
                // 某些播放器不提供封面或返回受保护流，界面会显示渐变占位图。
                return null;
            }
        }

        public void Dispose()
        {
            if (activeSession != null)
            {
                activeSession = null;
            }

            manager = null;
        }
    }
}
