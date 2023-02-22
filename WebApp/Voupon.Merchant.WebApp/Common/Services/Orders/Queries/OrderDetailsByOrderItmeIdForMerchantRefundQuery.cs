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
using static Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries.OrderDetailsByOrderIdQueryHandler;

namespace Voupon.Merchant.WebApp.Common.Services.Orders.Queries
{
    public class OrderDetailsByOrderItmeIdForMerchantRefundQuery : IRequest<ApiResponseViewModel>
    {
        public string OrderId { get; set; }
    }
    public class OrderDetailsByOrderItmeIdForMerchantRefundQueryHandler : IRequestHandler<OrderDetailsByOrderItmeIdForMerchantRefundQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public OrderDetailsByOrderItmeIdForMerchantRefundQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(OrderDetailsByOrderItmeIdForMerchantRefundQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                OrderDetailsModel orderDetailsModel = new OrderDetailsModel();
                var order = await rewardsDBContext.Orders.Include(x => x.OrderItems).Where(x => x.Id == new Guid(request.OrderId)).FirstOrDefaultAsync();
                if (order != null)
                {
                    order.TotalItems = 0;
                    order.TotalPrice = 0;
                    order.TotalPoints = 0;
                    order.TotalShippingCost = 0;
                    foreach (var orderItem in order.OrderItems)
                    {
                        order.TotalItems ++;
                        order.TotalPrice += orderItem.Price;
                        order.TotalPoints += orderItem.Points;
                        order.TotalShippingCost += orderItem.ShippingCost;
                    }

                    order.OrderItems = null;
                    order.Logs = "";
                    response.Successful = true;
                    response.Message = "Get Order Details Successfully";
                    orderDetailsModel.Order = order;
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
    }
}
