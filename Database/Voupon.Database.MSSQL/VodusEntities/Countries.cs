using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Countries
    {
        public Countries()
        {
            ClientProfiles = new HashSet<ClientProfiles>();
            MasterMemberProfiles = new HashSet<MasterMemberProfiles>();
            MemberProfiles = new HashSet<MemberProfiles>();
            States = new HashSet<States>();
        }

        public short Id { get; set; }
        public string ISO2 { get; set; }
        public string ISO3 { get; set; }
        public string CodeNumber { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ClientProfiles> ClientProfiles { get; set; }
        public virtual ICollection<MasterMemberProfiles> MasterMemberProfiles { get; set; }
        public virtual ICollection<MemberProfiles> MemberProfiles { get; set; }
        public virtual ICollection<States> States { get; set; }
    }
}
