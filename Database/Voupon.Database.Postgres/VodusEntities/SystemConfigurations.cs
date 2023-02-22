using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class SystemConfigurations
    {
        public string Code { get; set; } = null!;
        public string? Description { get; set; }
        public string? Value { get; set; }
    }
}
