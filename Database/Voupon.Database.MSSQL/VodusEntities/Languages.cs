using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
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
        public string LanguageDisplayName { get; set; }
        public string LanguageCode { get; set; }
        public bool? IsDefault { get; set; }
        public bool? IsActive { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Commercials> Commercials { get; set; }
        public virtual ICollection<CommercialsLanguages> CommercialsLanguages { get; set; }
        public virtual ICollection<HUTSurveyProjects> HUTSurveyProjects { get; set; }
    }
}
