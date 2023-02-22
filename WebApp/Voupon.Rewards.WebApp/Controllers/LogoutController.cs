using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voupon.Rewards.WebApp.Services.Identity.Commands;
using static Voupon.Rewards.WebApp.Services.Identity.Commands.LogoutCommand;

namespace Voupon.Rewards.WebApp.Controllers
{
    public class LogoutController : BaseController
    {
        [HttpGet]
        [Route("/Logout")]
        public async Task<IActionResult> Index(int partnerWebsiteId)
        {
            var deviceId = "";
            var token = "";
            HttpContext.Request.Cookies.TryGetValue("Vodus.Token", out token);
            HttpContext.Request.Cookies.TryGetValue("vodus_device_id", out deviceId);
            HttpContext.Response.Cookies.Delete("Rewards.Temporary.Points");
            HttpContext.Response.Cookies.Delete("Vodus.Token");
            HttpContext.Response.Cookies.Delete("Rewards.Account.Email");
            HttpContext.Response.Cookies.Delete("Vodus.Language");
            HttpContext.Response.Cookies.Delete("vodus_device_sync_at");
            //HttpContext.Response.Cookies.Delete("vodus_visitor_id");
            //HttpContext.Response.Cookies.Delete("vodus_old_visitor_id");
            var result = await Mediator.Send(new LogoutCommand
            {
                PartnerWebsiteId = partnerWebsiteId,
                Token = token
            });

            if(result.Data != null)
            {
                var logoutPartnerWebsiteViewModel = (LogoutPartnerWebsiteViewModel)result.Data;
                ViewData["PartnerWebsiteName"] = logoutPartnerWebsiteViewModel.Name;
                ViewData["PartnerWebsiteUrl"] = logoutPartnerWebsiteViewModel.Url;
            }
            return View();
        }
    }
}