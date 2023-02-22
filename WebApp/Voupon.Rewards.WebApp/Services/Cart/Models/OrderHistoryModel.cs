using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Services.Cart.Models
{
    public class OrderHistoryModel
    {
        public int SortId { get; set; }
        public Guid Id { get; set; }

        public string ShortId { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string Email { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalPoints { get; set; }
        public int TotalItems { get; set; }
        public byte OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BillingPersonFirstName { get; set; }
        public string BillingPersonLastName { get; set; }
        public string BillingEmail { get; set; }
        public string BillingMobileNumber { get; set; }
        public string BillingMobileCountryCode { get; set; }
        public string BillingAddressLine1 { get; set; }
        public string BillingAddressLine2 { get; set; }
        public string BillingPostcode { get; set; }
        public string BillingCity { get; set; }
        public string BillingState { get; set; }
        public string BillingCountry { get; set; }
        public string ShippingPersonFirstName { get; set; }
        public string ShippingPersonLastName { get; set; }
        public string ShippingEmail { get; set; }
        public string ShippingMobileNumber { get; set; }
        public string ShippingMobileCountryCode { get; set; }
        public string ShippingAddressLine1 { get; set; }
        public string ShippingAddressLine2 { get; set; }
        public string ShippingPostcode { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingCountry { get; set; }
        public decimal ShippingCost { get; set; }
      
        public virtual ICollection<OrderItems> OrderItems { get; set; }
    }
    public class OrderItems
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductExternalId { get; set; }
        public int MerchantExternalTypeId { get; set; }
        public int MerchantId { get; set; }
        public string MerchantExternalId { get; set; }
        public string MerchantDisplayName { get; set; }
        public decimal Commision { get; set; }
        public decimal Price { get; set; }
        public int Points { get; set; }
        public string ProductDetail { get; set; }
        public int ExpirationTypeId { get; set; }
        public string ProductTitle { get; set; }
        public string ProductImageFolderUrl { get; set; }
        public short Status { get; set; }
        public bool IsRedeemed { get; set; }
        public bool IsReviewed { get; set; }
        public int? VariationId { get; set; }
        public string VariationText { get; set; }
        public bool IsVariationProduct { get; set; }
        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public short ExternalTypeId { get; set; }
        public Commands.ProductList Product { get; set; }

        //Customised for History Page
        public DateTime CreatedAt { get; set; }
        public string ShortId { get; set; }
        public short OrderStatus { get; set; }
    }
}
