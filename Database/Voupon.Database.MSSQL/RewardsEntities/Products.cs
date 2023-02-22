using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class Products
    {
        public Products()
        {
            CartProducts = new HashSet<CartProducts>();
            DealExpirations = new HashSet<DealExpirations>();
            DeliveryRedemptionTokens = new HashSet<DeliveryRedemptionTokens>();
            DigitalRedemptionTokens = new HashSet<DigitalRedemptionTokens>();
            FinanceTransaction = new HashSet<FinanceTransaction>();
            InStoreRedemptionTokens = new HashSet<InStoreRedemptionTokens>();
            LuckyDraws = new HashSet<LuckyDraws>();
            OrderItems = new HashSet<OrderItems>();
            ProductDemographicsTarget = new HashSet<ProductDemographicsTarget>();
            ProductDiscounts = new HashSet<ProductDiscounts>();
            ProductOutlets = new HashSet<ProductOutlets>();
            ProductReview = new HashSet<ProductReview>();
            ProductShippingCost = new HashSet<ProductShippingCost>();
            ProductVariation = new HashSet<ProductVariation>();
            Variations = new HashSet<Variations>();
            ProductCarousel = new HashSet<ProductCarousel>();
        }

        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string AdditionInfo { get; set; }
        public string FinePrintInfo { get; set; }
        public string RedemptionInfo { get; set; }
        public int? ProductCategoryId { get; set; }
        public int? ProductSubCategoryId { get; set; }
        public int? DealTypeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Price { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int? DiscountRate { get; set; }
        public int? PointsRequired { get; set; }
        public int? AvailableQuantity { get; set; }
        public int? DealExpirationId { get; set; }
        public int? LuckyDrawId { get; set; }
        public int StatusTypeId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUser { get; set; }
        public int? TotalBought { get; set; }
        public string Remarks { get; set; }
        public bool IsPublished { get; set; }
        public string ImageFolderUrl { get; set; }
        public string PendingChanges { get; set; }
        public decimal? DefaultCommission { get; set; }
        public Guid? ThirdPartyTypeId { get; set; }
        public Guid? ThirdPartyProductId { get; set; }
        public decimal Rating { get; set; }
        public decimal? ActualPriceForVpoints { get; set; }
        public int TotalRatingCount { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsDiscountedPriceEnabled { get; set; }
        public bool IsProductVariationEnabled { get; set; }
        public bool IsShareShippingDifferentItem { get; set; }
        public int ShareShippingCostSameItem { get; set; }

        public bool IsExternal { get; set; }
        public string ExternalUrl { get; set; }
        public byte? ExternalTypeId { get; set; }
        public string ExternalId { get; set; }
        public string ExternalMerchantId { get; set; }

        public virtual DealExpirations DealExpiration { get; set; }
        public virtual DealTypes DealType { get; set; }
        public virtual LuckyDraws LuckyDraw { get; set; }
        public virtual Merchants Merchant { get; set; }
        public virtual ProductCategories ProductCategory { get; set; }
        public virtual ProductSubCategories ProductSubCategory { get; set; }
        public virtual StatusTypes StatusType { get; set; }
        public virtual ICollection<CartProducts> CartProducts { get; set; }
        public virtual ICollection<DealExpirations> DealExpirations { get; set; }
        public virtual ICollection<DeliveryRedemptionTokens> DeliveryRedemptionTokens { get; set; }
        public virtual ICollection<DigitalRedemptionTokens> DigitalRedemptionTokens { get; set; }
        public virtual ICollection<FinanceTransaction> FinanceTransaction { get; set; }
        public virtual ICollection<InStoreRedemptionTokens> InStoreRedemptionTokens { get; set; }
        public virtual ICollection<LuckyDraws> LuckyDraws { get; set; }
        public virtual ICollection<OrderItems> OrderItems { get; set; }
        public virtual ICollection<ProductDemographicsTarget> ProductDemographicsTarget { get; set; }
        public virtual ICollection<ProductDiscounts> ProductDiscounts { get; set; }
        public virtual ICollection<ProductOutlets> ProductOutlets { get; set; }
        public virtual ICollection<ProductReview> ProductReview { get; set; }
        public virtual ICollection<ProductShippingCost> ProductShippingCost { get; set; }
        public virtual ICollection<ProductVariation> ProductVariation { get; set; }
        public virtual ICollection<Variations> Variations { get; set; }
        public virtual ICollection<ProductCarousel> ProductCarousel { get; set; }
    }
}
