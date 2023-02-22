using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Products.ViewModels
{
    public class NewProductViewModel
    {
        [Required(ErrorMessage = "Please input product name")]
        [Display(Name = "Product name")]
        public string Title { get; set; }

        public int MerchantId { get; set; }

    }
}
