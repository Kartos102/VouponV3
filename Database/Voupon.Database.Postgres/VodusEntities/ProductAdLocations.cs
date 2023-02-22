using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class ProductAdLocations
    {
        public int Id { get; set; }
        public int ProductAdId { get; set; }
        public int AdImpressionCount { get; set; }
        public int AdClickCount { get; set; }
        public decimal? CTR { get; set; }
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public virtual ProductAds ProductAd { get; set; } = null!;
    }
}
