using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class PromoCodes
    {
        public PromoCodes()
        {
            PromoCodeSelectedUsers = new HashSet<PromoCodeSelectedUsers>();
        }

        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string PromoCode { get; set; } = null!;
        public string Description { get; set; } = null!;
        public short Status { get; set; }
        public short DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime ExpireOn { get; set; }
        public decimal MinSpend { get; set; }
        public decimal MaxDiscountValue { get; set; }
        public int TotalRedemptionAllowed { get; set; }
        public int TotalRedeemed { get; set; }
        public bool IsFirstTimeUserOnly { get; set; }
        public bool IsNewSignupUserOnly { get; set; }
        public int TotalAllowedPerUser { get; set; }
        public bool IsSelectedUserOnly { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        //Promo code removes shipping cost value 
        public bool IsShipCostDeduct { get; set; }
        public virtual ICollection<PromoCodeSelectedUsers> PromoCodeSelectedUsers { get; set; }
    }
}
