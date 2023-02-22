using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class AggregatorApiUrls
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public byte StatusId { get; set; }
        public byte TypeId { get; set; }
        public bool IsLocalAggregator { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }
}
