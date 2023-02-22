using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class BankAccounts
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string Name { get; set; }
        public string AccountNumber { get; set; }
        public int? BankId { get; set; }
        public string DocumentUrl { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
        public string Remarks { get; set; }
        public string PendingChanges { get; set; }

        public virtual Banks Bank { get; set; }
        public virtual Merchants Merchant { get; set; }
        public virtual StatusTypes StatusType { get; set; }
    }
}
