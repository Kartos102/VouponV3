using System.Collections.Generic;
using Voupon.Merchant.WebApp.Common.Services.CrawledProduct.Models;

namespace Voupon.Merchant.WebApp.Common.Services.CrawlerMerchant.Models
{
    public class CrawledMerchantPaginationModel
    {
        public List<CrawlerMerchantModel> Crawled { get; set; }
        public Pagination Pagination { get; set; }      
    }

}
