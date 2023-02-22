
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Areas.Admin.Services.EmailBlast.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.EmailBlast.ViewModels;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class EmailBlastController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Route("EmailBlast")]
        public async Task<ApiResponseViewModel> EmailBlast(string emailTitle, string testEmail, int emailType, string emailContent)
        {
            ApiResponseViewModel apiResponse = new ApiResponseViewModel();
            SendEmailViewModel sendEmailViewModel = new SendEmailViewModel() { 
            TestEmail = testEmail,
            CreatedAt = DateTime.Now,
            CreatedBy = new Guid(User.Identity.GetUserId()),
            EmailContent = emailContent,
            EmailTitle = emailTitle,
            EmailType = emailType,
            };
            apiResponse = await Mediator.Send(new SendEmailCommand() { EmailViewModel = sendEmailViewModel });

            return apiResponse;
        }
    }
}
