using MediatR;
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
    public class ExternalOrderItemQuery : IRequest<ApiResponseViewModel>
    {
        public byte WebsiteTypeId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public class ExternalOrderItemQueryHandler : IRequestHandler<ExternalOrderItemQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ExternalOrderItemQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ExternalOrderItemQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var OrderShopModelTtems = new List<OrderShopExternalViewModel>();

                    var externalOrders = await rewardsDBContext.OrderShopExternal.Include(x => x.Order).Include(x=> x.OrderItemExternal).AsNoTracking().Where(x => x.Order.CreatedAt.Date >= request.From && x.Order.CreatedAt.Date <= request.To).OrderByDescending(x => x.Order.CreatedAt).ToListAsync();

                    if(request.WebsiteTypeId != 0)
                        externalOrders = externalOrders.Where(x=>  x.ExternalTypeId == request.WebsiteTypeId).ToList();


                    foreach (var OrderShopExternal in externalOrders)
                    {
                        var shopModel = new OrderShopExternalViewModel()
                        {
                            CreatedAt = OrderShopExternal.Order.CreatedAt,
                            Email = OrderShopExternal.Order.Email,
                            OrderId = OrderShopExternal.Order.Id,

                            Id = OrderShopExternal.Id,
                            ExternalShopUrl = OrderShopExternal.ExternalShopUrl,
                            ShippingCost = OrderShopExternal.ShippingCost,
                            ShippingHtml = OrderShopExternal.ShippingDetailsJson,
                            Status = OrderShopExternal.OrderShippingExternalStatus,
                            TrackingNo = OrderShopExternal.TrackingNo,
                            LastUpdatedAt = OrderShopExternal.LastUpdatedAt,
                            LastUpdatedByUserId = OrderShopExternal.LastUpdatedByUser,
                            ExternalShopName = OrderShopExternal.ExternalShopName,
                            OrderItemViewModels = new List<ExternalOrderItemViewModel>()
                        };


                        var modelTtems = new List<ExternalOrderItemViewModel>();
                        foreach (var item in OrderShopExternal.OrderItemExternal)
                        {
                            var model = new ExternalOrderItemViewModel()
                            {
                                Id = item.Id,
                                ExternalUrl = item.ExternalUrl,
                                ProductTitle = item.ProductTitle,
                                Status = item.OrderItemExternalStatus,
                                Revenue = item.TotalPrice,
                                LastUpdatedAt = item.LastUpdatedAt,
                                LastUpdatedByUserId = item.LastUpdatedByUser
                            };
                            modelTtems.Add(model);
                        }

                        shopModel.OrderItemViewModels.AddRange(modelTtems);
                        OrderShopModelTtems.Add(shopModel);

                    }
                    apiResponseViewModel.Data = OrderShopModelTtems;
                    apiResponseViewModel.Successful = true;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;
                }
                return apiResponseViewModel;
            }
        }

        public class ExternalOrdersModel
        {
            public Guid OrderId { get; set; }
            public List<OrderShopExternalViewModel> orderShopExternalViewModel { get; set; }
        }
        public class OrderShopExternalViewModel
        {
            public Guid Id { get; set; }
            public Guid OrderId { get; set; }
            public string Email { get; set; }
            public string ExternalShopUrl { get; set; }
            public string ExternalShopName { get; set; }

            public DateTime CreatedAt { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public string LastUpdatedByUserId { get; set; }
            public string TrackingNo { get; set; }
            public string ShippingHtml { get; set; }
            public decimal ShippingCost { get; set; }
            public short Status { get; set; }
            public List<ExternalOrderItemViewModel> OrderItemViewModels { get; set; }

        }

        public class ExternalOrderItemViewModel
        {
            public Guid Id { get; set; }
            public string ExternalUrl { get; set; }
            public string ProductTitle { get; set; }
            public short Status { get; set; }
            public decimal Revenue { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public string LastUpdatedByUserId { get; set; }
            public string ShortOrderId { get; set; }
        }
    }
}
