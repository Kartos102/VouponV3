using System;

namespace Voupon.Merchant.WebApp.Common.Services.CrawlerMerchant.Models
{
    public class CrawlerMerchantModel
    {
        public Guid Id { get; set; }
        public string MerchantName { get; set; }
        public string Url { get; set; }
        public short ExternalTypeId { get; set; }
        public short StatusId { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdateAt { get; set; }
    }
}
