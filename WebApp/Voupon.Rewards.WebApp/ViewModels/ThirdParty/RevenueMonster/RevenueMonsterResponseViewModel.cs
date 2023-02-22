using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.ViewModels.ThirdParty.RevenueMonster
{
    public class RevenueMonsterResponseViewModel
    {
        public string Code { get; set; }
        public Item Item { get; set; }
    }

    public class Item
    {
        public string CheckoutId { get; set; }
        public string Url { get; set; }
    }
}
