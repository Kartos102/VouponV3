using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ConsoleProductJSON
    {
        public Guid Id { get; set; }
        public string URL { get; set; } = null!;
        public string? PageUrl { get; set; }
        public string? ItemName { get; set; }
        public string ExternalId { get; set; } = null!;
        public string ExternalMerchantId { get; set; } = null!;
        public short ExternalTypeId { get; set; }
        public short StatusId { get; set; }
        public string JSON { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
