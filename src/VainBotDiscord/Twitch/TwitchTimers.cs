using System.Threading;

namespace VainBotDiscord.Twitch
{
    public class TwitchCheckTimer
    {
        public TwitchCheckTimer(long userId, Timer timer)
        {
            UserId = userId;
            Timer = timer;
        }

        public long UserId { get; set; }

        public Timer Timer { get; set; }
    }

    public class TwitchUpdateTimer
    {
        public TwitchUpdateTimer(long streamId, Timer timer)
        {
            StreamId = streamId;
            Timer = timer;
        }

        public long StreamId { get; set; }

        public Timer Timer { get; set; }
    }
}
