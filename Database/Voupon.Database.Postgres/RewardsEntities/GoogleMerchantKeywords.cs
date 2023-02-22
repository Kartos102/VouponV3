using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class GoogleMerchantKeywords
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; } = null!;
        public int TotalListing { get; set; }
        public string SortBy { get; set; } = null!;
        public string Language { get; set; } = null!;
    }
}
