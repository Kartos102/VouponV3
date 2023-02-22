using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Common.ShippingCost.Models
{
    public class ProductIdAndVariationIdModel
    {
        public int ProductId { get; set; }
        public int VariationId { get; set; }
        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public byte ExternalTypeId { get; set; }
        public string ExternalVariationText { get; set; }
        public string ProductTitle { get; set; }
        public int Quantity { get; set; }
        public decimal ProductPrice { get; set; }

    }

    public class OrderShippingCostForPoductIdAndVariationIdModel
    {
        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public byte ExternalTypeId { get; set; }
        public string ExternalItemVariationText { get; set; }
        public int ProductId { get; set; }
        public int VariationId { get; set; }
        public decimal OrderShippingCost { get; set; }
        public string ProductTitle { get; set; }

        public decimal ProductPrice { get; set; }
    }

    public class OrderShippingCostsModel
    {
        public decimal TotalShippingCost { get; set; }

        public List<OrderShippingCostForPoductIdAndVariationIdModel> OrderShippingCosts { get; set; }
    }
}
