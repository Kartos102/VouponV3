using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Rewards.WebApp.ViewModels.ThirdParty.IPay88
{
    public class IPay88ResponseViewModel
    {
        public string MerchantCode { get; set; }
        public int PaymentId { get; set; }
        public string RefNo { get; set; }
        public string Amount { get; set; }
        public string eCurrency { get; set; }
        public string Remark { get; set; }
        public string TransId { get; set; }
        public string AuthCode { get; set; }
        public string eStatus { get; set; }
        public string ErrDesc { get; set; }
        public string Signature { get; set; }
        public string CCName { get; set; }
        public string CCNo { get; set; }
        public string S_bankname { get; set; }
        public string S_country { get; set; }
    }
}
