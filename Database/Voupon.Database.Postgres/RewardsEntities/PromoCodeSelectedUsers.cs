using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class PromoCodeSelectedUsers
    {
        public Guid Id { get; set; }
        public Guid PromoCodeId { get; set; }
        public string Email { get; set; } = null!;
        public bool IsRedeemed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }

        public virtual PromoCodes PromoCode { get; set; } = null!;
    }
}
