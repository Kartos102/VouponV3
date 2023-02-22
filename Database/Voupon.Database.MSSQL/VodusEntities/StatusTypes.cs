using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class StatusTypes
    {
        public short Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
