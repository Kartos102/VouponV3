using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MasterMemberProfiles
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AvailablePoints { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

       
        public string MobileVerified { get; set; }
        public string LastGenOtp { get; set; }


        public DateTime? LastGenDateTime { get; set; }

        public string Postcode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public short? CountryId { get; set; }
        public string MobileCountryCode { get; set; }
        public string MobileNumber { get; set; }
        public string PreferLanguage { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public long? MemberProfileId { get; set; }

        public virtual Countries Country { get; set; }
        public virtual Users User { get; set; }
    }
}
