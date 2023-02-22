using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Services.SignUp.Page
{
    public class IndexPage : IRequest<IndexPageViewModel>
    {
    }

    public class IndexPageViewModel
    {
        [Required(ErrorMessage = "Country selection is required")]
        [Display( Name = "Country")]
        public string CountryId { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Display(Name = "Mobile Number")]
        [Phone]
        public string MobileNumber { get; set; }
        [Required]
        [Display(Name = "Business Name")]
        public string BusinessName { get; set; }
        [Required]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [JsonPropertyName("g-recaptcha-response")]
        public string GoogleRecaptchaResponse { get; set; }
    }
}
