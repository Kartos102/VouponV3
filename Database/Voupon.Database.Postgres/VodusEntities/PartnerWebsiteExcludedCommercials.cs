using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class PartnerWebsiteExcludedCommercials
    {
        public int Id { get; set; }
        public int PartnerWebsiteId { get; set; }
        public int CommercialId { get; set; }
        public bool IsActive { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;

        public virtual Commercials Commercial { get; set; } = null!;
    }
}
