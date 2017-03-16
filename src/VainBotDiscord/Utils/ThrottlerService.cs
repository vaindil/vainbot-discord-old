using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VainBotDiscord.Utils
{
    public class ThrottlerService
    {
        Timer _ytThrottleTimer;
        Timer _liveThrottleTimer;

        bool _ytAllowed = true;
        bool _liveAllowed = true;

        public bool YouTubeAllowed()
        {
            return _ytAllowed;
        }

        public bool LiveAllowed()
        {
            return _liveAllowed;
        }

        public void ThrottleYouTube()
        {
            _ytAllowed = false;
            _ytThrottleTimer = new Timer(ClearYouTubeThrottler, null, TimeSpan.FromSeconds(60), Timeout.InfiniteTimeSpan);
        }

        public void ThrottleLive()
        {
            _liveAllowed = false;
            _liveThrottleTimer = new Timer(ClearLiveThrottler, null, TimeSpan.FromSeconds(30), Timeout.InfiniteTimeSpan);
        }

        void ClearYouTubeThrottler(object unused)
        {
            _ytAllowed = true;
            _ytThrottleTimer.Dispose();
        }

        void ClearLiveThrottler(object unused)
        {
            _liveAllowed = true;
            _liveThrottleTimer.Dispose();
        }
    }
}
