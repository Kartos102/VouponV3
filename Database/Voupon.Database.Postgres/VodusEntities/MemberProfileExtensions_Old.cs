using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class MemberProfileExtensions_Old
    {
        public int MemberProfileId { get; set; }
        public short DemographicTypeId { get; set; }
        public int DemographicValueId { get; set; }
        public string Value { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public virtual DemographicTypes DemographicType { get; set; } = null!;
        public virtual DemographicValues DemographicValue { get; set; } = null!;
    }
}
