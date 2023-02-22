using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class ArticleCategories
    {
        public ArticleCategories()
        {
            Articles = new HashSet<Articles>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public virtual ICollection<Articles> Articles { get; set; }
    }
}
