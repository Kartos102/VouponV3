using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class BlacklistedIPs
    {
        public Guid Id { get; set; }
        public string IpAddress { get; set; } = null!;
        public string? Remark { get; set; }
        public int TotalRequest { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
