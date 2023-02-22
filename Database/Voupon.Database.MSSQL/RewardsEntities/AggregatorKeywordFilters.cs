using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class AggregatorKeywordFilters
    {
        public int Id { get; set; }
        public string Keyword { get; set; }
        public byte StatusId { get; set; }
        public byte ExternalTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
