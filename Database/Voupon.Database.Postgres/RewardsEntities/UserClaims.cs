using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class UserClaims
    {

        public virtual Users User { get; set; } = null!;
    }
}
