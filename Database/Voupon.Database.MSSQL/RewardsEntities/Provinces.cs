using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.RewardsEntities
{
    public partial class Provinces
    {
        public Provinces()
        {
            Districts = new HashSet<Districts>();
            Merchants = new HashSet<Merchants>();
            Outlets = new HashSet<Outlets>();
            ShippingCost = new HashSet<ShippingCost>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int CountryId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
        public int? DemographicId { get; set; }

        public virtual Countries Country { get; set; }
        public virtual ICollection<Districts> Districts { get; set; }
        public virtual ICollection<Merchants> Merchants { get; set; }
        public virtual ICollection<Outlets> Outlets { get; set; }
        public virtual ICollection<ShippingCost> ShippingCost { get; set; }
    }
}
