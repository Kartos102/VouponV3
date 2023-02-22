using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.Services.Ads.Queries;
using Voupon.Rewards.WebApp.Services.Device.Queries;
using Voupon.Rewards.WebApp.Services.Order.Commands;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Services.Device.Queries.NonDeviceByTempTokenQuery;

namespace Voupon.Rewards.WebApp.ApiControllers
{
    [Route("api/v1/[controller]")]
    public class DeviceController : BaseApiController
    {
        [HttpGet]
        [Route("data/{id}")]
        public async Task<IActionResult> GetDeviceData(string id)
        {
            var result = await Mediator.Send(new DeviceDataQuery
            {
                DeviceId = new Guid(id)
            });

            if (result.Successful)
            {
                return new OkObjectResult(result);
            }
            return new BadRequestObjectResult(result);

        }
        [HttpGet]
        [Route("token/{id}")]
        public async Task<IActionResult> GetToken(string id)
        {
            var result = await Mediator.Send(new NonDeviceByTokenQuery
            {
                Token = id
            });

            if (result.Successful)
            {
                return new OkObjectResult(result);
            }
            return new BadRequestObjectResult(result);

        }

        [HttpGet]
        [Route("temptoken/{id}")]
        public async Task<IActionResult> GetTempToken(string id)
        {
            var result = await Mediator.Send(new NonDeviceByTempTokenQuery
            {
                TempToken = new Guid(id)
            });

            if (result.Successful)
            {
                var resultData = (DeviceViewModel)result.Data;
                if (!string.IsNullOrEmpty(resultData.Token))
                {
                    var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
                    {
                        Path = "/",
                        Expires = DateTime.Now.AddDays(3650),
                        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                        Secure = true
                    };
                    HttpContext.Response.Cookies.Append("Vodus.Token", resultData.Token, cookieOptions);
                    HttpContext.Response.Cookies.Delete("vodus_temp_token");
                    return Ok(result);
                }
                return new OkObjectResult(result);
            }
            return new BadRequestObjectResult(result);

        }

    }




}
