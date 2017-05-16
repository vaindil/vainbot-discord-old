using System;
using System.Collections.Generic;
using System.Threading;

namespace VainBotDiscord.Utils
{
    public class ThrottlerService
    {
        List<ThrottledChannel> _throttles;

        public ThrottlerService()
        {
            _throttles = new List<ThrottledChannel>();
        }

        public bool CommandAllowed(ThrottleTypes type, ulong channelId)
        {
            return !_throttles.Exists(t => t.Type == type && t.ChannelId == channelId);
        }

        public void Throttle(ThrottleTypes type, ulong channelId)
        {
            var throttleWrapper = new ThrottleWrapper(type, channelId);
            var timer = new Timer(Unthrottle, throttleWrapper, TimeSpan.FromSeconds(60), Timeout.InfiniteTimeSpan);
            _throttles.Add(new ThrottledChannel(type, channelId, timer));
        }

        void Unthrottle(object throttleWrapperIn)
        {
            var throttleWrapper = (ThrottleWrapper)throttleWrapperIn;

            var item = _throttles.Find(t => t.Type == throttleWrapper.Type && t.ChannelId == throttleWrapper.ChannelId);
            if (item == null)
                return;

            item.Timer.Dispose();
            _throttles.Remove(item);
        }
    }

    public enum ThrottleTypes
    {
        YouTube,
        Twitter,
        Live,
        Ferret,
        Cat
    }

    class ThrottledChannel
    {
        public ThrottledChannel(ThrottleTypes type, ulong channelId, Timer timer)
        {
            Type = type;
            ChannelId = channelId;
            Timer = timer;
        }

        public ThrottleTypes Type { get; set; }

        public ulong ChannelId { get; set; }

        public Timer Timer { get; set; }
    }

    class ThrottleWrapper
    {
        public ThrottleWrapper(ThrottleTypes type, ulong channelId)
        {
            Type = type;
            ChannelId = channelId;
        }

        public ThrottleTypes Type { get; set; }

        public ulong ChannelId { get; set; }
    }
}
