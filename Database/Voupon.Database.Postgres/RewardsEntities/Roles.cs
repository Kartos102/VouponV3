using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class Roles
    {
        public Roles()
        {
            RoleClaims = new HashSet<RoleClaims>();
            UserRoles = new HashSet<UserRoles>();
        }

        public virtual ICollection<RoleClaims> RoleClaims { get; set; }
        public virtual ICollection<UserRoles> UserRoles { get; set; }
    }
}
