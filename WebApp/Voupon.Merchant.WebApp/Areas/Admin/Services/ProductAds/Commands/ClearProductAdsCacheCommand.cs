using MediatR;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.ProductAds.Commands
{
    public class ClearProductAdsCacheCommand : IRequest<ApiResponseViewModel>
    {

    }

    public class ClearProductAdsCacheCommandHandler : IRequestHandler<ClearProductAdsCacheCommand, ApiResponseViewModel>
    {
        IConnectionMultiplexer connectionMultiplexer;

        public ClearProductAdsCacheCommandHandler(IConnectionMultiplexer connectionMultiplexer)
        {
            this.connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<ApiResponseViewModel> Handle(ClearProductAdsCacheCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            var redisCache = connectionMultiplexer.GetDatabase();
            if (redisCache.KeyDelete("VouponRewardsAds"))
            {
                response.Successful = true;
                response.Message = "Cleared Product Ads Cache Successfully";
            }
            else
            {
                response.Message = "Fail to Cleared Product Ads Cache";
            }         
            return response;
        }
    }
}
