using LigaNOS.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace LigaNOS.Models
{
    public class TeamViewModel : Team
    {
        [Display(Name = "Emblem")]
        public IFormFile ImageFile { get; set; }

    }
}
