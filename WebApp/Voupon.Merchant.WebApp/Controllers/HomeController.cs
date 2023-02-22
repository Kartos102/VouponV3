using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using Voupon.Merchant.WebApp.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Controllers
{
    public class OutletRedemptionReport
    {
        public string MerchantName { get; set; }
        public string TotalRevenue { get; set; }
        public string Date { get; set; }
    }
    public class HomeController : BaseController
    {
       
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //return RedirectToAction("Index", "Login");
            if (User.Claims.Count() > 0)
            {
                var roles = User.Claims.FirstOrDefault(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
                if (roles != null)
                {
                    if (roles.Value == "Admin")
                    {
                        return Redirect("/Admin/Dashboard");
                    }
                    return Redirect("/App/Dashboard");
                }
                return Redirect("/login");
            }
            return View();
        }

        [Route("Contact")]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("About")]
        public IActionResult About()
        {
            return View();
        }
        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }
        [Route("TermsAndConditions")]
        public IActionResult TermsAndConditions()
        {
            return View();
        }
        

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

       // [Route("DemoViewAsPDF")]
        public IActionResult DemoViewAsPDF()
        {
            OutletRedemptionReport temp = new OutletRedemptionReport();
            temp.MerchantName = "Demo Comapny";
            temp.TotalRevenue = "RM 200.00";
            temp.Date = "01/08/2020 ~ 31/08/2020";
            return new Rotativa.AspNetCore.ViewAsPdf("DemoViewAsPDF", temp) { FileName = "OutletReport.pdf" };
            //try
            //{
            //    return new Rotativa.AspNetCore.ViewAsPdf("Demo/DemoViewAsPDF");
            //}
            //catch (Exception ex)
            //{
            //    string mess = ex.Message;
            //}
            return View();
        }

        [HttpPost]
        [Route("RetrieveRewardsAds")]
        public async Task<ApiResponseViewModel> RetrieveRewardsAds()
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new Common.Services.RewardsAds.CreateRewardsAdsRedisCommand() { });
            return response;
        }

        [HttpGet]
        [Route("GETADS")]
        public async Task<ApiResponseViewModel> GETADS()
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            response = await Mediator.Send(new Common.Services.RewardsAds.CreateRewardsAdsRedisCommand() { });
            return response;
        }

        [HttpPost]
        [Route("SubmitNewRequest")]
        public async Task<ApiResponseViewModel> SubmitNewRequest(string email,string name, string subject,string message, string recaptcha)
        {
            if (string.IsNullOrEmpty(recaptcha))
            {
                return new ApiResponseViewModel
                {
                    Successful = false,
                    Message = "Invalid request"
                };
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();  
            var apiKey = "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";
            string templateId = "e6efdac9-4712-4ca7-80be-cb213f913c55";
            string subjectTitle = "Vodus Merchant - New Request";
            var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus Merchant");
            var to = new SendGrid.Helpers.Mail.EmailAddress("Support@vodus.my");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.From = from;
            msg.TemplateId = templateId;
            msg.Personalizations = new System.Collections.Generic.List<Personalization>();
            var personalization = new Personalization();
            personalization.Substitutions = new Dictionary<string, string>();
            personalization.Substitutions.Add("-email-", email);
            personalization.Substitutions.Add("-name-", name);
            personalization.Substitutions.Add("-subject-", subject);
            personalization.Substitutions.Add("-message-", message);
            personalization.Subject = subjectTitle;
            personalization.Tos = new List<EmailAddress>();
            personalization.Tos.Add(to);
            msg.Personalizations.Add(personalization);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully sent email";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed to send email";
            }
            return apiResponseViewModel;
        }
    }
}
