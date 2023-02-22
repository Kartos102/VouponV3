using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Models;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Models;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Business.ViewModels
{
    public class BusinessDetailsViewModel
    {
        public MerchantModel Merchant { get; set; }
        public PersonInChargeModel PersonInCharge { get; set; }
        public BankAccountModel BankAccount { get; set; }
    }
}
