using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class MasterMemberShippingAddress
    {
        public int Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string AddressLine1 { get; set; } = null!;
        public string? AddressLine2 { get; set; }
        public string Postcode { get; set; } = null!;
        public string City { get; set; } = null!;
        public short CountryId { get; set; }
        public string State { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public bool IsLastSelected { get; set; }

        public virtual MasterMemberProfiles MasterMemberProfile { get; set; } = null!;
    }
}
