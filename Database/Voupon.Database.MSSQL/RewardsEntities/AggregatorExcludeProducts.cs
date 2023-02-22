using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class AggregatorExcludeProducts
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string MerchantId { get; set; }
        public byte ExternalTypeId { get; set; }
        public string ProductUrl { get; set; }
        public byte StatusId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
