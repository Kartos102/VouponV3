using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models;

namespace Voupon.Merchant.WebApp.Areas.Admin.ViewModels.ProductAds
{
    public class ProductAdsList
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int AdImpressionCount { get; set; }
        public int AdClickCount { get; set; }
        public decimal CTR { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }


        public string MerchantName { get; set; }
        public string ProductTargetsText { get; set; }
        public int MerchantId{ get; set; }
        public string Title { get; set; }
        public decimal? Price { get; set; }
        public int? PointsRequired { get; set; }
        public decimal? ActualPriceForVpoints { get; set; }
        public List<string> ProductAdLocations { get; set; }
        public List<ProductDemographicTargets> ProductDemographicTargets { get; set; }




    }
}
