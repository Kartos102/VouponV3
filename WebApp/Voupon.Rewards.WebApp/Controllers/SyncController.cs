using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.Common.PartnerWebsites.Queries;
using Voupon.Rewards.WebApp.Services.Identity.Commands;
using Voupon.Rewards.WebApp.Services.Identity.Queries;

namespace Voupon.Rewards.WebApp.Controllers
{
    public class SyncController : BaseController
    {
        private const int COOKIE_DAYS = 36000;
        [HttpGet]
        [Route("/sync")]
        public async Task<IActionResult> Index(string tempToken, string token, int syncType, string redirectUrl, string host, int partnerWebsiteId, string vodusId, int? thirdPartyEnabled)
        {
            try
            {
                if (syncType == 0)
                {
                    ViewData["Message"] = "Invalid requests [0]";
                    return View("~/views/sync/invalid.cshtml");
                }

                if (string.IsNullOrEmpty(tempToken) && string.IsNullOrEmpty(token))
                {
                    ViewData["Message"] = "Invalid requests";
                    return View("~/views/sync/invalid.cshtml");
                }

                var partnerWebsiteResult = await Mediator.Send(new PartnerWebsiteQuery { PartnerWebsiteId = partnerWebsiteId });
                if (partnerWebsiteResult != null)
                {
                    ViewData["PartnerWebsiteName"] = partnerWebsiteResult.Data.ToString();
                }

                ViewData["RedirectUrl"] = redirectUrl;
                ViewData["RedirectHostname"] = host;

                var viewModel = new SyncViewModel();
                var cookieToken = "";
                var authenticatedUserEmail = "";
                if (User.Identity.IsAuthenticated)
                {
                    authenticatedUserEmail = User.Identity.Name;
                }
                HttpContext.Request.Cookies.TryGetValue("Vodus.Token", out cookieToken);

                if (string.IsNullOrEmpty(cookieToken))
                {
                    if (string.IsNullOrEmpty(token))
                    {
                        var newToken = await Mediator.Send(new GenerateTokenCommand
                        {
                            TempToken = new Guid(tempToken)
                        });

                        if (!newToken.Successful)
                        {
                            ViewData["Message"] = "Fail to generate token. Please try again later";
                            return View("~/views/sync/invalid.cshtml");
                        }
                        cookieToken = newToken.Data.ToString();
                        var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
                        {
                            Path = "/",
                            HttpOnly = false,
                            SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                            Secure = true,
                            Expires = DateTime.Now.AddDays(365),
                            IsEssential = true
                        };
                        HttpContext.Response.Cookies.Append("Vodus.Token", cookieToken, cookieOptions);
                    }
                    else
                    {
                        var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
                        {
                            Path = "/",
                            HttpOnly = false,
                            SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                            Secure = true,
                            Expires = DateTime.Now.AddDays(365),
                            IsEssential = true
                        };
                        HttpContext.Response.Cookies.Append("Vodus.Token", token, cookieOptions);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(tempToken))
                    {
                        var updateTokenResult = await Mediator.Send(new UpdateTempTokenCommand
                        {
                            TempToken = new Guid(tempToken),
                            Token = cookieToken
                        });
                    }

                    if (!string.IsNullOrEmpty(token))
                    {
                        var updateTokenResult = await Mediator.Send(new UpdateSyncTokenCommand
                        {
                            SyncToken = token,
                            Token = cookieToken,
                            UserName = authenticatedUserEmail
                        });
                    }
                }


                if (syncType == 1)  // I have seen this question before
                {
                    //  temptoken exists but not token
                    
                   

                    if (!string.IsNullOrEmpty(vodusId) && !string.IsNullOrEmpty(cookieToken))
                    {
                        var updateTokenResult = await Mediator.Send(new UpdateVodusIdTokenCommand
                        {
                            VodusId = new Guid(vodusId),
                            Token = cookieToken
                        });

                        if (!updateTokenResult.Successful)
                        {
                            ViewData["Message"] = "Fail to sync token data";
                            return View("~/views/sync/invalid.cshtml");
                        }
                    }

                   
                    if (User.Identity.IsAuthenticated)
                    {
                        ViewData["LoggedInEmail"] = User.Identity.Name;
                        return View("~/views/sync/loginsuccess.cshtml");
                    }


                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        if (redirectUrl.Contains("vodus.my") || redirectUrl.Contains("voupon-uat.azurewebsites.net"))
                        {
                            return Redirect(redirectUrl);
                        }
                    }
                    return View("~/views/sync/success.cshtml");
                }

                else if (syncType == 2)  //Login
                {
                    if (!string.IsNullOrEmpty(vodusId))
                    {
                        if (string.IsNullOrEmpty(cookieToken))
                        {
                            var generateTokenResult = await Mediator.Send(new GenerateTokenCommand());
                            if (!generateTokenResult.Successful)
                            {
                                ViewData["Message"] = "Fail to generate sync token";
                                return View("~/views/sync/invalid.cshtml");
                            }
                            cookieToken = generateTokenResult.Data.ToString();

                            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
                            {
                                Path = "/",
                                HttpOnly = false,
                                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                                Secure = true,
                                Expires = DateTime.Now.AddDays(365),
                                IsEssential = true
                            };
                            HttpContext.Response.Cookies.Append("Vodus.Token", cookieToken, cookieOptions);
                        }

                        var updateTokenResult = await Mediator.Send(new UpdateVodusIdTokenCommand
                        {
                            VodusId = new Guid(vodusId),
                            Token = cookieToken
                        });

                        if (!updateTokenResult.Successful)
                        {
                            ViewData["Message"] = "Fail to sync token data";
                            return View("~/views/sync/invalid.cshtml");
                        }
                    }

                    if (User.Identity.IsAuthenticated)
                    {
                        ViewData["LoggedInEmail"] = User.Identity.Name;
                        return View("~/views/sync/loginsuccess.cshtml");
                    }

                    return View("~/views/sync/login.cshtml");
                }

                else if (syncType == 3)  //View more from products
                {
                  
                    if (!string.IsNullOrEmpty(redirectUrl))
                    {
                        if (!string.IsNullOrEmpty(redirectUrl))
                        {
                            if (redirectUrl.Contains("vodus.my") || redirectUrl.Contains("voupon-uat.azurewebsites.net"))
                            {
                                return Redirect(redirectUrl);
                            }
                        }
                    }
                    return View("~/views/sync/success.cshtml");
                }

                else if (syncType == 4)  // footer/logo links
                {
                    return Redirect("/");
                }

            }
            catch (Exception ex)
            {
                //  Log
            }

            ViewData["Message"] = "Invalid request";
            return View("~/views/sync/invalid.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> Success()
        {
            return View();
        }

        [HttpGet]
        [Route("/sync/mobile-app")]
        public async Task<IActionResult> MobileApp()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> LoginSuccess(int partnerWebsiteId)
        {
            var partnerWebsiteResult = await Mediator.Send(new PartnerWebsiteQuery { PartnerWebsiteId = partnerWebsiteId });
            if (partnerWebsiteResult != null)
            {
                ViewData["PartnerWebsiteName"] = partnerWebsiteResult.Data.ToString();
            }

            if (User.Identity.IsAuthenticated)
            {
                ViewData["LoggedInEmail"] = User.Identity.Name;
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Invalid()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

    }

    public class SyncViewModel
    {

    }
}
