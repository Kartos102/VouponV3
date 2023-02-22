using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class MobileDevices
    {
        public Guid VodusId { get; set; }
        public string DeviceId { get; set; } = null!;
        public short OS { get; set; }
        public long MemberProfileId { get; set; }
        public string Token { get; set; } = null!;
        public int PartnerWebsiteId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
