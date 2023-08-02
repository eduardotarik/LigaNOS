using LigaNOS.Data.Entities;
using LigaNOS.Models;
using System;

namespace LigaNOS.Helpers
{
    public interface IConverterHelper
    {
        Team ToTeam(TeamViewModel model, Guid imageId, bool isNew);

        TeamViewModel ToTeamViewModel(Team team);

        Player ToPlayer(PlayerViewModel model, Guid pictureId, bool isNew);

        PlayerViewModel ToPlayerViewModel(Player player);
    }
}
