using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Invoices
    {
        public Invoices()
        {
            InvoiceItems = new HashSet<InvoiceItems>();
        }

        public int Id { get; set; }
        public int CommercialId { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public byte InvoiceStatus { get; set; }
        public byte InvoiceType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string FileUrl { get; set; }
        public string ToCompanyName { get; set; }
        public string ToAddressLine1 { get; set; }
        public string ToAddressLine2 { get; set; }
        public string ToPostalCode { get; set; }
        public string ToState { get; set; }
        public string ToCountry { get; set; }
        public string Notes { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public decimal? FinalAmount { get; set; }
        public int? PromoCodeId { get; set; }
        public string PromoCodeDescription { get; set; }
        public decimal? DepositRequiredPercentage { get; set; }
        public decimal? FinalPaymentDiscountPercentage { get; set; }
        public int ScreeningQuota { get; set; }
        public short NumberOfTiers { get; set; }
        public short NumberOfQuestions { get; set; }

        public virtual Commercials Commercial { get; set; }
        public virtual PromoCodes PromoCode { get; set; }
        public virtual ICollection<InvoiceItems> InvoiceItems { get; set; }
    }
}
