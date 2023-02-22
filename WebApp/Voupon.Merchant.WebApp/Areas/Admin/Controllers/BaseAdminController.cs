using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Areas.Admin.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Voupon.Common.Extensions;
using System;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    public abstract class BaseAdminController : Controller
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());

        private string GetUserName(HttpContext context)
        {
            if (context.User != null)
            {
                return context.User.Identity.Name;
            }
            return "";
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            try
            {
                if (filterContext != null)
                {

                    if (filterContext.HttpContext != null)
                    {
                        //  Only get actions & not ajax
                        if (!HttpRequestExtensions.IsAjaxRequest(filterContext.HttpContext.Request) && filterContext.HttpContext.Request.Method == "GET")
                        {

                            var username = GetUserName(filterContext.HttpContext);

                            var result = Mediator.Send(new BasePageQuery
                            {
                                UserName = username
                            }).Result;


                            ViewData["ServerlessUrl"] = result.ServerlessUrl;
                            ViewData["ChatToken"] = result.ChatToken;
                            ViewData["UserName"] = username;
                        }

                        base.OnActionExecuted(filterContext);
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
    }
}