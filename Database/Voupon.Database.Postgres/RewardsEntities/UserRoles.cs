using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class UserRoles
    {
        public int? MerchantId { get; set; }

        public virtual Merchants? Merchant { get; set; }
        public virtual Roles Role { get; set; } = null!;
        public virtual Users User { get; set; } = null!;
    }
}
