using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Models;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Models;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.EmailBlast.ViewModels
{
    public class SendEmailViewModel
    {
        public string TestEmail { get; set; }
        public string EmailContent { get; set; }
        public string EmailTitle { get; set; }
        public int EmailType { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
