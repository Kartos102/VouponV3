using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class PartnerWebsiteVisits
    {
        public int Id { get; set; }
        public int PartnerWebsitesId { get; set; }
        public int DemographicValueId { get; set; }
        public int VisitCount { get; set; }
        public int Type1ResponseCount { get; set; }
        public int Type1CloseCount { get; set; }
        public int Type2ResponseCount { get; set; }
        public int Type2CloseCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public int Type1CCCount { get; set; }
        public int Type2CCCount { get; set; }

        public virtual DemographicValues DemographicValue { get; set; } = null!;
        public virtual PartnerWebsites PartnerWebsites { get; set; } = null!;
    }
}
