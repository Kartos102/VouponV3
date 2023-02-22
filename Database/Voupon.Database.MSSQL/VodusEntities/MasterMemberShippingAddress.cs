using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MasterMemberShippingAddress
    {
        public int Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Postcode { get; set; }
        public string City { get; set; }
        public short CountryId { get; set; }
        public string State { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsLastSelected { get; set; }
    }
}
