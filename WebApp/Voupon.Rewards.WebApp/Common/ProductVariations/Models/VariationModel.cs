using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Common.Services.ProductVariations.Models
{
    public class VariationModel
    {
        public int ProductId { get; set; }
        public List<VariationList> VariationList { get; set; }
        public List<ProductVariationDetailsList> ProductVariationDetailsList { get; set; }
    }
}
