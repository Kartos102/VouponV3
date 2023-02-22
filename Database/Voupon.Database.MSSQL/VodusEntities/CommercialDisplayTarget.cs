using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialDisplayTarget
    {
        public int Id { get; set; }
        public int CommercialId { get; set; }
        public byte DisplayTarget { get; set; }
        public bool? IsEnabled { get; set; }

        public virtual Commercials Commercial { get; set; }
    }
}
