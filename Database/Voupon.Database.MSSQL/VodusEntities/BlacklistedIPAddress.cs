using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class BlacklistedIPAddress
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; }
        public int PartnerWebsiteId { get; set; }
        public byte StatusId { get; set; }
        public int TotalBannedCount { get; set; }
        public string Remark { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastBannedAt { get; set; }
        public DateTime? LastUnBannedAt { get; set; }
        public DateTime? LastCheckedAt { get; set; }
    }
}
