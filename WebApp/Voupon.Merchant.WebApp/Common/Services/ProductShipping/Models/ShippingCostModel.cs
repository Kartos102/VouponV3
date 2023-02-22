using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.ProductShipping.Models
{
    public class ShippingCostModel
    {
        public int Id { get; set; }
        public int ProductShippingId { get; set; }
        public int ProvinceId { get; set; }
        public string? ProvinceName{ get; set; }
        public decimal Cost { get; set; }
    }
}
