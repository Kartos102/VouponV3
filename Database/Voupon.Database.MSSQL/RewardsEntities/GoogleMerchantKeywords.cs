using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class GoogleMerchantKeywords
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; }
        public int TotalListing { get; set; }
        public string SortBy { get; set; }
        public string Language { get; set; }
    }
}
