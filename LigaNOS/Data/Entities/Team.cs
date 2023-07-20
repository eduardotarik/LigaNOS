using System;
using System.ComponentModel.DataAnnotations;

namespace LigaNOS.Data.Entities
{
    public class Team
    {
        public int Id { get; set; }

        public string Emblem { get; set; }

        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime Founded { get; set; }

        public string Country { get; set; }

        public string City { get; set; }

        public string Stadium { get; set; }

        
    }
}
