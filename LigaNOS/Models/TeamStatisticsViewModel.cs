namespace LigaNOS.Models
{
    public class TeamStatisticsViewModel
    {
        public string TeamName { get; set; }
        public int TotalGames { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points => Wins * 3 + Draws;
    }
}
