using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class StatusTypes
    {
        public short Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
