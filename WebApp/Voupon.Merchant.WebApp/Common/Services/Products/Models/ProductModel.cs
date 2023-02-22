using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Common.Services.Products.Models
{
    public class ProductModel
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string MerchantCode { get; set; }
        public string MerchantName { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string AdditionInfo { get; set; }
        public string FinePrintInfo { get; set; }
        public string RedemptionInfo { get; set; }
        public string ImageFolderUrl { get; set; }
        public int? ProductCategoryId { get; set; }
        public string ProductCategory { get; set; }
        public int? ProductSubCategoryId { get; set; }
        public string ProductSubCategory { get; set; }
        public int? DealTypeId { get; set; }
        public string DealType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Price { get; set; }
        public decimal? ActualPriceForVpoints { get; set; }
        public decimal? DiscountedPrice { get; set; }
        public int? DiscountRate { get; set; }
        public int? PointsRequired { get; set; }
        public int? AvailableQuantity { get; set; }
        public int? DealExpirationId { get; set; }
        public int? LuckyDrawId { get; set; }
        public int StatusTypeId { get; set; }
        public string StatusType { get; set; }
        public string StatusTypePendingChanges { get; set; }
        public bool IsActivated { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPublished { get; set; }
        public bool IsDiscountedPriceEnabled { get; set; }
        public bool IsProductVariationEnabled { get; set; }

        public bool IsShareShippingDifferentItem { get; set; }

        public int ShareShippingCostSameItem { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUser { get; set; }
        public int? TotalBought { get; set; }
        public string Remarks { get; set; }
        public decimal? DefaultCommission { get; set; }

        public Guid? ThirdPartyTypeId { get; set; }
        public Guid? ThirdPartyProductId { get; set; }
        public string RedemptionType { get; set; }
    }
}
