namespace VainBotDiscord.OverwatchModels
{
    public class StatsModel
    {
        public CompetitiveModel Competitive { get; set; }
    }

    public class CompetitiveModel
    {
        public OverallStatsModel OverallStats { get; set; }
    }

    public class OverallStatsModel
    {
        public int WinRate { get; set; }
        public int Level { get; set; }
        public int Prestige { get; set; }
        public string Avatar { get; set; }
        public int Wins { get; set; }
        public int? Games { get; set; }
        public int Comprank { get; set; }
        public int? Losses { get; set; }
    }
}
