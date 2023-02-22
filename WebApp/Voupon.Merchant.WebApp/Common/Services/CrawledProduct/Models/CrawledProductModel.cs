using System;

namespace Voupon.Merchant.WebApp.Common.Services.CrawledProduct.Models
{
    public class CrawledProductModel
    {
        public Guid Id { get; set; }

        public string MerchantName { get; set; }
        public string Url { get; set; }
        public string PageUrl { get; set; }
        public string ItemName { get; set; }
        public string ExternalId { get; set; }
        public string ExternalMerchantId { get; set; }
        public short StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdateAt { get; set; }
    }
}
