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
    public class ExternalOrderDetailsByOrderItmeShortIdQuery : IRequest<ApiResponseViewModel>
    {
        public Guid ExternalOrderId { get; set; }

    }
    public class ExternalOrderDetailsByOrderItmeShortIdQueryHandler : IRequestHandler<ExternalOrderDetailsByOrderItmeShortIdQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ExternalOrderDetailsByOrderItmeShortIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ExternalOrderDetailsByOrderItmeShortIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                ExternalOrderDetailsModel externalOrderModel = new ExternalOrderDetailsModel();
                var externalOrder = await rewardsDBContext.OrderItemExternal.AsNoTracking().Include(x=> x.OrderShopExternal).Where(x => x.Id == request.ExternalOrderId).FirstOrDefaultAsync();
                if (externalOrder != null)
                {
                    ExternalOrderItemModel externalOrderItem = new ExternalOrderItemModel()
                    {
                        CartProductType = externalOrder.CartProductType,
                        DealExpirationId = externalOrder.DealExpirationId,
                        DiscountedAmount = externalOrder.DiscountedAmount,
                        DiscountName = externalOrder.DiscountName,
                        DiscountPointRequired = externalOrder.DiscountPointRequired,
                        DiscountPriceValue = externalOrder.DiscountPriceValue,
                        DiscountTypeId = externalOrder.DiscountTypeId,
                        ExternalItemId = externalOrder.ExternalItemId,
                        ExternalShopId = externalOrder.OrderShopExternal.ExternalShopId,
                        ExternalShopName = externalOrder.OrderShopExternal.ExternalShopName,
                        ExternalTypeId = (byte)externalOrder.OrderShopExternal.ExternalTypeId,
                        ExternalUrl = externalOrder.ExternalUrl,
                        Id = externalOrder.Id,
                        IsVariationProduct = externalOrder.IsVariationProduct,
                        JsonData = externalOrder.JsonData,
                        LastUpdatedAt = externalOrder.LastUpdatedAt,
                        LastUpdatedByUser = externalOrder.LastUpdatedByUser,
                        OrderId = externalOrder.OrderShopExternal.OrderId,
                        OrderItemExternalStatus = (byte)externalOrder.OrderItemExternalStatus,
                        Points = externalOrder.Points,
                        Price = externalOrder.Price,
                        ProductCartPreviewSmallImageURL = externalOrder.ProductCartPreviewSmallImageURL,
                        ProductTitle = externalOrder.ProductTitle,
                        ProductVariation = externalOrder.ProductVariation,
                        ShippingCost = externalOrder.OrderShopExternal.ShippingCost,
                        ShortId = externalOrder.ShortId,
                        SubTotal = externalOrder.SubTotal,
                        SubtotalPrice = externalOrder.SubtotalPrice,
                        TotalPrice = externalOrder.TotalPrice,
                        TrackingNo = externalOrder.OrderShopExternal.TrackingNo,
                        VariationText = externalOrder.VariationText,
                        Quantity = externalOrder.Quantity
                    };
                    externalOrderModel.ExternalOrderItemsViewModel = externalOrderItem;
                    var orderDetails = await rewardsDBContext.Orders.AsNoTracking().Where(x => x.Id == externalOrder.OrderShopExternal.OrderId).FirstOrDefaultAsync();
                    orderDetails.Logs = "";
                    externalOrderModel.Order = orderDetails;

                }


                if (externalOrderModel != null)
                {
                    response.Successful = true;
                    response.Message = "Get Order Details Successfully";
                    response.Data = externalOrderModel;
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
