namespace VainBot.OverwatchModels
{
    public class BlobModel
    {
        public RequestKey _Request { get; set; }

        public UsModel Us { get; set; }

        public class UsModel
        {
            public StatsModel Stats { get; set; }
        }
    }
}
