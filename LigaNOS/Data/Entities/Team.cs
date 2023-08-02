using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LigaNOS.Data.Entities
{
    public class Team : IEntity
    {
        public int Id { get; set; }

        public Guid ImageId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Founded { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string Stadium { get; set; }

        public User User { get; set; }

        public string ImageFullPath => ImageId == Guid.Empty
            ? $"https://liganos.azurewebsites.net/images/no_image.png"
            : $"https://liganostorage.blob.core.windows.net/emblems/{ImageId}";
    }
}
