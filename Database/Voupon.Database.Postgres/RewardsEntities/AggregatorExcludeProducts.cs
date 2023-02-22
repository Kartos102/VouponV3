using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class AggregatorExcludeProducts
    {
        public int Id { get; set; }
        public string ProductId { get; set; } = null!;
        public string MerchantId { get; set; } = null!;
        public short ExternalTypeId { get; set; }
        public string ProductUrl { get; set; } = null!;
        public short StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
