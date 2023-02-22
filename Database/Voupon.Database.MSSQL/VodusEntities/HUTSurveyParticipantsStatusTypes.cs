using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class HUTSurveyParticipantsStatusTypes
    {
        public HUTSurveyParticipantsStatusTypes()
        {
            HUTSurveyParticipants = new HashSet<HUTSurveyParticipants>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<HUTSurveyParticipants> HUTSurveyParticipants { get; set; }
    }
}
