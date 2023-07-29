using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LigaNOS.Data.Entities
{
    public class Team : IEntity
    {
        public int Id { get; set; }

        public string Emblem { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Founded { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string Stadium { get; set; }

        public CustomUser User { get; set; }

        public string ImageFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(Emblem))
                {
                    return null;
                }

                return $"https://localhost:44354{Emblem.Substring(1)}";
            }
        }
    }
}
