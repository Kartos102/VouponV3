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

namespace Voupon.Merchant.WebApp.Common.Services.Orders.Queries
{
    public class OrderDetailsByOrderItmeShortIdQuery : Voupon.Database.Postgres.RewardsEntities.Orders
    {
        //public string ShortOrderItemId { get; set; }
        //public string ShortOrderId { get; set; }
    }
    public class OrderDetailsWithOrderShortIdQuery : IRequest<ApiResponseViewModel>
    {
        //public int MerchantId { get; set; }
        //public string ShortOrderId { get; set; }
        public string ShortOrderItemId { get; set; }

    }
    public class OrderDetailsWithOrderShortIdQueryHandler : IRequestHandler<OrderDetailsWithOrderShortIdQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public OrderDetailsWithOrderShortIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(OrderDetailsWithOrderShortIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var order = await rewardsDBContext.OrderItems.Include(x => x.Order).Where(x => x.ShortId == request.ShortOrderItemId).FirstOrDefaultAsync();
                if (order != null)
                {
                    OrderItemDetailsViewModel orderItemDetailsViewModel = new OrderItemDetailsViewModel()
                    {
                        OrderItem = order,
                        Quantity = await rewardsDBContext.OrderItems.Where(x => x.OrderId == order.OrderId && x.ProductId == order.ProductId).CountAsync()
                    };
                    if(order.Status == 4 || order.Status == 5 || order.Status == 8 || order.Status == 9)
                    {
                        var refund = await rewardsDBContext.Refunds.Where(x => x.OrderItemId == order.Id).FirstOrDefaultAsync();
                        if(refund != null)
                        {
                            orderItemDetailsViewModel.MoneyRefunded = refund.MoneyRefunded;
                            orderItemDetailsViewModel.PointsRefunded = refund.PointsRefunded;
                            orderItemDetailsViewModel.Remark = refund.Remark;
                        }
                    }
                    response.Successful = true;
                    response.Message = "Get Order Details Successfully";
                    response.Data = orderItemDetailsViewModel;
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
    public class OrderItemDetailsViewModel
    {
        public Voupon.Database.Postgres.RewardsEntities.OrderItems OrderItem { get; set; }
        public int Quantity { get; set; }
        public decimal MoneyRefunded { get; set; }
        public int PointsRefunded { get; set; }
        public string Remark { get; set; }
    }
}
