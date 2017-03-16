using System;
using System.Collections.Generic;
using System.Threading;

namespace VainBotDiscord.Utils
{
    public class ThrottlerService
    {
        List<ChannelThrottler> _ytThrottles;
        List<ChannelThrottler> _liveThrottles;

        public ThrottlerService()
        {
            _ytThrottles = new List<ChannelThrottler>();
            _liveThrottles = new List<ChannelThrottler>();
        }

        public bool YouTubeAllowed(ulong channelId)
        {
            return !_ytThrottles.Exists(t => t.ChannelId == channelId);
        }

        public bool LiveAllowed(ulong channelId)
        {
            return !_liveThrottles.Exists(t => t.ChannelId == channelId);
        }

        public void ThrottleYouTube(ulong channelId)
        {
            var t = new Timer(UnthrottleYouTube, channelId, TimeSpan.FromSeconds(60), Timeout.InfiniteTimeSpan);
            _ytThrottles.Add(new ChannelThrottler(channelId, t));
        }

        public void ThrottleLive(ulong channelId)
        {
            var t = new Timer(UnthrottleLive, channelId, TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);
            _liveThrottles.Add(new ChannelThrottler(channelId, t));
        }

        void UnthrottleYouTube(object channelId)
        {
            var item = _ytThrottles.Find(t => t.ChannelId == (ulong)channelId);
            if (item == null)
                return;

            item.Timer.Dispose();
            _ytThrottles.Remove(item);
        }

        void UnthrottleLive(object channelId)
        {
            var item = _liveThrottles.Find(t => t.ChannelId == (ulong)channelId);
            if (item == null)
                return;

            item.Timer.Dispose();
            _liveThrottles.Remove(item);
        }
    }

    class ChannelThrottler
    {
        public ChannelThrottler(ulong channelId, Timer timer)
        {
            ChannelId = channelId;
            Timer = timer;
        }

        public ulong ChannelId { get; set; }

        public Timer Timer { get; set; }
    }
}
