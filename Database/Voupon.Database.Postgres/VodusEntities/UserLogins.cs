using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class UserLogins
    {

        public virtual Users User { get; set; } = null!;
    }
}
