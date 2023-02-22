using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;
using System.Web;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Voupon.Common;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Services.Identity.Commands;
using Voupon.Rewards.WebApp.Services.Token.Commands;
using Voupon.Rewards.WebApp.Services.Token.Queries;

namespace Voupon.Rewards.WebApp.Controllers
{
    [Route("Token")]
    public class TokenController : TokenBaseController
    {
        private readonly IOptions<AppSettings> appSettings;
        private readonly SignInManager<Voupon.Database.Postgres.VodusEntities.Users> signInManager;
        private readonly VodusV2Context _vodusV2Context;

        public TokenController(IOptions<AppSettings> appSettings, SignInManager<Voupon.Database.Postgres.VodusEntities.Users> signInManager, VodusV2Context vodusV2Context)
        {
            this.appSettings = appSettings;
            this.signInManager = signInManager;
            _vodusV2Context = vodusV2Context;
        }
        [Route("create-temporary-points")]
        public async Task<ActionResult> CreateTemporaryPoints()
        {
            try
            {
                UserToken token = null;

                var RequestEmail = Request.Query["email"];
                //banned user list Backlog - 1756
                var bannedUser = await this._vodusV2Context.BannedUsers.AnyAsync(m => m.Email == RequestEmail);
                if (bannedUser)
                {
                    ViewData["Error"] = "Login failed. This account is suspended";
                    return View();
                }


                var points = 0;
                if (int.TryParse(Request.Query["points"], out points))
                {
                    Response.Cookies.Append("Rewards.Temporary.Points", points.ToString(), new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(3650),
                        Domain = appSettings.Value.App.CookieDomain,
                        SameSite = SameSiteMode.None,
                        Secure = appSettings.Value.App.IsSecure
                    });
                    ViewData["AvailablePoints"] = points.ToString();
                }


                if (!string.IsNullOrEmpty(Request.Query["token"]))
                {
                    token = UserToken.FromTokenValue(HttpUtility.UrlDecode(Request.Query["token"]));
                    Response.Cookies.Append("Vodus.Token", Request.Query["token"].ToString(), new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(3650),
                        Domain = appSettings.Value.App.CookieDomain,
                        SameSite = SameSiteMode.None,
                        Secure = appSettings.Value.App.IsSecure
                    });
                }

                if (!string.IsNullOrEmpty(Request.Query["login"]))
                {
                    if (Request.Query["login"] == "1")
                    {
                        await signInManager.SignOutAsync();

                        var loginCommand = new EmailLoginCommand
                        {
                            Email = Request.Query["email"],
                            Password = Request.Query["password"],
                            LoginProvider = Request.Query["LoginProvider"],
                            ThirdPartyToken = Request.Query["ThirdPartyToken"]
                        };

                        var loginResult = await Mediator.Send(loginCommand);

                        if (!loginResult.Successful)
                        {
                            return View();
                        }

                        if (!string.IsNullOrEmpty(Request.Query["email"]))
                        {
                            Response.Cookies.Append("Rewards.Account.Email", Request.Query["email"], new CookieOptions
                            {
                                Expires = DateTime.Now.AddDays(3650),
                                Domain = appSettings.Value.App.CookieDomain,
                                SameSite = SameSiteMode.None,
                                Secure = appSettings.Value.App.IsSecure
                            });


                            //  Update default language if the new GUID have more points
                            if (token != null)
                            {
                                if (!string.IsNullOrEmpty(Request.Query["language"]))
                                {

                                    var llanguageResult = Mediator.Send(new UpdateLanguageCommand
                                    {
                                        MemberProfileId = token.MemberProfileId,
                                        Language = Request.Query["language"]
                                    });

                                    Response.Cookies.Append("Vodus.Language", Request.Query["language"], new CookieOptions
                                    {
                                        Expires = DateTime.Now.AddDays(3650),
                                        Domain = appSettings.Value.App.CookieDomain,
                                        SameSite = SameSiteMode.None,
                                        Secure = appSettings.Value.App.IsSecure
                                    });
                                }
                                else
                                {
                                    //  Create language from master account if its null
                                    if (token.MemberMasterId != 0)
                                    {
                                        var master = await Mediator.Send(new MasterProfileQuery
                                        {
                                            Id = token.MemberMasterId
                                        });

                                        if (master.Successful)
                                        {

                                            if (!string.IsNullOrEmpty(master.Message))
                                            {
                                                Response.Cookies.Append("Vodus.Language", master.Message, new CookieOptions
                                                {
                                                    Expires = DateTime.Now.AddDays(3650),
                                                    Domain = appSettings.Value.App.CookieDomain,
                                                    SameSite = SameSiteMode.None,
                                                    Secure = appSettings.Value.App.IsSecure
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["Error"] = ex.ToString();
            }
            return View();
        }

        [Route("delete-temporary-points")]
        public async Task<ActionResult> DeleteTemporaryPoints()
        {
            //  Delete vodus token
            Response.Cookies.Append("Vodus.Token", "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-3650),
                Domain = appSettings.Value.App.CookieDomain,
                SameSite = SameSiteMode.None,
                Secure = appSettings.Value.App.IsSecure
            });

            //  Delete vodus token
            Response.Cookies.Append("Vodus.Token", "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-3650),
                Domain = appSettings.Value.App.ApiCookieDomain,
                SameSite = SameSiteMode.None,
                Secure = appSettings.Value.App.IsSecure
            });

            //  Delete temporary points    
            Response.Cookies.Append("Rewards.Temporary.Points", "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-3650),
                Domain = appSettings.Value.App.CookieDomain,
                SameSite = SameSiteMode.None,
                Secure = appSettings.Value.App.IsSecure
            });

            //  Delete temporary points    
            Response.Cookies.Append("Rewards.Account.Email", "", new CookieOptions
            {
                Expires = DateTime.Now.AddDays(-3650),
                Domain = appSettings.Value.App.CookieDomain,
                SameSite = SameSiteMode.None,
                Secure = appSettings.Value.App.IsSecure
            });

            await signInManager.SignOutAsync();
            return View();
        }

        [Route("check")]
        public async Task<ActionResult> Check()
        {
            return View();
        }
    }
}