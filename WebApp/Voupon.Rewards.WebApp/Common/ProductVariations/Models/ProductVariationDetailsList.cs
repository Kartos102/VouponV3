using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Common.Services.ProductVariations.Models
{
    public class ProductVariationDetailsList
    {
        public int Id { get; set; }
        public string Sku { get; set; }
        public int AvailableQuantity { get; set; }
        public string Order { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountedPrice { get; set; }
        public bool IsDiscountedPriceEnabled { get; set; }
    }
}
