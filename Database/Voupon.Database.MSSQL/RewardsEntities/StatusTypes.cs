using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class StatusTypes
    {
        public StatusTypes()
        {
            BankAccounts = new HashSet<BankAccounts>();
            LuckyDraws = new HashSet<LuckyDraws>();
            Merchants = new HashSet<Merchants>();
            PersonInCharges = new HashSet<PersonInCharges>();
            Products = new HashSet<Products>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual ICollection<BankAccounts> BankAccounts { get; set; }
        public virtual ICollection<LuckyDraws> LuckyDraws { get; set; }
        public virtual ICollection<Merchants> Merchants { get; set; }
        public virtual ICollection<PersonInCharges> PersonInCharges { get; set; }
        public virtual ICollection<Products> Products { get; set; }
    }
}
