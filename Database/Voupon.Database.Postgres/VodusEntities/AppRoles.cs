using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class AppRoles
    {
        public AppRoles()
        {
            Permission = new HashSet<Permissions>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Permissions> Permission { get; set; }
    }
}
