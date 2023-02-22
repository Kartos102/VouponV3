using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class BlacklistedIPAddress
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; } = null!;
        public int PartnerWebsiteId { get; set; }
        public short StatusId { get; set; }
        public int TotalBannedCount { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastBannedAt { get; set; }
        public DateTime? LastUnBannedAt { get; set; }
        public DateTime? LastCheckedAt { get; set; }
    }
}
