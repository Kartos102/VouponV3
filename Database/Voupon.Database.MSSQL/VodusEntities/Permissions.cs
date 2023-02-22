using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Permissions
    {
        public Permissions()
        {
            AppRolePermissions = new HashSet<AppRolePermissions>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<AppRolePermissions> AppRolePermissions { get; set; }
    }
}
