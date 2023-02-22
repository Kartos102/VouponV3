using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class SurveyResponses
    {
        public SurveyResponses()
        {
            SurveyResponseAnswers = new HashSet<SurveyResponseAnswers>();
            SurveyResponseDemographic = new HashSet<SurveyResponseDemographic>();
        }

        public int Id { get; set; }
        public int CommercialId { get; set; }
        public long MemberProfileId { get; set; }
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public string Host { get; set; }
        public string Origin { get; set; }
        public string Referer { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsExceedQuotaResponse { get; set; }
        public int RespondTimeInSeconds { get; set; }
        public bool IsFailedScreening { get; set; }
        public int TierNo { get; set; }
        public int? SubgroupId { get; set; }
        public string PartnerCode { get; set; }
        public int? SurveyQuestionId { get; set; }
        public byte PointsCollected { get; set; }
        public string MemberDemographic { get; set; }

        public virtual Commercials Commercial { get; set; }
        public virtual ICollection<SurveyResponseAnswers> SurveyResponseAnswers { get; set; }
        public virtual ICollection<SurveyResponseDemographic> SurveyResponseDemographic { get; set; }
    }
}
