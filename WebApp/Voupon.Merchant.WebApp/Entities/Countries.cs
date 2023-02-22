using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class Countries
    {
        public Countries()
        {
            Merchants = new HashSet<Merchants>();
            Outlets = new HashSet<Outlets>();
            Provinces = new HashSet<Provinces>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual ICollection<Merchants> Merchants { get; set; }
        public virtual ICollection<Outlets> Outlets { get; set; }
        public virtual ICollection<Provinces> Provinces { get; set; }
    }
}
