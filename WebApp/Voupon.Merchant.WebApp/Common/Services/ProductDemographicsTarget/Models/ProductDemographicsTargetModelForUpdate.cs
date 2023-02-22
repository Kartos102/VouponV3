using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models
{
    public class ProductDemographicsTargetModelForUpdate
    {
        public string ProductTargetsText { get; set; }
        public List<ProductDemographicTargets> ProductDemographicTargets { get; set; }
    }
}
