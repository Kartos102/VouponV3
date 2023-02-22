using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Areas.Admin.ViewModels.PromoCode
{
    public class PromoCodeViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Promo code is required")]
        public string PromoCode { get; set; }
        public string GeneratedId { get; set; }

        public string Description { get; set; }
        public byte Status { get; set; }
        public byte DiscountType { get; set; }
        [Required(ErrorMessage = "Promo value is required")]
        public decimal DiscountValue { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpireOn { get; set; }
        public decimal MinSpend { get; set; }
        public decimal MaxDiscountValue { get; set; }
        public int TotalRedemptionAllowed { get; set; }
        public int TotalRedeemed { get; set; }
        public bool IsFirstTimeUserOnly { get; set; }
        public bool IsNewSignupUserOnly { get; set; }
        public bool IsSelectedUserOnly { get; set; }
        public bool IsShipCostDeduct { get; set; }
        public int TotalAllowedPerUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}