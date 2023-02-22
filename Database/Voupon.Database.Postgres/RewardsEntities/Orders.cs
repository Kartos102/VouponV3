﻿using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class Orders
    {
        public Orders()
        {
            OrderItems = new HashSet<OrderItems>();
            OrderPayments = new HashSet<OrderPayments>();
            OrderShopExternal = new HashSet<OrderShopExternal>();
        }

        public Guid Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string Email { get; set; } = null!;
        public decimal TotalPriceBeforePromoCodeDiscount { get; set; }
        public decimal TotalPromoCodeDiscount { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalPoints { get; set; }
        public int TotalItems { get; set; }
        public short OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public string BillingPersonFirstName { get; set; } = null!;
        public string BillingPersonLastName { get; set; } = null!;
        public string BillingEmail { get; set; } = null!;
        public string BillingMobileNumber { get; set; } = null!;
        public string BillingMobileCountryCode { get; set; } = null!;
        public string BillingAddressLine1 { get; set; } = null!;
        public string? BillingAddressLine2 { get; set; }
        public string BillingPostcode { get; set; } = null!;
        public string BillingCity { get; set; } = null!;
        public string BillingState { get; set; } = null!;
        public string BillingCountry { get; set; } = null!;
        public string? ShippingPersonFirstName { get; set; }
        public string? ShippingPersonLastName { get; set; }
        public string? ShippingEmail { get; set; }
        public string? ShippingMobileNumber { get; set; }
        public string? ShippingMobileCountryCode { get; set; }
        public string? ShippingAddressLine1 { get; set; }
        public string? ShippingAddressLine2 { get; set; }
        public string? ShippingPostcode { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingCountry { get; set; }
        public string? ShortId { get; set; }
        public decimal TotalShippingCost { get; set; }
        public decimal ShippingCostSubTotal { get; set; }
        public decimal ShippingCostDiscount { get; set; }
        public Guid? PromoCodeId { get; set; }
        public string? PromoCodeValue { get; set; }
        public short? PromoCodeDiscountType { get; set; }
        public decimal? PromoCodeDiscountValue { get; set; }
        public DateTime? PromoCodeExpireOn { get; set; }
        public decimal? PromoCodeMinSpend { get; set; }
        public decimal? PromoCodeMaxDiscountValue { get; set; }
        public string? Logs { get; set; }

        public virtual ICollection<OrderItems> OrderItems { get; set; }
        public virtual ICollection<OrderPayments> OrderPayments { get; set; }
        public virtual ICollection<OrderShopExternal> OrderShopExternal { get; set; }
    }
}
