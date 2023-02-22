using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class DemographicTypes
    {
        public DemographicTypes()
        {
            CommercialFilters = new HashSet<CommercialFilters>();
            CommercialTargetCosts = new HashSet<CommercialTargetCosts>();
            CommercialTargets = new HashSet<CommercialTargets>();
            DemographicValues = new HashSet<DemographicValues>();
            SurveyQuestions = new HashSet<SurveyQuestions>();
            SurveyResponseDemographic = new HashSet<SurveyResponseDemographic>();
            Temp_CommercialFilters = new HashSet<Temp_CommercialFilters>();
            Temp_CommercialWeightedScoreDemographics = new HashSet<Temp_CommercialWeightedScoreDemographics>();
        }

        public short Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ResourceString { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsRange { get; set; }
        public short? DisplaySequence { get; set; }
        public decimal TargetAverageRate { get; set; }

        public virtual ICollection<CommercialFilters> CommercialFilters { get; set; }
        public virtual ICollection<CommercialTargetCosts> CommercialTargetCosts { get; set; }
        public virtual ICollection<CommercialTargets> CommercialTargets { get; set; }
        public virtual ICollection<DemographicValues> DemographicValues { get; set; }
        public virtual ICollection<SurveyQuestions> SurveyQuestions { get; set; }
        public virtual ICollection<SurveyResponseDemographic> SurveyResponseDemographic { get; set; }
        public virtual ICollection<Temp_CommercialFilters> Temp_CommercialFilters { get; set; }
        public virtual ICollection<Temp_CommercialWeightedScoreDemographics> Temp_CommercialWeightedScoreDemographics { get; set; }
    }
}
