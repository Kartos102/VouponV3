using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class AggregatorApiUrls
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = null!;
        public short Status { get; set; }
        public bool IsLocalAggregator { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
