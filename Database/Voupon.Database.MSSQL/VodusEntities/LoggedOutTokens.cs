using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class LoggedOutTokens
    {
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedAtTimestamp { get; set; }
    }
}
