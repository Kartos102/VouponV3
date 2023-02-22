using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models
{
    public class ProductDemographicsList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<DemographicsValues> DemographicsValues { get; set; }

    }
}
