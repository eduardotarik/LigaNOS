using LigaNOS.Data.Entities;
using LigaNOS.Models;
using System.IO;

namespace LigaNOS.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        public Team ToTeam(TeamViewModel model, string path, bool isNew)
        {
            return new Team
            {
                Id = isNew ? 0 : model.Id,
                Emblem = path,
                Name = model.Name,
                Founded = model.Founded,
                Country = model.Country,
                City = model.City,
                Stadium = model.Stadium,
                User = model.User,
            };
        }

        public TeamViewModel ToTeamViewModel(Team team)
        {
            return new TeamViewModel
            {
                Id = team.Id,
                Emblem = team.Emblem,
                Name = team.Name,
                Founded = team.Founded,
                Country = team.Country,
                City = team.City,
                Stadium = team.Stadium,
                User = team.User
            };
        }
        public Player ToPlayer(PlayerViewModel model, string path, bool isNew)
        {
            return new Player
            {
                Id = isNew ? 0 : model.Id,
                Picture = path,
                Name = model.Name,
                Age = model.Age,
                Position = model.Position,
                TeamName = model.TeamName,
                User = model.User,
            };
        }

        public PlayerViewModel ToPlayerViewModel(Player player)
        {
            return new PlayerViewModel
            {
                Id = player.Id,
                Picture = player.Picture,
                Name = player.Name,
                Age = player.Age,
                Position = player.Position,
                TeamName = player.TeamName,
                User = player.User
            };
        }
    }
}
