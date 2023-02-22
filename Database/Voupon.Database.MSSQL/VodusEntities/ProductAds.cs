using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class ProductAds
    {
        public ProductAds()
        {
            ProductAdLocations = new HashSet<ProductAdLocations>();
            ProductAdPartnersDomain = new HashSet<ProductAdPartnersDomain>();
            ProductAdSubgroups = new HashSet<ProductAdSubgroups>();
        }

        public int Id { get; set; }
        public int ProductId { get; set; }
        public int AdImpressionCount { get; set; }
        public int AdClickCount { get; set; }
        public decimal? CTR { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<ProductAdLocations> ProductAdLocations { get; set; }
        public virtual ICollection<ProductAdPartnersDomain> ProductAdPartnersDomain { get; set; }
        public virtual ICollection<ProductAdSubgroups> ProductAdSubgroups { get; set; }
    }
}
