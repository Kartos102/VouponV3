using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Voupon.Merchant.WebApp.ApiControllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public abstract class BaseApiController : ControllerBase
    {
        private IMediator _mediator;
        //private IIdentityService _identityService;
        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());
        //protected IIdentityService IdentityService => _identityService ?? (_identityService = HttpContext.RequestServices.GetService<IIdentityService>());

        public bool IsSecretValid()
        {
            if (string.IsNullOrEmpty(HttpContext.Request.Query["secret"]))
            {
                return false;
            }

            if (HttpContext.Request.Query["secret"].ToString().ToLower() != "voupon-pg99")
            {
                return false;
            }
            return true;
        }
    }
}
