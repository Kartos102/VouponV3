using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MobileDevices
    {
        public Guid VodusId { get; set; }
        public string DeviceId { get; set; }
        public byte OS { get; set; }
        public long MemberProfileId { get; set; }
        public string Token { get; set; }
        public int PartnerWebsiteId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
