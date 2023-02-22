using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.Infrastructures.Extensions;
using Voupon.Rewards.WebApp.Services.Base.Queries.Single;
using Voupon.Common.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Voupon.Common;
using System;

namespace Voupon.Rewards.WebApp.Controllers
{
    public abstract class BaseController : Controller
    {
        public int MemberMasterId { get; set; }
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());


        public UserToken GetTokenData(HttpContext context)
        {
            var token = context.Request.Cookies["Vodus.Token"];
            if (!string.IsNullOrEmpty(token))
            {
                var userTokenObject = UserToken.FromTokenValue(token);
                if (userTokenObject != null)
                {
                    return userTokenObject;
                }
            }
            return null;
        }

        public int GetRewardsPoints(HttpContext context)
        {
            var points = context.Request.Cookies["Rewards.Temporary.Points"];
            if (!string.IsNullOrEmpty(points))
            {
                return int.Parse(points);
            }
            return 0;
        }

        public string GetUserName(HttpContext context)
        {
            if (context.User != null)
            {
                return context.User.Identity.Name;
            }
            return "";
        }

        public int GetMasterMemberId(HttpContext context)
        {
            if (context.User != null)
            {
                return context.User.Identity.GetMasterMemberId();
            }
            return 0;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                var masterMemberId = 0;
                long MemberProfileId = 0;
                if (filterContext != null)
                {
                    if (filterContext.HttpContext != null)
                    {
                        //  Only get actions & not ajax
                        if (!HttpRequestExtensions.IsAjaxRequest(filterContext.HttpContext.Request) && filterContext.HttpContext.Request.Method == "GET")
                        {
                            var username = GetUserName(filterContext.HttpContext);
                            
                          var userToken = GetTokenData(filterContext.HttpContext);
                            if (userToken != null)
                            {
                                masterMemberId = userToken.MemberMasterId;
                                MemberProfileId = userToken.MemberProfileId;
                            }
                            
                            if(masterMemberId == 0)
                            {
                                username = "";
                            }

                            var result = Mediator.Send(new BasePageQuery
                            {
                                MemberProfileId = MemberProfileId,
                                UserName = username
                            }).Result;

                            ViewData["AvailablePoints"] = result.AvailablePoints;
                            ViewData["AccountEmail"] = result.AccountEmail;
                            ViewData["AccountName"] = result.AccountName;
                            ViewData["PreferredLanguage"] = result.PreferredLanguage;
                            ViewData["AllLanguages"] = result.AllLanguages;
                            ViewData["BaseUrl"] = result.BaseUrl;
                            ViewData["APIUrl"] = result.ApiUrl;
                            ViewData["CDNUrl"] = result.CDNUrl;
                            ViewData["ServerlessUrl"] = result.ServerlessUrl;
                            ViewData["IsFingerprintingEnabled"] = result.IsFingerprintingEnabled;
                            ViewData["ChatAPIUrl"] = result.ChatAPIUrl;
                            ViewData["ChatToken"] = result.ChatToken;
                        }
                        base.OnActionExecuted(filterContext);
                    }
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
            }

        }
    }
}