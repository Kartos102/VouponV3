using System;
using System.Collections.Generic;

namespace Voupon.Database.Postgres.VodusEntities
{
    public partial class RequestForDownloads
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public string DownloadLink { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public short Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = null!;

        public virtual Articles Article { get; set; } = null!;
    }
}
