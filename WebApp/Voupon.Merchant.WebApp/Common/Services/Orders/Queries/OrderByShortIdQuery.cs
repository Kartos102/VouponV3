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
    public class OrderByShortIdQuery : IRequest<ApiResponseViewModel>
    {
        public string Id { get; set; }

        public class OrderByShortIdQueryHandler : IRequestHandler<OrderByShortIdQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public OrderByShortIdQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(OrderByShortIdQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var order = await rewardsDBContext.Orders.AsNoTracking().Include(x => x.OrderItems).Where(x => x.ShortId == request.Id).OrderByDescending(x => x.CreatedAt).Select(x => new OrderViewModel
                    {
                        Id = x.Id,
                        MasterMemberProfileId = x.MasterMemberProfileId,
                        Email = x.Email,
                        OrderStatus = x.OrderStatus,
                        TotalItems = x.TotalItems,
                        TotalPrice = x.TotalPrice,
                        TotalPoints = x.TotalPoints,
                        CreatedAt = x.CreatedAt,
                        ShortId = x.ShortId,
                        CreatedAtString = x.CreatedAt.ToString("dd/MM/yyyy")
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
            public decimal TotalPrice { get; set; }
            public int TotalPoints { get; set; }
            public int TotalItems { get; set; }
            public short OrderStatus { get; set; }
            public DateTime CreatedAt { get; set; }

            public string ShortId { get; set; }

            public string CreatedAtString { get; set; }


        }


    }

}
