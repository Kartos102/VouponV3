using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class MemberProfileSubgroups
    {
        public int SubgroupId { get; set; }
        public int MemberProfileId { get; set; }

        public virtual Subgroups Subgroup { get; set; } = null!;
    }
}
