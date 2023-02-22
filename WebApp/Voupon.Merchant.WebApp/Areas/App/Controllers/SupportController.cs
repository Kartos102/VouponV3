using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant,Staff")]
    public class SupportController : BaseAppController
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("ContactUs")]
        public async Task<IActionResult> ContactUs()
        {
            return View();
        }
        [HttpPost]
        [Route("SendSupportRequest")]
        public async Task<ApiResponseViewModel> SendSupportRequest(string message)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var merchantName = "-";
            var merchantCode = "-";
            var email = "-";
            var name = "-";
            var phone = "-";
            var userid = User.Identity.GetUserId();
            email = User.Identity.GetUserName();

            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(MerchantId))
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed email to support";
                return apiResponseViewModel;
            }

            var merchantId = Int32.Parse(MerchantId);
            var merchantResponse = await Mediator.Send(new MerchantQuery() { MerchantId = merchantId });
            if (merchantResponse.Successful)
            {
                var merchant = (MerchantModel)merchantResponse.Data;
                merchantName = merchant.DisplayName;
                merchantCode = merchant.Code;
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed email to support";
                return apiResponseViewModel;
            }


            var apiKey = "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";
            string templateId = "a346e956-506e-4bd1-8f8f-62025013c5ad";
            string subject = "Voupon Merchant Support Request["+ merchantCode + "]";
            var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus");
            var to = new SendGrid.Helpers.Mail.EmailAddress("Support@vodus.my", "");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.From = from;
            msg.TemplateId = templateId;
            msg.Personalizations = new System.Collections.Generic.List<Personalization>();
            var personalization = new Personalization();
            personalization.Substitutions = new Dictionary<string, string>();
            personalization.Substitutions.Add("-merchantName-", merchantName);
            personalization.Substitutions.Add("-merchantCode-", merchantCode);
            personalization.Substitutions.Add("-email-", email);
            personalization.Substitutions.Add("-name-", name);
            personalization.Substitutions.Add("-phone-", phone);
            personalization.Substitutions.Add("-message-", message);
            personalization.Subject = subject;
            personalization.Tos = new List<EmailAddress>();
            personalization.Tos.Add(to);
            msg.Personalizations.Add(personalization);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully email to support";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed email to support";

            }

            return apiResponseViewModel;
        }
    }
}