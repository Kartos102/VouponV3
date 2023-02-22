using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class PartitionGrouping
    {
        public int Id { get; set; }
        public string Summary { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
