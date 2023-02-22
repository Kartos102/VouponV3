using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class ConsoleMerchantJSON
    {
        public Guid Id { get; set; }
        public string URL { get; set; } = null!;
        public string? MerchantUsername { get; set; }
        public string? PageUrl { get; set; }
        public short ExternalTypeId { get; set; }
        public string ExternalId { get; set; } = null!;
        public short StatusId { get; set; }
        public string JSON { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
