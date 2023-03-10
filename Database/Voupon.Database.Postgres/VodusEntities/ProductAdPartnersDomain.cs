using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class ProductAdPartnersDomain
    {
        public ProductAdPartnersDomain()
        {
            ProductAdPartnersDomainWebsites = new HashSet<ProductAdPartnersDomainWebsites>();
        }

        public int Id { get; set; }
        public int ProductAdId { get; set; }
        public int PartnerId { get; set; }
        public int AdImpressionCount { get; set; }
        public int AdClickCount { get; set; }
        public decimal? CTR { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Partners Partner { get; set; } = null!;
        public virtual ProductAds ProductAd { get; set; } = null!;
        public virtual ICollection<ProductAdPartnersDomainWebsites> ProductAdPartnersDomainWebsites { get; set; }
    }
}
