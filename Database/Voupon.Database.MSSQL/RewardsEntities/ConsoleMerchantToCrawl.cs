using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class ConsoleMerchantToCrawl
    {
        public Guid Id { get; set; }
        public string URL { get; set; }
        public string MerchantName { get; set; }
        public byte ExternalTypeId { get; set; }
        public byte StatusId { get; set; }
        public byte CurrentProcess { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public DateTime? LastCrawledAt { get; set; }
    }
}
