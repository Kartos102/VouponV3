using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Common.Services.Provinces.Models
{
    public class ProvinceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string LastUpdatedByUser { get; set; }
    }
}
