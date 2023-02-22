using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class CartProductExternal
    {
        public Guid Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string ProductCartPreviewSmallImageURL { get; set; }
        public string ExternalShopName { get; set; }
        public string ProductTitle { get; set; }
        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public byte ExternalTypeId { get; set; }
        public string ExternalUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int OrderQuantity { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public string VariationText { get; set; }
        public string ProductVariation { get; set; }
        public string JsonData { get; set; }
        public bool IsVariationProduct { get; set; }
        public int DealExpirationId { get; set; }
        public int CartProductType { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal ProductDiscountedPrice { get; set; }
        public string AdditionalDiscountName { get; set; }
        public int? AdditionalDiscountTypeId { get; set; }
        public decimal? AdditionalDiscountPriceValue { get; set; }
        public int? AdditionalDiscountPointRequired { get; set; }
        public decimal? VPointsMultiplier { get; set; }
        public decimal? VPointsMultiplierCap { get; set; }
    }
}
