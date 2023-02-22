using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class HUTSurveyParticipants
    {
        public HUTSurveyParticipants()
        {
            HUTSurveyResponses = new HashSet<HUTSurveyResponses>();
        }

        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Demographics { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public int StatusTypeId { get; set; }
        public int HUTSurveyProjectId { get; set; }
        public int CompletedForms { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int? UserId { get; set; }
        public bool? IsQualified { get; set; }
        public int? CurrentFormId { get; set; }
        public string? Postcode { get; set; }
        public string? City { get; set; }
        public short ListTypeId { get; set; }

        public virtual HUTSurveyProjects HUTSurveyProject { get; set; } = null!;
        public virtual HUTSurveyParticipantsStatusTypes StatusType { get; set; } = null!;
        public virtual ICollection<HUTSurveyResponses> HUTSurveyResponses { get; set; }
    }
}
