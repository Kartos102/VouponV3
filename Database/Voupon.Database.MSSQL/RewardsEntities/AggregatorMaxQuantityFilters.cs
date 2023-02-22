using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class AggregatorMaxQuantityFilters
    {
        public int Id { get; set; }
        public string Keyword { get; set; }
        public byte StatusId { get; set; }
        public byte MaxQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
