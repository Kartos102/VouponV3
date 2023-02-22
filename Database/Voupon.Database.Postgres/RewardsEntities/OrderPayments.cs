using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.RewardsEntities
{
    public partial class OrderPayments
    {
        public Guid Id { get; set; }
        public short PaymentProviderId { get; set; }
        public Guid RefNo { get; set; }
        public string MerchantCode { get; set; } = null!;
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public string? Remark { get; set; }
        public string TransactionId { get; set; } = null!;
        public string? AuthCode { get; set; }
        public string Status { get; set; } = null!;
        public string? ErrorDescription { get; set; }
        public string? Signature { get; set; }
        public string? CreditCardName { get; set; }
        public string? CreditCardNumber { get; set; }
        public string? CreditCardBankName { get; set; }
        public string? CreditCardIssuingCountry { get; set; }
        public string JsonResponse { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public virtual Orders RefNoNavigation { get; set; } = null!;
    }
}
