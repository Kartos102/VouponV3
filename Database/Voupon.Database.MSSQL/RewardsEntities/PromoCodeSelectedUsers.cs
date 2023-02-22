using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class PromoCodeSelectedUsers
    {
        public Guid Id { get; set; }
        public Guid PromoCodeId { get; set; }
        public string Email { get; set; }
        public bool IsRedeemed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }

        public virtual PromoCodes PromoCode { get; set; }
    }
}
