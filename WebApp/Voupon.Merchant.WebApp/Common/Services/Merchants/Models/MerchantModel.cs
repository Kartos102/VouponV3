using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Voupon.Merchant.WebApp.Common.Services.Merchants.Models
{
    public class MerchantModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        [Required]
        [Display(Name = "Company Display Name")]
        public string DisplayName { get; set; }
        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Required]
        [Display(Name = "Company Registration No")]
        public string RegistrationNumber { get; set; }
        [Required]
        [Display(Name = "WebsiteSite Url")]
        public string WebsiteSiteUrl { get; set; }
        //[Required(ErrorMessage = "The Logo is required")]
        [Display(Name = "Logo")]
        public string LogoUrl { get; set; }
        //[Required(ErrorMessage = "Please provide Description for your Company")]
        [Display(Name = "Describe your company to your customers​")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Please select your Company Business Type")]
        [Display(Name = "Business Type​")]
        public int? BusinessTypeId { get; set; }
        public string BusinessType { get; set; }
        [Required(ErrorMessage = "Please select the Country")]
        [Display(Name = "Country​")]
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        [Required(ErrorMessage = "Please select the Province")]
        [Display(Name = "Province​")]
        public int? ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        [Required(ErrorMessage = "Please select the District")]
        [Display(Name = "District​")]
        public int? DistritId { get; set; }
        public string DistritName { get; set; }
        [Required(ErrorMessage = "Please select the PostCode")]
        [Display(Name = "PostCode​")]
        public int? PostCodeId { get; set; }
        public string PostCodeName { get; set; }
        [Required(ErrorMessage = "Please provide the Address for your Company")]
        [Display(Name = "Address1")]
        public string Address_1 { get; set; }
        [Display(Name = "Address2")]
        public string Address_2 { get; set; }
        [Required(ErrorMessage = "Please provide the Company Contact Number for your Company")]
        [Display(Name = "Company Contact Number")]
        public string CompanyContact { get; set; }
        //[Required(ErrorMessage = "The Business Identity document is required")]
        [Display(Name = "Upload your Business Identity document")]
        [DataType(DataType.Upload)]
        //[MaxFileSize(5 * 1024 * 1024)]
        //[AllowedExtensions(new string[] { ".jpg", ".png" })]
        public string BIDDocumentUrl { get; set; }
        public int StatusTypeId { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }               
        public int PICStatusTypeId { get; set; }
        public string PICStatus { get; set; }
        public string PICRemarks { get; set; }
        public int BankStatusTypeId { get; set; }
        public string BankStatus { get; set; }
        public string BankRemarks { get; set; }
        public bool IsPublished { get; set; }

        public bool IsTestAccount { get; set; }
        public bool IsBrandShownInHomePage { get; set; }
        public int TotalOutlets { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public decimal? DefaultCommission { get; set; }
    }
}
