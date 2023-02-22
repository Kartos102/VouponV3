using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class RequestForDownloads
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public string DownloadLink { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public virtual Articles Article { get; set; }
    }
}
