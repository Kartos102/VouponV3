using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class CartProductExternal
    {
        public Guid Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string ProductCartPreviewSmallImageURL { get; set; } = null!;
        public string ExternalShopName { get; set; } = null!;
        public string ProductTitle { get; set; } = null!;
        public string ExternalItemId { get; set; } = null!;
        public string ExternalShopId { get; set; } = null!;
        public short ExternalTypeId { get; set; }
        public string ExternalUrl { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int OrderQuantity { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public string? VariationText { get; set; }
        public string? ProductVariation { get; set; }
        public string? JsonData { get; set; }
        public bool IsVariationProduct { get; set; }
        public int DealExpirationId { get; set; }
        public int CartProductType { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ProductDiscountedPrice { get; set; }
        public string? AdditionalDiscountName { get; set; }
        public int? AdditionalDiscountTypeId { get; set; }
        public decimal? AdditionalDiscountPriceValue { get; set; }
        public int? AdditionalDiscountPointRequired { get; set; }
        public decimal? VPointsMultiplier { get; set; }
        public decimal? VPointsMultiplierCap { get; set; }
    }
}
