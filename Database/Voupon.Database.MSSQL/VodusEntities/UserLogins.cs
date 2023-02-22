using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class UserLogins
    {
        public virtual Users User { get; set; }
    }
}
