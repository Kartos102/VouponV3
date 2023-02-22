using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Fingerprints
    {
        public Guid Id { get; set; }
        public string VisitorId { get; set; } = null!;
        public Guid DeviceId { get; set; }
        public int MemberProfileId { get; set; }
        public int PartnerWebsiteId { get; set; }
        public string? Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ComponentsJSON { get; set; }
        public DateTime? LastCCAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
