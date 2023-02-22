using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MemberProfilesOld
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string Postcode { get; set; }
        public short? StateId { get; set; }
        public short? CountryId { get; set; }
        public short? DistrictId { get; set; }
        public short? AreaId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CreatedAtPartnerWebsiteId { get; set; }
        public string CreatedAtCountryCode { get; set; }
        public int? MasterMemberProfileId { get; set; }
        public bool? IsMasterProfile { get; set; }
        public int AvailablePoints { get; set; }
        public DateTime? LastRespondedAt { get; set; }
        public int? DemographicAgeId { get; set; }
        public int? DemographicEducationId { get; set; }
        public int? DemographicGenderId { get; set; }
        public int? DemographicOccupationId { get; set; }
        public int? DemographicEthnicityId { get; set; }
        public int? DemographicReligionId { get; set; }
        public int? DemographicMonthlyIncomeId { get; set; }
        public int? DemographicMaritalStatusId { get; set; }
        public int? DemographicStateId { get; set; }
        public byte DemographicPoints { get; set; }
        public DateTime? DemographicLastUpdatedAt { get; set; }
        public DateTime? LastCommercialRequestedAt { get; set; }
        public int? DemographicAgeIdPartnerWebsiteId { get; set; }
        public int? DemographicEducationPartnerWebsiteId { get; set; }
        public int? DemographicGenderPartnerWebsiteId { get; set; }
        public int? DemographicOccupationPartnerWebsiteId { get; set; }
        public int? DemographicEthnicityPartnerWebsiteId { get; set; }
        public int? DemographicReligionPartnerWebsiteId { get; set; }
        public int? DemographicMonthlyIncomePartnerWebsiteId { get; set; }
        public int? DemographicMaritalStatusPartnerWebsiteId { get; set; }
        public int? DemographicStatePartnerWebsiteId { get; set; }
        public int? DemographicRuralUrbanId { get; set; }
        public int? DemographicRuralUrbanPartnerWebsiteId { get; set; }
        public int ResponseCount { get; set; }
        public int CloseClickCount { get; set; }
        public DateTime? CloseClickLastUpdatedAt { get; set; }
        public int? DemographicMonthlyHouseHoldIncomeId { get; set; }
        public int? DemographicMonthlyHouseHoldIncomePartnerWebsiteId { get; set; }
        public double? MemberR { get; set; }
        public int SyncMemberProfileId { get; set; }
        public string SyncMemberProfileToken { get; set; }
        public long? NewIdToUse { get; set; }
    }
}
