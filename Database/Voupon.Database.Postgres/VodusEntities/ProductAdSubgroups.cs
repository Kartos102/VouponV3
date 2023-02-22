using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class ProductAdSubgroups
    {
        public int Id { get; set; }
        public int ProductAdId { get; set; }
        public string SubgroupId { get; set; } = null!;
        public int AdImpressionCount { get; set; }
        public int AdClickCount { get; set; }
        public decimal? CTR { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ProductAds ProductAd { get; set; } = null!;
        public virtual SubgroupsV2 Subgroup { get; set; } = null!;
    }
}
