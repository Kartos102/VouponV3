using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class ClientSurveyTemplates
    {
        public ClientSurveyTemplates()
        {
            ClientSurveyQuestionTemplates = new HashSet<ClientSurveyQuestionTemplates>();
            Commercials = new HashSet<Commercials>();
        }

        public int Id { get; set; }
        public int? ClientProfileId { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedIP { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string LastUpdatedIP { get; set; }

        public virtual ClientProfiles ClientProfile { get; set; }
        public virtual ICollection<ClientSurveyQuestionTemplates> ClientSurveyQuestionTemplates { get; set; }
        public virtual ICollection<Commercials> Commercials { get; set; }
    }
}
