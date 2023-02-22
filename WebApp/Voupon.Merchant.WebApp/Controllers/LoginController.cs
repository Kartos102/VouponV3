using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;
using Voupon.Merchant.WebApp.Services.Identity.Commands;
using Voupon.Merchant.WebApp.Services.Login.Page;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Merchant.WebApp.Services.Identity.Queries;
namespace Voupon.Merchant.WebApp.Controllers
{
    [Route("[controller]")]
    public class LoginController : BaseController
    {
        public IActionResult Index()
        {
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
            }
            return View(new IndexPageViewModel());
        }

        [HttpPost]
        [Route("validate-login")]
        public async Task<IActionResult> ValidateLogin(AuthenticateUserCommand command)
        {
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                var roleList=(List<string>)result.Data;                    
                if (roleList.Contains("Merchant") || roleList.Contains("Staff"))
                {
                    result.Data = "/app/Dashboard";
                    var userResponse= await Mediator.Send(new UserMerchantIdQuery() {  Email = command.Email });
                    if (userResponse.Successful)
                    {
                        var merchantId = (int)userResponse.Data;
                        ApiResponseViewModel merchantResponse = await Mediator.Send(new MerchantQuery() { MerchantId = merchantId });
                        ApiResponseViewModel merchantPendingChangesResponse = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = merchantId });
                        if (merchantResponse.Successful && merchantPendingChangesResponse.Successful)
                        {
                            var merchant1 = (Common.Services.Merchants.Models.MerchantModel)merchantResponse.Data;
                            var merchant2 = (Common.Services.Merchants.Models.MerchantModel)merchantPendingChangesResponse.Data;
                            if (!merchant1.IsPublished && merchant1.StatusTypeId == 1 && merchant2.StatusTypeId == 1)
                            {
                                result.Data = "/App/Business";
                            }                           
                        }
                    }                   
                }
                else
                    result.Data = "/admin/Dashboard";
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

    }
}