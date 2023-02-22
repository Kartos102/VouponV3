using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.Postcodes.Models
{
    public class PostcodeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DistrictId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string LastUpdatedByUser { get; set; }
    }
}
