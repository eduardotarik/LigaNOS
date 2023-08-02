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

        [Required]
        [Display(Name = "Home Score")]
        public int? HomeTeamScore { get; set; }

        [Required]
        [Display(Name = "Away Score")]
        public int? AwayTeamScore { get; set; }

        public bool IsPlayed { get; set; }

        public User User { get; set; }

    }
}
