using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class AggregatorKeywordFilters
    {
        public int Id { get; set; }
        public string Keyword { get; set; } = null!;
        public short StatusId { get; set; }
        public short ExternalTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
