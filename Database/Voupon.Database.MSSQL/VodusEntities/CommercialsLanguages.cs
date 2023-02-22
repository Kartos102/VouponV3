using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class CommercialsLanguages
    {
        public int Id { get; set; }
        public int CommercialId { get; set; }
        public int LanguageId { get; set; }

        public virtual Commercials Commercial { get; set; }
        public virtual Languages Language { get; set; }
    }
}
