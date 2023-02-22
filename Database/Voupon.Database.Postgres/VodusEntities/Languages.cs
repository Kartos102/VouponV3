using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Languages
    {
        public Languages()
        {
            Commercials = new HashSet<Commercials>();
            CommercialsLanguages = new HashSet<CommercialsLanguages>();
            HUTSurveyProjects = new HashSet<HUTSurveyProjects>();
        }

        public int Id { get; set; }
        public string LanguageDisplayName { get; set; } = null!;
        public string LanguageCode { get; set; } = null!;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Commercials> Commercials { get; set; }
        public virtual ICollection<CommercialsLanguages> CommercialsLanguages { get; set; }
        public virtual ICollection<HUTSurveyProjects> HUTSurveyProjects { get; set; }
    }
}
