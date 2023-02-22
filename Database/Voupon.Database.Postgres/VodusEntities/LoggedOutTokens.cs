using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class LoggedOutTokens
    {
        public string Token { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public int CreatedAtTimestamp { get; set; }
    }
}
