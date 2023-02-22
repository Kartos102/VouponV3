using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class AppRolePermissions
    {
        public int AppRoleId { get; set; }
        public int PermissionId { get; set; }

        public virtual AppRoles AppRole { get; set; }
        public virtual Permissions Permission { get; set; }
    }
}
