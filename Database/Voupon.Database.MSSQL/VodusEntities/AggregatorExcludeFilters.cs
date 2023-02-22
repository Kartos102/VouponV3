using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class AggregatorExcludeFilters
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; }
        public bool IsEnabled { get; set; }
    }
}
