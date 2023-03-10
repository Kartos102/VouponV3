using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialResponseReports
    {
        public long Id { get; set; }
        public long? MemberProfileId { get; set; }
        public int CommercialId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public bool? IsFailedScreening { get; set; }
        public bool? IsCompleted { get; set; }
        public int? DemographicAgeId { get; set; }
        public string DemographicAgeDisplay { get; set; }
        public int? DemographicEducationId { get; set; }
        public string DemographicEducationDisplay { get; set; }
        public int? DemographicGenderId { get; set; }
        public string DemographicGenderDisplay { get; set; }
        public int? DemographicOccupationId { get; set; }
        public string DemographicOccupationDisplay { get; set; }
        public int? DemographicEthnicityId { get; set; }
        public string DemographicEthnicityDisplay { get; set; }
        public int? DemographicStateId { get; set; }
        public string DemographicStateDisplay { get; set; }
        public int? DemographicMonthlyIncomeId { get; set; }
        public string DemographicMonthlyIncomeDisplay { get; set; }
        public int? DemographicMaritalStatusId { get; set; }
        public string DemographicMaritalStatusDisplay { get; set; }
        public int? DemographicRuralUrbanId { get; set; }
        public string DemographicRuralUrbanDisplay { get; set; }
        public int? Question1Id { get; set; }
        public string Question1Title { get; set; }
        public string Question1AnswerId { get; set; }
        public string Question1AnswerDisplay { get; set; }
        public DateTime? Question1AnswerUpdatedAt { get; set; }
        public int? Question1RespondTimeInSeconds { get; set; }
        public int? Question2Id { get; set; }
        public string Question2Title { get; set; }
        public string Question2AnswerId { get; set; }
        public string Question2AnswerDisplay { get; set; }
        public DateTime? Question2AnswerUpdatedAt { get; set; }
        public int? Question2RespondTimeInSeconds { get; set; }
        public int? Question3Id { get; set; }
        public string Question3Title { get; set; }
        public string Question3AnswerId { get; set; }
        public string Question3AnswerDisplay { get; set; }
        public DateTime? Question3AnswerUpdatedAt { get; set; }
        public int? Question3RespondTimeInSeconds { get; set; }
        public int? Question4Id { get; set; }
        public string Question4Title { get; set; }
        public string Question4AnswerId { get; set; }
        public string Question4AnswerDisplay { get; set; }
        public DateTime? Question4AnswerUpdatedAt { get; set; }
        public int? Question4RespondTimeInSeconds { get; set; }
        public int? Question5Id { get; set; }
        public string Question5Title { get; set; }
        public string Question5AnswerId { get; set; }
        public string Question5AnswerDisplay { get; set; }
        public DateTime? Question5AnswerUpdatedAt { get; set; }
        public int? Question5RespondTimeInSeconds { get; set; }
        public int? Question6Id { get; set; }
        public string Question6Title { get; set; }
        public string Question6AnswerId { get; set; }
        public string Question6AnswerDisplay { get; set; }
        public DateTime? Question6AnswerUpdatedAt { get; set; }
        public int? Question6RespondTimeInSeconds { get; set; }
        public int? Question7Id { get; set; }
        public string Question7Title { get; set; }
        public string Question7AnswerId { get; set; }
        public string Question7AnswerDisplay { get; set; }
        public DateTime? Question7AnswerUpdatedAt { get; set; }
        public int? Question7RespondTimeInSeconds { get; set; }
        public int? Question8Id { get; set; }
        public string Question8Title { get; set; }
        public string Question8AnswerId { get; set; }
        public string Question8AnswerDisplay { get; set; }
        public DateTime? Question8AnswerUpdatedAt { get; set; }
        public int? Question8RespondTimeInSeconds { get; set; }
        public int? Question9Id { get; set; }
        public string Question9Title { get; set; }
        public string Question9AnswerId { get; set; }
        public string Question9AnswerDisplay { get; set; }
        public DateTime? Question9AnswerUpdatedAt { get; set; }
        public int? Question9RespondTimeInSeconds { get; set; }
        public int? Question10Id { get; set; }
        public string Question10Title { get; set; }
        public string Question10AnswerId { get; set; }
        public string Question10AnswerDisplay { get; set; }
        public DateTime? Question10AnswerUpdatedAt { get; set; }
        public int? Question10RespondTimeInSeconds { get; set; }
        public int? Question11Id { get; set; }
        public string Question11Title { get; set; }
        public string Question11AnswerId { get; set; }
        public string Question11AnswerDisplay { get; set; }
        public DateTime? Question11AnswerUpdatedAt { get; set; }
        public int? Question11RespondTimeInSeconds { get; set; }
        public int? Question12Id { get; set; }
        public string Question12Title { get; set; }
        public string Question12AnswerId { get; set; }
        public string Question12AnswerDisplay { get; set; }
        public DateTime? Question12AnswerUpdatedAt { get; set; }
        public int? Question12RespondTimeInSeconds { get; set; }
        public int? Question13Id { get; set; }
        public string Question13Title { get; set; }
        public string Question13AnswerId { get; set; }
        public string Question13AnswerDisplay { get; set; }
        public DateTime? Question13AnswerUpdatedAt { get; set; }
        public int? Question13RespondTimeInSeconds { get; set; }
        public int? Question14Id { get; set; }
        public string Question14Title { get; set; }
        public string Question14AnswerId { get; set; }
        public string Question14AnswerDisplay { get; set; }
        public DateTime? Question14AnswerUpdatedAt { get; set; }
        public int? Question14RespondTimeInSeconds { get; set; }
        public int? Question15Id { get; set; }
        public string Question15Title { get; set; }
        public string Question15AnswerId { get; set; }
        public string Question15AnswerDisplay { get; set; }
        public DateTime? Question15AnswerUpdatedAt { get; set; }
        public int? Question15RespondTimeInSeconds { get; set; }
        public int? Question16Id { get; set; }
        public string Question16Title { get; set; }
        public string Question16AnswerId { get; set; }
        public string Question16AnswerDisplay { get; set; }
        public DateTime? Question16AnswerUpdatedAt { get; set; }
        public int? Question16RespondTimeInSeconds { get; set; }
        public int? Question17Id { get; set; }
        public string Question17Title { get; set; }
        public string Question17AnswerId { get; set; }
        public string Question17AnswerDisplay { get; set; }
        public DateTime? Question17AnswerUpdatedAt { get; set; }
        public int? Question17RespondTimeInSeconds { get; set; }
        public int? Question18Id { get; set; }
        public string Question18Title { get; set; }
        public string Question18AnswerId { get; set; }
        public string Question18AnswerDisplay { get; set; }
        public DateTime? Question18AnswerUpdatedAt { get; set; }
        public int? Question18RespondTimeInSeconds { get; set; }
        public int? Question19Id { get; set; }
        public string Question19Title { get; set; }
        public string Question19AnswerId { get; set; }
        public string Question19AnswerDisplay { get; set; }
        public DateTime? Question19AnswerUpdatedAt { get; set; }
        public int? Question19RespondTimeInSeconds { get; set; }
        public int? Question20Id { get; set; }
        public string Question20Title { get; set; }
        public string Question20AnswerId { get; set; }
        public string Question20AnswerDisplay { get; set; }
        public DateTime? Question20AnswerUpdatedAt { get; set; }
        public int? Question20RespondTimeInSeconds { get; set; }
        public int? Question21Id { get; set; }
        public string Question21Title { get; set; }
        public string Question21AnswerId { get; set; }
        public string Question21AnswerDisplay { get; set; }
        public DateTime? Question21AnswerUpdatedAt { get; set; }
        public int? Question21RespondTimeInSeconds { get; set; }
        public int? Question22Id { get; set; }
        public string Question22Title { get; set; }
        public string Question22AnswerId { get; set; }
        public string Question22AnswerDisplay { get; set; }
        public DateTime? Question22AnswerUpdatedAt { get; set; }
        public int? Question22RespondTimeInSeconds { get; set; }
        public int? Question23Id { get; set; }
        public string Question23Title { get; set; }
        public string Question23AnswerId { get; set; }
        public string Question23AnswerDisplay { get; set; }
        public DateTime? Question23AnswerUpdatedAt { get; set; }
        public int? Question23RespondTimeInSeconds { get; set; }
        public int? Question24Id { get; set; }
        public string Question24Title { get; set; }
        public string Question24AnswerId { get; set; }
        public string Question24AnswerDisplay { get; set; }
        public DateTime? Question24AnswerUpdatedAt { get; set; }
        public int? Question24RespondTimeInSeconds { get; set; }
        public int? Question25Id { get; set; }
        public string Question25Title { get; set; }
        public string Question25AnswerId { get; set; }
        public string Question25AnswerDisplay { get; set; }
        public DateTime? Question25AnswerUpdatedAt { get; set; }
        public int? Question25RespondTimeInSeconds { get; set; }
        public int? Question26Id { get; set; }
        public string Question26Title { get; set; }
        public string Question26AnswerId { get; set; }
        public string Question26AnswerDisplay { get; set; }
        public DateTime? Question26AnswerUpdatedAt { get; set; }
        public int? Question26RespondTimeInSeconds { get; set; }
        public int? Question27Id { get; set; }
        public string Question27Title { get; set; }
        public string Question27AnswerId { get; set; }
        public string Question27AnswerDisplay { get; set; }
        public DateTime? Question27AnswerUpdatedAt { get; set; }
        public int? Question27RespondTimeInSeconds { get; set; }
        public int? Question28Id { get; set; }
        public string Question28Title { get; set; }
        public string Question28AnswerId { get; set; }
        public string Question28AnswerDisplay { get; set; }
        public DateTime? Question28AnswerUpdatedAt { get; set; }
        public int? Question28RespondTimeInSeconds { get; set; }
        public int? Question29Id { get; set; }
        public string Question29Title { get; set; }
        public string Question29AnswerId { get; set; }
        public string Question29AnswerDisplay { get; set; }
        public DateTime? Question29AnswerUpdatedAt { get; set; }
        public int? Question29RespondTimeInSeconds { get; set; }
        public int? Question30Id { get; set; }
        public string Question30Title { get; set; }
        public string Question30AnswerId { get; set; }
        public string Question30AnswerDisplay { get; set; }
        public DateTime? Question30AnswerUpdatedAt { get; set; }
        public int? Question30RespondTimeInSeconds { get; set; }
        public decimal ResponseCost { get; set; }
        public byte NumberOfTiers { get; set; }
        public bool IsPassFilter { get; set; }
        public int? CommercialSubgroupId { get; set; }
        public byte NumberOfQuestionCompleted { get; set; }
        public decimal? TotalResponseCost { get; set; }
        public int? ChunkId { get; set; }
        public int? DemographicMonthlyHouseHoldIncomeId { get; set; }
        public string DemographicMonthlyHouseHoldIncomeDisplay { get; set; }
        public string UserId { get; set; }
    }
}
