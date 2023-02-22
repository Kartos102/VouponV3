using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class BusinessTypes
    {
        public BusinessTypes()
        {
            Merchants = new HashSet<Merchants>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual ICollection<Merchants> Merchants { get; set; }
    }
}
