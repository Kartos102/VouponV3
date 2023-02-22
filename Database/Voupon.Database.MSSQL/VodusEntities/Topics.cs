using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Topics
    {
        public Topics()
        {
            Commercials = new HashSet<Commercials>();
        }

        public byte Id { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<Commercials> Commercials { get; set; }
    }
}
