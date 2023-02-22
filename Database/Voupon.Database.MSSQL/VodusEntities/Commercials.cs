using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Commercials
    {
        public Commercials()
        {
            CommercialDisplayTarget = new HashSet<CommercialDisplayTarget>();
            CommercialFilters = new HashSet<CommercialFilters>();
            CommercialLeads = new HashSet<CommercialLeads>();
            CommercialPsy = new HashSet<CommercialPsy>();
            CommercialSubgroups = new HashSet<CommercialSubgroups>();
            CommercialTargets = new HashSet<CommercialTargets>();
            CommercialWeightedScores = new HashSet<CommercialWeightedScores>();
            CommercialsLanguages = new HashSet<CommercialsLanguages>();
            Invoices = new HashSet<Invoices>();
            MemberProfileEndedCommercials = new HashSet<MemberProfileEndedCommercials>();
            PartnerWebsiteExcludedCommercials = new HashSet<PartnerWebsiteExcludedCommercials>();
            PartnerWebsitePriorityCommercials = new HashSet<PartnerWebsitePriorityCommercials>();
            SurveyChunkings = new HashSet<SurveyChunkings>();
            SurveyQuestions = new HashSet<SurveyQuestions>();
            SurveyResponses = new HashSet<SurveyResponses>();
            Temp_CommercialFilters = new HashSet<Temp_CommercialFilters>();
            Temp_CommercialWeightedScoreDemographics = new HashSet<Temp_CommercialWeightedScoreDemographics>();
            Temp_CommercialWeightedScores = new HashSet<Temp_CommercialWeightedScores>();
            TreatedDataJsonFiles = new HashSet<TreatedDataJsonFiles>();
        }

        public int Id { get; set; }
        public int ClientProfileId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public byte CommercialType { get; set; }
        public byte Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int MaxResponse { get; set; }
        public bool IsRandomized { get; set; }
        public bool IsBoosted { get; set; }
        public byte? TopicId { get; set; }
        public short? LocationFilterLevel { get; set; }
        public byte SampleType { get; set; }
        public byte? QuestionDelayMode { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ClientTemplateId { get; set; }
        public decimal? DemographicQuotaRate { get; set; }
        public int ScreeningQuota { get; set; }
        public bool ScreeningQuotaIsCompleted { get; set; }
        public int ScreeningQuotaCompletedCount { get; set; }
        public string TargetCountryCode { get; set; }
        public int PriorityLevel { get; set; }
        public short EstimatedDuration { get; set; }
        public byte DesignerLastStep { get; set; }
        public int DefaultLanguage { get; set; }
        public decimal ScreeningQuotaMAverageValue { get; set; }
        public decimal ScreeningQuotaM { get; set; }
        public byte ScreeningQuestionTier { get; set; }
        public int ScreeningQuotaMCompletedCount { get; set; }
        public bool ScreeningQuotaMIsCompleted { get; set; }
        public bool IsReferral { get; set; }
        public string ReferralUrl { get; set; }
        public byte ReferralSurveyInMinutes { get; set; }
        public byte VPointsPerQuestion { get; set; }
        public string SecretCode { get; set; }
        public decimal IncidentRate { get; set; }
        public bool CustomScreeningRequired { get; set; }
        public short TotalATNQDemographic { get; set; }
        public bool? IsShowPartnerDashboard { get; set; }
        public int SurveyCampaignId { get; set; }
        public bool IsLongitudinal { get; set; }
        public int? LongitudinalPrecedingSurveyId { get; set; }
        public byte TotalChunks { get; set; }
        public bool HasOpeningChunk { get; set; }

        public virtual ClientProfiles ClientProfile { get; set; }
        public virtual ClientSurveyTemplates ClientTemplate { get; set; }
        public virtual Languages DefaultLanguageNavigation { get; set; }
        public virtual SurveyCampaigns SurveyCampaign { get; set; }
        public virtual Topics Topic { get; set; }
        public virtual ICollection<CommercialDisplayTarget> CommercialDisplayTarget { get; set; }
        public virtual ICollection<CommercialFilters> CommercialFilters { get; set; }
        public virtual ICollection<CommercialLeads> CommercialLeads { get; set; }
        public virtual ICollection<CommercialPsy> CommercialPsy { get; set; }
        public virtual ICollection<CommercialSubgroups> CommercialSubgroups { get; set; }
        public virtual ICollection<CommercialTargets> CommercialTargets { get; set; }
        public virtual ICollection<CommercialWeightedScores> CommercialWeightedScores { get; set; }
        public virtual ICollection<CommercialsLanguages> CommercialsLanguages { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
        public virtual ICollection<MemberProfileEndedCommercials> MemberProfileEndedCommercials { get; set; }
        public virtual ICollection<PartnerWebsiteExcludedCommercials> PartnerWebsiteExcludedCommercials { get; set; }
        public virtual ICollection<PartnerWebsitePriorityCommercials> PartnerWebsitePriorityCommercials { get; set; }
        public virtual ICollection<SurveyChunkings> SurveyChunkings { get; set; }
        public virtual ICollection<SurveyQuestions> SurveyQuestions { get; set; }
        public virtual ICollection<SurveyResponses> SurveyResponses { get; set; }
        public virtual ICollection<Temp_CommercialFilters> Temp_CommercialFilters { get; set; }
        public virtual ICollection<Temp_CommercialWeightedScoreDemographics> Temp_CommercialWeightedScoreDemographics { get; set; }
        public virtual ICollection<Temp_CommercialWeightedScores> Temp_CommercialWeightedScores { get; set; }
        public virtual ICollection<TreatedDataJsonFiles> TreatedDataJsonFiles { get; set; }
    }
}
