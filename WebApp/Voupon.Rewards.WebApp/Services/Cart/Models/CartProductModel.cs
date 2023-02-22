using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;

namespace Voupon.Rewards.WebApp.Services.Cart.Models
{
    public class CartProductModel
    {
        public int Id { get; set; }
        public string ProductCartPreviewSmallImage { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal DiscountRate { get; set; }
        public decimal Price { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPrice { get; set; }
        public int AvailableQuantity { get; set; }
        public int OrderQuantity { get; set; }
        public int PointsRequired { get; set; }
        public bool IsVariationProduct { get; set; }
        public string Title { get; set; }
        public int TypeId { get; set; }
        public int CartProductType { get; set; }
        public string VariationText { get; set; }
        public int ProductId { get; set; }
        public int VariationId { get; set; }
        public CartMerchantDetails Merchant { get; set; }
        public AdditionalDiscount AdditionalDiscount { get; set; }
        public DealExpiration DealExpiration { get; set; }

        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public byte ExternalTypeId { get; set; }

        public string ExternalId { get; set; }
        public string ExternalShopName { get; set; }

        public string ExternalShopUrl { get; set; }
        public string ExternalItemUrl { get; set; }
        public DateTime AddedAt { get; set; }
    }
    public class CartMerchantDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        public string ExternalId { get; set; }

        public int TypeId { get; set; }
    } 
    public class AdditionalDiscount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PointsRequired { get; set; }
        public int Type { get; set; }
        public decimal Value { get; set; }

        public decimal ExternalItemDiscountPercentage { get; set; }

        public decimal VPointsMultiplier { get; set; }
        public decimal VPointsMultiplierCap { get; set; }
    }
    public class DealExpiration
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public int Type { get; set; }
        public int? TotalValidDays { get; set; }
    }
}
