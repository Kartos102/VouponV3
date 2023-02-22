using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class PartnerProfiles
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public int PartnerWebsiteId { get; set; }

        public virtual Users User { get; set; } = null!;
    }
}
