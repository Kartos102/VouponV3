using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class DeliveryRedemptionTokens
    {
        public int Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public int MerchantId { get; set; }
        public Guid OrderItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string CourierProvider { get; set; }
        public string Token { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string RedemptionInfo { get; set; }
        public bool IsRedeemed { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RedeemedAt { get; set; }
        public decimal? Revenue { get; set; }
        public string Email { get; set; }
        public DateTime? UpdateTokenAt { get; set; }

        public virtual Merchants Merchant { get; set; }
        public virtual OrderItems OrderItem { get; set; }
        public virtual Products Product { get; set; }
    }
}
