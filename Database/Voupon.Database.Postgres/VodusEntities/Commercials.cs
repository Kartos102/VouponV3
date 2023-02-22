using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
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
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public short CommercialType { get; set; }
        public short Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int MaxResponse { get; set; }
        public bool IsRandomized { get; set; }
        public bool IsBoosted { get; set; }
        public short? TopicId { get; set; }
        public short? LocationFilterLevel { get; set; }
        public short SampleType { get; set; }
        public short? QuestionDelayMode { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? ClientTemplateId { get; set; }
        public decimal? DemographicQuotaRate { get; set; }
        public int ScreeningQuota { get; set; }
        public bool ScreeningQuotaIsCompleted { get; set; }
        public int ScreeningQuotaCompletedCount { get; set; }
        public string TargetCountryCode { get; set; } = null!;
        public int PriorityLevel { get; set; }
        public short EstimatedDuration { get; set; }
        public short DesignerLastStep { get; set; }
        public int DefaultLanguage { get; set; }
        public decimal ScreeningQuotaMAverageValue { get; set; }
        public decimal ScreeningQuotaM { get; set; }
        public short ScreeningQuestionTier { get; set; }
        public int ScreeningQuotaMCompletedCount { get; set; }
        public bool ScreeningQuotaMIsCompleted { get; set; }
        public bool IsReferral { get; set; }
        public string? ReferralUrl { get; set; }
        public short ReferralSurveyInMinutes { get; set; }
        public short VPointsPerQuestion { get; set; }
        public string? SecretCode { get; set; }
        public decimal IncidentRate { get; set; }
        public bool CustomScreeningRequired { get; set; }
        public short TotalATNQDemographic { get; set; }
        public bool IsShowPartnerDashboard { get; set; }
        public int SurveyCampaignId { get; set; }
        public bool IsLongitudinal { get; set; }
        public int? LongitudinalPrecedingSurveyId { get; set; }
        public short TotalChunks { get; set; }
        public bool HasOpeningChunk { get; set; }
        public decimal EffectiveRate { get; set; }
        public bool UseEffectiveRate { get; set; }
        public decimal InUseRate { get; set; }

        public virtual ClientProfiles ClientProfile { get; set; } = null!;
        public virtual ClientSurveyTemplates? ClientTemplate { get; set; }
        public virtual Languages DefaultLanguageNavigation { get; set; } = null!;
        public virtual SurveyCampaigns SurveyCampaign { get; set; } = null!;
        public virtual Topics? Topic { get; set; }
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
