using System;
using System.Drawing;
using Windows.Media.Control;

namespace MDC.Services
{
    /// <summary>
    /// 保存一次 SMTC 读取到的媒体状态，窗体只消费这个轻量快照，避免 UI 层直接依赖 WinRT 对象。
    /// </summary>
    internal sealed class MediaSnapshot
    {
        public static readonly MediaSnapshot Empty = new MediaSnapshot
        {
            Title = "未检测到媒体会话",
            Artist = "请先打开网易云音乐并播放一首歌",
            Album = string.Empty,
            SourceAppId = string.Empty,
            PlaybackStatus = GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed,
            Position = TimeSpan.Zero,
            Duration = TimeSpan.Zero
        };

        public string Title { get; set; }

        public string Artist { get; set; }

        public string Album { get; set; }

        public string SourceAppId { get; set; }

        public GlobalSystemMediaTransportControlsSessionPlaybackStatus PlaybackStatus { get; set; }

        public TimeSpan Position { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan Duration { get; set; }

        public Image CoverImage { get; set; }

        public bool IsPlaying
        {
            get { return PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing; }
        }

        public double Progress
        {
            get
            {
                if (Duration <= TimeSpan.Zero)
                {
                    return 0;
                }

                var value = (Position - StartTime).TotalMilliseconds / Duration.TotalMilliseconds;
                return Math.Max(0, Math.Min(1, value));
            }
        }
    }
}
