using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class HUTSurveyResponses
    {
        public int Id { get; set; }
        public int ParticipantId { get; set; }
        public int HUTSurveyFormId { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual HUTSurveyForms HUTSurveyForm { get; set; } = null!;
        public virtual HUTSurveyParticipants Participant { get; set; } = null!;
    }
}
