using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class UserClaims
    {

        public virtual Users User { get; set; } = null!;
    }
}
