using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class AspNetRoles
    {
        public AspNetRoles()
        {
            User = new HashSet<AspNetUsers>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;

        public virtual ICollection<AspNetUsers> User { get; set; }
    }
}
