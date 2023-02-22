using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class UserLogins
    {

        public virtual Users User { get; set; }
    }
}
