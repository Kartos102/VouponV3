using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class ClientPasswordResets
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string ResetCode { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool IsReset { get; set; }
    }
}
