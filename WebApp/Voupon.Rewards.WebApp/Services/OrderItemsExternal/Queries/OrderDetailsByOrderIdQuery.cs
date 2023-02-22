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
    public class OrderDetailsByOrderIdQuery : IRequest<ApiResponseViewModel>
    {
        public Guid OrderId { get; set; }

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

                var orderDetails = await rewardsDBContext.Orders.AsNoTracking().Where(x => x.Id == request.OrderId).FirstOrDefaultAsync();
                orderDetails.Logs = "";



                if (orderDetails != null)
                {
                    response.Successful = true;
                    response.Message = "Get Order Details Successfully";
                    response.Data = orderDetails;
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

        public class ExternalOrderDetailsModel
        {
           public Voupon.Database.Postgres.RewardsEntities.Orders Order { get; set; }
            public ExternalOrderItemModel ExternalOrderItemsViewModel { get; set; }
        }
        public class ExternalOrderItemModel
        {
            public Guid Id { get; set; }
            public Guid OrderId { get; set; }
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalShopName { get; set; }
            public string ExternalUrl { get; set; }
            public string ProductCartPreviewSmallImageURL { get; set; }
            public string ProductTitle { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public string LastUpdatedByUser { get; set; }
            public decimal SubTotal { get; set; }
            public decimal TotalPrice { get; set; }
            public string VariationText { get; set; }
            public string ProductVariation { get; set; }
            public string JsonData { get; set; }
            public string TrackingNo { get; set; }
            public bool IsVariationProduct { get; set; }
            public int DealExpirationId { get; set; }
            public int CartProductType { get; set; }
            public string DiscountName { get; set; }
            public int? DiscountTypeId { get; set; }
            public decimal? DiscountPriceValue { get; set; }
            public int? DiscountPointRequired { get; set; }
            public decimal Price { get; set; }
            public int Points { get; set; }
            public int Quantity { get; set; }
            public byte OrderItemExternalStatus { get; set; }
            public string ShortId { get; set; }
            public decimal DiscountedAmount { get; set; }
            public decimal SubtotalPrice { get; set; }
            public decimal ShippingCost { get; set; }
        }
    }
}
