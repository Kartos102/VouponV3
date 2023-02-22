using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class AggregatorExcludeMerchants
    {
        public int Id { get; set; }
        public string MerchantId { get; set; } = null!;
        public string MerchantUsername { get; set; } = null!;
        public short StatusId { get; set; }
        public short ExternalTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
