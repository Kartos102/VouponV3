using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Services.Home.Models
{
    public class UserPrefrencesModel
    {
        public int? CategoryId{ get; set; }
        public string CategoryName { get; set; }
        public int? ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public string SearchText { get; set; }

        public byte IsSearch { get; set; }
    }

}
