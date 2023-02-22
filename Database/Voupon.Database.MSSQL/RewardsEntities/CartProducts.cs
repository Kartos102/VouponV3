using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class CartProducts
    {
        public int Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string ProductCartPreviewSmallImageURL { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int OrderQuantity { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public string VariationText { get; set; }
        public bool IsVariationProduct { get; set; }
        public int? VariationId { get; set; }
        public int ProductId { get; set; }
        public int? AdditionalDiscountId { get; set; }
        public int DealExpirationId { get; set; }
        public int MerchantId { get; set; }
        public int CartProductType { get; set; }

        public virtual ProductDiscounts AdditionalDiscount { get; set; }
        public virtual DealExpirations DealExpiration { get; set; }
        public virtual Merchants Merchant { get; set; }
        public virtual Products Product { get; set; }
        public virtual ProductVariation Variation { get; set; }
    }
}
