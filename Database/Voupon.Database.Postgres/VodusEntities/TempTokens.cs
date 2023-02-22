using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class TempTokens
    {
        public Guid Id { get; set; }
        public long? MemberProfileId { get; set; }
        public string? Token { get; set; }
        public int PartnerWebsiteId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
