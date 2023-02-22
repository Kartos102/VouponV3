using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class OrderItemExternal
    {
        public OrderItemExternal()
        {
            RefundsExternalOrderItems = new HashSet<RefundsExternalOrderItems>();
        }

        public Guid Id { get; set; }
        public Guid OrderShopExternalId { get; set; }
        public string ExternalItemId { get; set; }
        public string ExternalUrl { get; set; }
        public string ProductCartPreviewSmallImageURL { get; set; }
        public string ProductTitle { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string LastUpdatedByUser { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public string VariationText { get; set; }
        public string ProductVariation { get; set; }
        public string JsonData { get; set; }
        public bool IsVariationProduct { get; set; }
        public int DealExpirationId { get; set; }
        public int CartProductType { get; set; }
        public int Quantity { get; set; }
        public string DiscountName { get; set; }
        public int? DiscountTypeId { get; set; }
        public decimal? DiscountPriceValue { get; set; }
        public int? DiscountPointRequired { get; set; }
        public decimal Price { get; set; }
        public int Points { get; set; }
        public byte OrderItemExternalStatus { get; set; }
        public string ShortId { get; set; }
        public decimal DiscountedAmount { get; set; }
        public decimal SubtotalPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal? VPointsMultiplier { get; set; }
        public decimal? VPointsMultiplierCap { get; set; }

        public virtual OrderShopExternal OrderShopExternal { get; set; }
        public virtual ICollection<RefundsExternalOrderItems> RefundsExternalOrderItems { get; set; }
    }
}
