using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class OrderItemExternal
    {
        public Guid Id { get; set; }
        public Guid OrderShopExternalId { get; set; }
        public string ExternalItemId { get; set; } = null!;
        public string ExternalUrl { get; set; } = null!;
        public string ProductCartPreviewSmallImageURL { get; set; } = null!;
        public string ProductTitle { get; set; } = null!;
        public DateTime? LastUpdatedAt { get; set; }
        public string? LastUpdatedByUser { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public string? VariationText { get; set; }
        public string? ProductVariation { get; set; }
        public string? JsonData { get; set; }
        public bool IsVariationProduct { get; set; }
        public int DealExpirationId { get; set; }
        public int CartProductType { get; set; }
        public int Quantity { get; set; }
        public string? DiscountName { get; set; }
        public int? DiscountTypeId { get; set; }
        public decimal? DiscountPriceValue { get; set; }
        public int? DiscountPointRequired { get; set; }
        public decimal Price { get; set; }
        public int Points { get; set; }
        public short OrderItemExternalStatus { get; set; }
        public string? ShortId { get; set; }
        public decimal DiscountedAmount { get; set; }
        public decimal SubtotalPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal? VPointsMultiplier { get; set; }
        public decimal? VPointsMultiplierCap { get; set; }

        public virtual OrderShopExternal OrderShopExternal { get; set; } = null!;
    }
}
