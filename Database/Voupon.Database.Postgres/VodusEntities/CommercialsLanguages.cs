using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class CommercialsLanguages
    {
        public int Id { get; set; }
        public int CommercialId { get; set; }
        public int LanguageId { get; set; }

        public virtual Commercials Commercial { get; set; } = null!;
        public virtual Languages Language { get; set; } = null!;
    }
}
