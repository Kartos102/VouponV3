using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.ViewModels.ThirdParty.IPay88
{
    public class IPay88RequestViewModel
    {
        public string MerchantKey { get; set; }
        public string MerchantCode { get; set; }
        public int PaymentId { get; set; }
        public string RefNo { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public string ProdDesc { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserContact { get; set; }
        public string Remark { get; set; }
        public string Lang { get; set; }
        public string SignatureType { get; set; }
        public string Signature { get; set; }
        public string PaymentUrl { get; set; }
        public string PaymentBackendUrl { get; set; }
        public string PaymentResponseUrl { get; set; }
    }

}
