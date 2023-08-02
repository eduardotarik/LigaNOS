using LigaNOS.Data.Entities;
using LigaNOS.Models;
using System;
using System.IO;

namespace LigaNOS.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        public Team ToTeam(TeamViewModel model, Guid imageId, bool isNew)
        {
            return new Team
            {
                Id = isNew ? 0 : model.Id,
                ImageId = imageId,
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
                ImageId = team.ImageId,
                Name = team.Name,
                Founded = team.Founded,
                Country = team.Country,
                City = team.City,
                Stadium = team.Stadium,
                User = team.User
            };
        }
        public Player ToPlayer(PlayerViewModel model, Guid pictureId, bool isNew)
        {
            return new Player
            {
                Id = isNew ? 0 : model.Id,
                PictureId = pictureId,
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
                PictureId = player.PictureId,
                Name = player.Name,
                Age = player.Age,
                Position = player.Position,
                TeamName = player.TeamName,
                User = player.User
            };
        }
    }
}
