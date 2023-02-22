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
    public abstract class TokenBaseController : Controller
    {
        private IMediator _mediator;
        protected IMediator Mediator => _mediator ?? (_mediator = HttpContext.RequestServices.GetService<IMediator>());
    }
}