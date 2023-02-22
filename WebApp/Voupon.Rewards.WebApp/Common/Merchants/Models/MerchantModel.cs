using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.Common.Merchants.Models
{
    public class MerchantModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public string CompanyName { get; set; }
        public string RegistrationNumber { get; set; }
        public string WebsiteSiteUrl { get; set; }
        public string LogoUrl { get; set; }
        public string Description { get; set; }
        public int? BusinessTypeId { get; set; }
        public string BusinessType { get; set; }
        public int? CountryId { get; set; }
        public string CountryName { get; set; }
        public int? ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public int? DistritId { get; set; }
        public string DistritName { get; set; }
        public int? PostCodeId { get; set; }
        public string PostCodeName { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public string CompanyContact { get; set; }
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
        public bool IsBrandShownInHomePage { get; set; }
        public int TotalOutlets { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
    }
}
