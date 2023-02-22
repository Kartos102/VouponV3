using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models
{
    public class ProductDemographicsTargetModel
    {
        public int ProductId { get; set; }
        public List<ProductDemographicsList> ProductDemographicsList { get; set; }
        public List<ProductDemographicTargets> ProductDemographicTargets { get; set; }
    }
}
