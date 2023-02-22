using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class AggregatorExcludeFilters
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; } = null!;
        public bool IsEnabled { get; set; }
    }
}
