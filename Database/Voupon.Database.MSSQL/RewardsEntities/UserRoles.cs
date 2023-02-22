using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class UserRoles
    {
        public int? MerchantId { get; set; }

        public virtual Merchants Merchant { get; set; }
        public virtual Roles Role { get; set; }
        public virtual Users User { get; set; }
    }
}
