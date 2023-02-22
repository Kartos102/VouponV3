using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class AggregatorExculdeFilters
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; }
        public bool IsEnabled { get; set; }
    }
}
