using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class Merchants
    {
        public Merchants()
        {
            BankAccounts = new HashSet<BankAccounts>();
            CartProducts = new HashSet<CartProducts>();
            DeliveryRedemptionTokens = new HashSet<DeliveryRedemptionTokens>();
            DigitalRedemptionTokens = new HashSet<DigitalRedemptionTokens>();
            InStoreRedemptionTokens = new HashSet<InStoreRedemptionTokens>();
            MerchantFinance = new HashSet<MerchantFinance>();
            Outlets = new HashSet<Outlets>();
            PersonInCharges = new HashSet<PersonInCharges>();
            ProductReview = new HashSet<ProductReview>();
            Products = new HashSet<Products>();
            UserRoles = new HashSet<UserRoles>();
            MerchantCarousel = new HashSet<MerchantCarousel>();
        }

        public int Id { get; set; }
        public string Code { get; set; }
        public string DisplayName { get; set; }
        public string CompanyName { get; set; }
        public string RegistrationNumber { get; set; }
        public string WebsiteSiteUrl { get; set; }
        public string Description { get; set; }
        public int? BusinessTypeId { get; set; }
        public int? CountryId { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistritId { get; set; }
        public int? PostCodeId { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public string CompanyContact { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
        public string Remarks { get; set; }
        public bool IsPublished { get; set; }
        public string PendingChanges { get; set; }
        public string LogoUrl { get; set; }
        public string BIDDocumentUrl { get; set; }
        public decimal? DefaultCommission { get; set; }
        public decimal Rating { get; set; }
        public bool IsBrandShownInHomePage { get; set; }
        public bool IsTestAccount { get; set; }
        public int TotalRatingCount { get; set; }
        public bool IsExternal { get; set; }
        public string ExternalUrl { get; set; }
        public byte? ExternalTypeId { get; set; }
        public string ExternalId { get; set; }

        public virtual BusinessTypes BusinessType { get; set; }
        public virtual Countries Country { get; set; }
        public virtual Districts Distrit { get; set; }
        public virtual PostCodes PostCode { get; set; }
        public virtual Provinces Province { get; set; }
        public virtual StatusTypes StatusType { get; set; }
        public virtual ICollection<BankAccounts> BankAccounts { get; set; }
        public virtual ICollection<CartProducts> CartProducts { get; set; }
        public virtual ICollection<DeliveryRedemptionTokens> DeliveryRedemptionTokens { get; set; }
        public virtual ICollection<DigitalRedemptionTokens> DigitalRedemptionTokens { get; set; }
        public virtual ICollection<InStoreRedemptionTokens> InStoreRedemptionTokens { get; set; }
        public virtual ICollection<MerchantFinance> MerchantFinance { get; set; }
        public virtual ICollection<Outlets> Outlets { get; set; }
        public virtual ICollection<PersonInCharges> PersonInCharges { get; set; }
        public virtual ICollection<ProductReview> ProductReview { get; set; }
        public virtual ICollection<Products> Products { get; set; }
        public virtual ICollection<UserRoles> UserRoles { get; set; }
        public virtual ICollection<MerchantCarousel> MerchantCarousel { get; set; }
    }
}
