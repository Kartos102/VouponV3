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
    public class OrderItemByEmailQuery : IRequest<ApiResponseViewModel>
    {
        //public int MerchantId { get; set; }
        //public string ShortOrderId { get; set; }
        public string Id { get; set; }
        public int MerchantId { get; set; }

        public string Email { get; set; }

        public class OrderItemByEmailQueryHandler : IRequestHandler<OrderItemByEmailQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public OrderItemByEmailQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(OrderItemByEmailQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var order = await rewardsDBContext.OrderItems.Include(x => x.Order).Where(x => x.Order.Email == request.Email && x.Order.OrderStatus == 2  && x.MerchantId == request.MerchantId).OrderByDescending(x => x.Order.CreatedAt).Select(x => new OrderViewModel
                    {
                        Id = x.Id,
                        MasterMemberProfileId = x.Order.MasterMemberProfileId,
                        Email = x.Order.Email,
                        OrderStatus = x.Order.OrderStatus,
                        OrderItemStatus = x.Status,
                        Price = x.Price,
                        Points = x.Points,
                        CreatedAt = x.Order.CreatedAt,
                        ShortId = x.ShortId,
                        ProductTitle = x.ProductTitle,
                        ProductImageUrl = x.ProductImageFolderUrl.Replace("http://", "https://"),
                        CreatedAtString = x.Order.CreatedAt.ToString("dd/MM/yyyy")
                    }).ToListAsync();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = order;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;
                }

                return apiResponseViewModel;
            }
        }

        public class OrderViewModel
        {
            public Guid Id { get; set; }
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public decimal Price { get; set; }
            public int Points { get; set; }
            public short OrderStatus { get; set; }
            public short OrderItemStatus { get; set; }
            public DateTime CreatedAt { get; set; }

            public string ShortId { get; set; }

            public string ProductTitle { get; set; }
            public string ProductImageUrl { get; set; }

            public string CreatedAtString { get; set; }


        }


    }

}
