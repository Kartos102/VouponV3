using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Variables
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ContactTelephoneNumber { get; set; }
        public string ContactEmail { get; set; }
        public string FaxNumber { get; set; }
        public decimal? CommercialDepositRequiredPercentage { get; set; }
        public short? CommercialPendingToLiveHours { get; set; }
        public decimal? DemographicAverageRate { get; set; }
        public bool? CommercialCallingIsActive { get; set; }
        public byte CommercialPendingToLiveDays { get; set; }
        public decimal ScreeningQuotaDiscountPercentage { get; set; }
        public short CommercialCallingMaximumIntervalMinutes { get; set; }
        public byte TRPM { get; set; }
        public bool? GeoBlockEnabled { get; set; }
        public int QuotaThreeQuarterRate { get; set; }
        public bool? CSVFilterEnabled { get; set; }
        public bool NoDemographic { get; set; }
        public short NoDemographicInterval { get; set; }
        public decimal StandardScreeningRate { get; set; }
        public bool IsStressTestEnabled { get; set; }
        public bool IsCosmosDBSyncEnabled { get; set; }
        public byte CCSortingType { get; set; }
        public bool IsFingerprintingEnabled { get; set; }
        public decimal FingerprintDistant { get; set; }
        public bool IsUrlSyncEnabled { get; set; }
    }
}
