using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MemberProfileSubgroups
    {
        public int SubgroupId { get; set; }
        public long MemberProfileId { get; set; }

        public virtual Subgroups Subgroup { get; set; }
    }
}
