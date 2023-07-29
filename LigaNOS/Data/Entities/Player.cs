using System.ComponentModel.DataAnnotations;

namespace LigaNOS.Data.Entities
{
    public class Player : IEntity
    {
        public int Id { get; set; }

        public string Picture { get; set; }

        [Required]
        public string Name { get; set; }

        public int Age { get; set; }

        [Required]
        public string Position { get; set; }

        [Required]
        [Display(Name = "Team Name")]
        public string TeamName { get; set; }

        public CustomUser User { get; set; }

        public string ImageFullPath
        {
            get
            {
                if (string.IsNullOrEmpty(Picture))
                {
                    return null;
                }

                return $"https://localhost:44354{Picture.Substring(1)}";
            }
        }
    }
}

