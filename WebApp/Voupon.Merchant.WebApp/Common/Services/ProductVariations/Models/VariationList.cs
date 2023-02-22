using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.ProductVariations.Models
{
    public class VariationList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProductId { get; set; }
        public bool IsFirstVariation { get; set; }
        public List<VariationOptions> VariationOptions { get; set; }

    }
}
