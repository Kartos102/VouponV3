using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Permissions
    {
        public Permissions()
        {
            AppRole = new HashSet<AppRoles>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<AppRoles> AppRole { get; set; }
    }
}
