using LigaNOS.Data.Entities;
using LigaNOS.Models;

namespace LigaNOS.Helpers
{
    public interface IConverterHelper
    {
        Team ToTeam(TeamViewModel model, string path, bool isNew);

        TeamViewModel ToTeamViewModel(Team team);

        Player ToPlayer(PlayerViewModel model, string path, bool isNew);

        PlayerViewModel ToPlayerViewModel(Player player);
    }
}
