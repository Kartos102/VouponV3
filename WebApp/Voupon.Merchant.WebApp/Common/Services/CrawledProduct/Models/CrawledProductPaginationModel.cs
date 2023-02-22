using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Common.Services.CrawledProduct.Models
{
    public class CrawledProductPaginationModel
    {
        public List<CrawledProductModel> Crawled { get; set; }

        public Pagination Pagination { get; set; }
    }


    public class Pagination { 
        public int Limit { get; set; }
        public int Page { get; set; }
        public int TotalPage { get; set; }
        public int TotalData { get; set; }
    }
}
