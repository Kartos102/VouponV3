using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class UserRoles
    {
        public virtual Roles Role { get; set; }
        public virtual Users User { get; set; }
    }
}
