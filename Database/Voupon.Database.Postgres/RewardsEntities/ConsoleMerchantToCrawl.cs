using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ConsoleMerchantToCrawl
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = null!;
        public string? MerchantName { get; set; }
        public short ExternalTypeId { get; set; }
        public short StatusId { get; set; }
        public short CurrentProcess { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        
        public DateTime LastCrawledAt { get; set; }
    }
}
