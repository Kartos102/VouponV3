using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class UserRoles
    {
        public string Id { get; set; } = null!;

        public virtual Roles Role { get; set; } = null!;
        public virtual Users User { get; set; } = null!;
    }
}
