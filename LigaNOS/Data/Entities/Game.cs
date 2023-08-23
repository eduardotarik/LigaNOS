using LigaNOS.Models;
using System;
using System.ComponentModel.DataAnnotations;


namespace LigaNOS.Data.Entities
{
    public class Game : IEntity
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [Display(Name = "Home Team")]
        public string HomeTeam { get; set; }

        [Display(Name = "Away Team")]
        public string AwayTeam { get; set; }

        [Display(Name = "Home Score")]
        public int? HomeTeamScore { get; set; }

        [Display(Name = "Away Score")]
        public int? AwayTeamScore { get; set; }

        [Display(Name = "Played")]
        public bool IsPlayed { get; set; }

        [Display(Name = "Home")]
        public string? HomeTeamIssuedCard { get; set; }

        [Display(Name = "Away")]
        public string? AwayTeamIssuedCard { get; set; }

        public SeasonStatus Status { get; set; }

        public User User { get; set; }

        public enum SeasonStatus
        {
            NotStarted, // Season has not started
            Active,     // Season is active
            Ended       // Season has ended
        }

        public GameStatus Stat { get; set; }

        public enum GameStatus
        {
            NotStarted, // Season has not started
            Active,     // Season is active
            Ended       // Season has ended
        }
    }
}
