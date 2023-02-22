using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SurveyResponseDemographic
    {
        public int Id { get; set; }
        public int SurveyResponseId { get; set; }
        public short? DemographicTypeId { get; set; }
        public string Value { get; set; }

        public virtual DemographicTypes DemographicType { get; set; }
        public virtual SurveyResponses SurveyResponse { get; set; }
    }
}
