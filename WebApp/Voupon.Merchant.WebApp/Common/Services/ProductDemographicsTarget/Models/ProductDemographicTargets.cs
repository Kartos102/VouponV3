using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models
{
    public class ProductDemographicTargets
    {
        public int Id { get; set; }
        public int DemographicTypeId { get; set; }
        public int DemographicValue { get; set; }
    }
}
