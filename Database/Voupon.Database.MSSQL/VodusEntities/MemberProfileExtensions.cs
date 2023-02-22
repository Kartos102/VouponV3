using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class MemberProfileExtensions
    {
        public long MemberProfileId { get; set; }
        public short DemographicTypeId { get; set; }
        public int DemographicValueId { get; set; }
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
