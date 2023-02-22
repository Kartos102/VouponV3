using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SurveyCampaigns
    {
        public SurveyCampaigns()
        {
            Commercials = new HashSet<Commercials>();
        }

        public int Id { get; set; }
        public int ClientProfileId { get; set; }
        public string Name { get; set; } = null!;
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; }

        public virtual ClientProfiles ClientProfile { get; set; } = null!;
        public virtual ICollection<Commercials> Commercials { get; set; }
    }
}
