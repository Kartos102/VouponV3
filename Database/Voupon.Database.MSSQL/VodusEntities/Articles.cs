using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Articles
    {
        public Articles()
        {
            RequestForDownloads = new HashSet<RequestForDownloads>();
        }

        public int Id { get; set; }
        public string PageSlug { get; set; }
        public Guid GUID { get; set; }
        public string Title { get; set; }
        public string BannerImageUrl { get; set; }
        public string StoryUrl { get; set; }
        public byte? Status { get; set; }
        public int CategoryId { get; set; }
        public bool ShowOnLandingPage { get; set; }
        public string ReportDownloadLink { get; set; }
        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public int Order { get; set; }
        public bool DirectDownload { get; set; }
        public DateTime PublishedAt { get; set; }

        public virtual ArticleCategories Category { get; set; }
        public virtual ICollection<RequestForDownloads> RequestForDownloads { get; set; }
    }
}
