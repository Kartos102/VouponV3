using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class __MigrationHistory
    {
        public string MigrationId { get; set; } = null!;
        public string ContextKey { get; set; } = null!;
        public byte[] Model { get; set; } = null!;
        public string ProductVersion { get; set; } = null!;
    }
}
