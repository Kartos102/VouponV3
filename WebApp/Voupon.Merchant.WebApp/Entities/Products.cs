using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class Products
    {
        public Products()
        {
            DealExpirations = new HashSet<DealExpirations>();
            LuckyDraws = new HashSet<LuckyDraws>();
            ProductOutlets = new HashSet<ProductOutlets>();
        }

        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string AdditionInfo { get; set; }
        public string FinePrintInfo { get; set; }
        public string RedemptionInfo { get; set; }
        public string ImageUrl { get; set; }
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

        public virtual DealExpirations DealExpiration { get; set; }
        public virtual DealTypes DealType { get; set; }
        public virtual LuckyDraws LuckyDraw { get; set; }
        public virtual Merchants Merchant { get; set; }
        public virtual ProductCategories ProductCategory { get; set; }
        public virtual ProductSubCategories ProductSubCategory { get; set; }
        public virtual StatusTypes StatusType { get; set; }
        public virtual ICollection<DealExpirations> DealExpirations { get; set; }
        public virtual ICollection<LuckyDraws> LuckyDraws { get; set; }
        public virtual ICollection<ProductOutlets> ProductOutlets { get; set; }
    }
}
