using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class Banks
    {
        public Banks()
        {
            BankAccounts = new HashSet<BankAccounts>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string BankCode { get; set; }
        public string SWIFTCode { get; set; }
        public int CountryId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual ICollection<BankAccounts> BankAccounts { get; set; }
    }
}
