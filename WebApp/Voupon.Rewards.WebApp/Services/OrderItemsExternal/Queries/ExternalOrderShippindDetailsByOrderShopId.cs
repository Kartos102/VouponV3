﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Services.OrderItemsExternal.Queries
{
    public class ExternalOrderShippindDetailsByOrderShopId : IRequest<ApiResponseViewModel>
    {
        public Guid OrderShippingId { get; set; }

    }
    public class ExternalOrderShippindDetailsByOrderShopIdHandler : IRequestHandler<ExternalOrderShippindDetailsByOrderShopId, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ExternalOrderShippindDetailsByOrderShopIdHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ExternalOrderShippindDetailsByOrderShopId request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var externalShopOrder = await rewardsDBContext.OrderShopExternal.AsNoTracking().Where(x => x.Id == request.OrderShippingId).FirstOrDefaultAsync();

                if (externalShopOrder != null)
                {
                    response.Successful = true;
                    response.Message = "Get Order Shipping Details Successfully";
                    response.Data = externalShopOrder;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Fail to get Order Shipping Details";
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
