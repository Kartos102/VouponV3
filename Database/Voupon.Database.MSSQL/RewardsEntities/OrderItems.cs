using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class OrderItems
    {
        public OrderItems()
        {
            DeliveryRedemptionTokens = new HashSet<DeliveryRedemptionTokens>();
            DigitalRedemptionTokens = new HashSet<DigitalRedemptionTokens>();
            FinanceTransaction = new HashSet<FinanceTransaction>();
            InStoreRedemptionTokens = new HashSet<InStoreRedemptionTokens>();
            ProductReview = new HashSet<ProductReview>();
            Refunds = new HashSet<Refunds>();
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public int MerchantId { get; set; }
        public string MerchantDisplayName { get; set; }
        public decimal Commision { get; set; }
        public decimal Price { get; set; }
        public int Points { get; set; }
        public string ProductDetail { get; set; }
        public int ExpirationTypeId { get; set; }
        public string ProductTitle { get; set; }
        public string ProductImageFolderUrl { get; set; }
        public byte Status { get; set; }
        public string ShortId { get; set; }
        public string VariationText { get; set; }
        public bool IsVariationProduct { get; set; }
        public int? VariationId { get; set; }
        public decimal DiscountedAmount { get; set; }
        public decimal SubtotalPrice { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal ShippingCostSubTotal { get; set; }
        public decimal ShippingCostDiscount { get; set; }
        public decimal? VPointsMultiplier { get; set; }
        public decimal? VPointsMultiplierCap { get; set; }

        public virtual Orders Order { get; set; }
        public virtual Products Product { get; set; }
        public virtual ICollection<DeliveryRedemptionTokens> DeliveryRedemptionTokens { get; set; }
        public virtual ICollection<DigitalRedemptionTokens> DigitalRedemptionTokens { get; set; }
        public virtual ICollection<FinanceTransaction> FinanceTransaction { get; set; }
        public virtual ICollection<InStoreRedemptionTokens> InStoreRedemptionTokens { get; set; }
        public virtual ICollection<ProductReview> ProductReview { get; set; }
        public virtual ICollection<Refunds> Refunds { get; set; }
    }
}
