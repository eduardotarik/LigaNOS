using System;
using System.ComponentModel.DataAnnotations;

namespace LigaNOS.Data.Entities
{
    public class Player : IEntity
    {
        public int Id { get; set; }

        [Display(Name = "Picture")]
        public Guid PictureId{ get; set; }

        [Required]
        public string Name { get; set; }

        public int Age { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; }

        public User User { get; set; }

        public string ImageFullPath => PictureId == Guid.Empty
            ? $"https://liganos.azurewebsites.net/images/no_image.png"
            : $"https://liganostorage.blob.core.windows.net/pictures/{PictureId}";

    }
}

