using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Invoices
    {
        public Invoices()
        {
            InvoiceItems = new HashSet<InvoiceItems>();
        }

        public int Id { get; set; }
        public int CommercialId { get; set; }
        public string CurrencyCode { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? Description { get; set; }
        public short InvoiceStatus { get; set; }
        public short InvoiceType { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;
        public string? FileUrl { get; set; }
        public string ToCompanyName { get; set; } = null!;
        public string ToAddressLine1 { get; set; } = null!;
        public string? ToAddressLine2 { get; set; }
        public string ToPostalCode { get; set; } = null!;
        public string ToState { get; set; } = null!;
        public string ToCountry { get; set; } = null!;
        public string? Notes { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public decimal? FinalAmount { get; set; }
        public int? PromoCodeId { get; set; }
        public string? PromoCodeDescription { get; set; }
        public decimal? DepositRequiredPercentage { get; set; }
        public decimal? FinalPaymentDiscountPercentage { get; set; }
        public int ScreeningQuota { get; set; }
        public short NumberOfTiers { get; set; }
        public short NumberOfQuestions { get; set; }

        public virtual Commercials Commercial { get; set; } = null!;
        public virtual PromoCodes? PromoCode { get; set; }
        public virtual ICollection<InvoiceItems> InvoiceItems { get; set; }
    }
}
