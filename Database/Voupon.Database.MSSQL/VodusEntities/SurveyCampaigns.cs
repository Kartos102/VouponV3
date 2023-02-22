using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SurveyCampaigns
    {
        public SurveyCampaigns()
        {
            Commercials = new HashSet<Commercials>();
        }

        public int Id { get; set; }
        public int ClientProfileId { get; set; }
        public string Name { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; }

        public virtual ClientProfiles ClientProfile { get; set; }
        public virtual ICollection<Commercials> Commercials { get; set; }
    }
}
