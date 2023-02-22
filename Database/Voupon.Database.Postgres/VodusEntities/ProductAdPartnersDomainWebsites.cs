using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class ProductAdPartnersDomainWebsites
    {
        public int Id { get; set; }
        public int ProductAdPartnersDomainId { get; set; }
        public int PartnerWebsiteId { get; set; }
        public int AdImpressionCount { get; set; }
        public int AdClickCount { get; set; }
        public decimal? CTR { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual PartnerWebsites PartnerWebsite { get; set; } = null!;
        public virtual ProductAdPartnersDomain ProductAdPartnersDomain { get; set; } = null!;
    }
}
