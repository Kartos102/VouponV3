using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class TreatedDataJsonFiles
    {
        public int Id { get; set; }
        public int CommercialId { get; set; }
        public string JsonFileName { get; set; }
        public string JsonFileUrl { get; set; }
        public DateTime UploadDate { get; set; }

        public virtual Commercials Commercial { get; set; }
    }
}
