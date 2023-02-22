using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class UserTokens
    {

        public virtual Users User { get; set; }
    }
}
