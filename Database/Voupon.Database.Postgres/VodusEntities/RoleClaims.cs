using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class RoleClaims
    {

        public virtual Roles Role { get; set; } = null!;
    }
}
