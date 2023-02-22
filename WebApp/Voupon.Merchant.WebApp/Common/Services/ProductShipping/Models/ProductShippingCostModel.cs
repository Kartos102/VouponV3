using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.ProductShipping.Models
{
    public class ProductShippingCostModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int ShippingTypeId { get; set; }
        public decimal ConditionalShippingCost { get; set; }
        public ICollection<ShippingCostModel> ShippingCosts { get; set; }

    }
}
