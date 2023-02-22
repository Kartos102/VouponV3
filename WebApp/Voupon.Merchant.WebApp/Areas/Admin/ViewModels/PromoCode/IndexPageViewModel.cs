using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Areas.Admin.ViewModels.PromoCode
{
    public class IndexPageViewModel
    {
        public PromoCodeViewModel AddPromoCodeViewModel { get; set; }
        public PromoCodeViewModel EditPromoCodeViewModel { get; set; }
    }
}
