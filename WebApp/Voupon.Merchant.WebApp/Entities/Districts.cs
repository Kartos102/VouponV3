using System;
using System.Collections.Generic;

namespace Voupon.Merchant.WebApp.Entities
{
    public partial class Districts
    {
        public Districts()
        {
            Merchants = new HashSet<Merchants>();
            Outlets = new HashSet<Outlets>();
            PostCodes = new HashSet<PostCodes>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int ProvinceId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }

        public virtual Provinces Province { get; set; }
        public virtual ICollection<Merchants> Merchants { get; set; }
        public virtual ICollection<Outlets> Outlets { get; set; }
        public virtual ICollection<PostCodes> PostCodes { get; set; }
    }
}
