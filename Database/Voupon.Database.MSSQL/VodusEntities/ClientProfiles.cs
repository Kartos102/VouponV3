using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class ClientProfiles
    {
        public ClientProfiles()
        {
            ClientSurveyTemplates = new HashSet<ClientSurveyTemplates>();
            Commercials = new HashSet<Commercials>();
            SurveyCampaigns = new HashSet<SurveyCampaigns>();
        }

        public int Id { get; set; }
        public string UserId { get; set; }
        public int? PartnerWebsiteId { get; set; }
        public bool IsPartnerSuperAdmin { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public short PartnerParentId { get; set; }
        public string Company { get; set; }
        public string CompanyNumber { get; set; }
        public string Title { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string LineOfWork { get; set; }
        public short CountryId { get; set; }

        public virtual Countries Country { get; set; }
        public virtual PartnerWebsites PartnerWebsite { get; set; }
        public virtual Users User { get; set; }
        public virtual ICollection<ClientSurveyTemplates> ClientSurveyTemplates { get; set; }
        public virtual ICollection<Commercials> Commercials { get; set; }
        public virtual ICollection<SurveyCampaigns> SurveyCampaigns { get; set; }
    }
}
