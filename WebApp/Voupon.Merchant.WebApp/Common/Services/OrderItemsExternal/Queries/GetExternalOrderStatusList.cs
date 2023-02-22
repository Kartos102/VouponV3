using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries
{
    public class GetExternalOrderStatusListQuery : IRequest<ApiResponseViewModel>
    {

    }
    public class GetExternalOrderStatusListQueryHandler : IRequestHandler<GetExternalOrderStatusListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public GetExternalOrderStatusListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(GetExternalOrderStatusListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var externalOrderStatusList = await rewardsDBContext.OrderItemsExternalStatusTypes.Where(x => x.IsActivated == true).ToListAsync();


                if (externalOrderStatusList != null)
                {
                    response.Successful = true;
                    response.Message = "Get external Order Status List Successfully";
                    response.Data = externalOrderStatusList;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Fail to get external Order Status List";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
