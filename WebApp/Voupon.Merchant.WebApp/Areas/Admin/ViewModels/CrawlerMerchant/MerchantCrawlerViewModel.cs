using System;

namespace Voupon.Merchant.WebApp.Areas.Admin.ViewModels.CrawlerMerchant
{
    public class MerchantCrawlerViewModel
    {
        public Guid Id { get; set; }
        public string MerchantName { get; set; }
        public string Url { get; set; }
        public byte ExternalTypeId { get; set; }
        public bool StatusId { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdateAt { get; set; }
    }
}
