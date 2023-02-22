using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Districts
    {
        public Districts()
        {
            Areas = new HashSet<Areas>();
            MemberProfiles = new HashSet<MemberProfiles>();
        }

        public short Id { get; set; }
        public short? StateId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }

        public virtual States State { get; set; }
        public virtual ICollection<Areas> Areas { get; set; }
        public virtual ICollection<MemberProfiles> MemberProfiles { get; set; }
    }
}
