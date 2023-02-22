using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class AppRoles
    {
        public AppRoles()
        {
            AppRolePermissions = new HashSet<AppRolePermissions>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AppRolePermissions> AppRolePermissions { get; set; }
    }
}
