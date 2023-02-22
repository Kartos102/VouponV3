using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class InprogressR
    {
        public long Id { get; set; }
        public int SubgroupId { get; set; }
        public string SubgroupName { get; set; }
        public long RecordCount { get; set; }
        public decimal RecordValue { get; set; }
        public long RecordIdStartAt { get; set; }
        public long RecordIdEndAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
