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
    public class OrderDetailsByOrderIdQuery : IRequest<ApiResponseViewModel>
    {
        public string OrderId { get; set; }

    }
    public class OrderDetailsByOrderIdQueryHandler : IRequestHandler<OrderDetailsByOrderIdQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public OrderDetailsByOrderIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(OrderDetailsByOrderIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                OrderDetailsModel orderDetailsModel = new OrderDetailsModel();
                var orderDetails = await rewardsDBContext.Orders.AsNoTracking().Where(x => x.Id == new Guid(request.OrderId)).FirstOrDefaultAsync();
                orderDetails.Logs = "";



                if (orderDetails != null)
                {
                    orderDetailsModel.Order = orderDetails;
                    response.Successful = true;
                    response.Message = "Get Order Details Successfully";
                    response.Data = orderDetailsModel;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Fail to get Order Details";
                    response.Data = null;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }

        public class OrderDetailsModel
        {
           public Voupon.Database.Postgres.RewardsEntities.Orders Order { get; set; }
        }
    }
}
