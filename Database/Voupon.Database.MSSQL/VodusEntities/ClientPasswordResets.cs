using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class ClientPasswordResets
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string ResetCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public bool IsReset { get; set; }
    }
}
