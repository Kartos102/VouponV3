using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Dashboard.Models
{
    public class DashboardModel
    {
        public int MerchantTotal { get; set; }
        public int MerchantPublished { get; set; }
        public int MerchantDraft { get; set; }
        public int MerchantPendingReview { get; set; }
        public int MerchantPendingRevision { get; set; }
        public int MerchantApproved { get; set; }
        public int ProductTotal { get; set; }
        public int ProductPublished { get; set; }
        public int ProductDraft { get; set; }
        public int ProductPendingReview { get; set; }
        public int ProductPendingRevision { get; set; }
        public int ProductApproved { get; set; }
    }
}
