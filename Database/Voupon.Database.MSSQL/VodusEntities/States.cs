using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class States
    {
        public States()
        {
            Districts = new HashSet<Districts>();
            MemberProfiles = new HashSet<MemberProfiles>();
        }

        public short Id { get; set; }
        public short? CountryId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Name2 { get; set; }

        public virtual Countries Country { get; set; }
        public virtual ICollection<Districts> Districts { get; set; }
        public virtual ICollection<MemberProfiles> MemberProfiles { get; set; }
    }
}
