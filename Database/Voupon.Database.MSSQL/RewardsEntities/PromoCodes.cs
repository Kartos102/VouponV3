using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class PromoCodes
    {
        public PromoCodes()
        {
            PromoCodeSelectedUsers = new HashSet<PromoCodeSelectedUsers>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string PromoCode { get; set; }
        public string Description { get; set; }
        public byte Status { get; set; }
        public byte DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public DateTime ExpireOn { get; set; }
        public decimal MinSpend { get; set; }
        public decimal MaxDiscountValue { get; set; }
        public int TotalRedemptionAllowed { get; set; }
        public int TotalRedeemed { get; set; }
        public bool IsFirstTimeUserOnly { get; set; }
        public bool IsNewSignupUserOnly { get; set; }
        public bool IsSelectedUserOnly { get; set; }
        public int TotalAllowedPerUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }

        public virtual ICollection<PromoCodeSelectedUsers> PromoCodeSelectedUsers { get; set; }
    }
}
