using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class Areas
    {
        public short Id { get; set; }
        public short DistrictId { get; set; }
        public string? AreaCode { get; set; }
        public string AreaName { get; set; } = null!;

        public virtual Districts District { get; set; } = null!;
    }
}
