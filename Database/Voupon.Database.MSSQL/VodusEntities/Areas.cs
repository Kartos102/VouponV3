using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Areas
    {
        public Areas()
        {
            MemberProfiles = new HashSet<MemberProfiles>();
        }

        public short Id { get; set; }
        public short DistrictId { get; set; }
        public string AreaCode { get; set; }
        public string AreaName { get; set; }

        public virtual Districts District { get; set; }
        public virtual ICollection<MemberProfiles> MemberProfiles { get; set; }
    }
}
