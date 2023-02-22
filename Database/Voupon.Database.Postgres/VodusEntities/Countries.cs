using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Countries
    {
        public Countries()
        {
            ClientProfiles = new HashSet<ClientProfiles>();
            MasterMemberProfiles = new HashSet<MasterMemberProfiles>();
            States = new HashSet<States>();
        }

        public short Id { get; set; }
        public string ISO2 { get; set; } = null!;
        public string ISO3 { get; set; } = null!;
        public string CodeNumber { get; set; } = null!;
        public string Name { get; set; } = null!;

        public virtual ICollection<ClientProfiles> ClientProfiles { get; set; }
        public virtual ICollection<MasterMemberProfiles> MasterMemberProfiles { get; set; }
        public virtual ICollection<States> States { get; set; }
    }
}
