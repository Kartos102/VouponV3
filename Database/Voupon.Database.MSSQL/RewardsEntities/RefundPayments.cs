using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class RefundPayments
    {
        public Guid Id { get; set; }
        public byte PaymentProviderId { get; set; }
        public Guid RefNo { get; set; }
        public string MerchantCode { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Remark { get; set; }
        public string TransactionId { get; set; }
        public string AuthCode { get; set; }
        public string Status { get; set; }
        public string ErrorDescription { get; set; }
        public string Signature { get; set; }
        public string CreditCardName { get; set; }
        public string CreditCardNumber { get; set; }
        public string CreditCardBankName { get; set; }
        public string CreditCardIssuingCountry { get; set; }
        public string JsonResponse { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual Refunds RefNoNavigation { get; set; }
    }
}
