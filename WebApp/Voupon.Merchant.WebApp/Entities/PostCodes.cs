using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class PostCodes
    {
        public PostCodes()
        {
            Merchants = new HashSet<Merchants>();
            Outlets = new HashSet<Outlets>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int DistrictId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual Districts District { get; set; }
        public virtual ICollection<Merchants> Merchants { get; set; }
        public virtual ICollection<Outlets> Outlets { get; set; }
    }
}
