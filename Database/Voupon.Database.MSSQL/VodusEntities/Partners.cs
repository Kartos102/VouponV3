using System;
using System.Collections.Generic;

namespace Voupon.Database.MSSQL.VodusEntities
{
    public partial class Partners
    {
        public Partners()
        {
            PartnerWebsites = new HashSet<PartnerWebsites>();
            ProductAdPartnersDomain = new HashSet<ProductAdPartnersDomain>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string LogoUrl { get; set; }
        public string DataSyncUrl { get; set; }
        public byte? DataSyncType { get; set; }
        public string DataSyncAccessKey { get; set; }
        public string DataSyncAccessSecret { get; set; }
        public string DataSyncContainerName { get; set; }

        public virtual ICollection<PartnerWebsites> PartnerWebsites { get; set; }
        public virtual ICollection<ProductAdPartnersDomain> ProductAdPartnersDomain { get; set; }
    }
}
