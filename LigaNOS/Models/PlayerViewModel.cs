using LigaNOS.Data.Entities;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LigaNOS.Models
{
    public class PlayerViewModel : Player
    {

        [Display(Name = "Picture")]
        public IFormFile ImageFile { get; set; }
    }
}
