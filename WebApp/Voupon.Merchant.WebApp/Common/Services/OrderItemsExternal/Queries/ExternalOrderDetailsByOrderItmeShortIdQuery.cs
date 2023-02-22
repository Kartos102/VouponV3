using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
    public class ExternalOrderDetailsByOrderItmeShortIdQuery : IRequest<ApiResponseViewModel>
    {
        public string ExternalOrderId { get; set; }

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
                var externalOrder = await rewardsDBContext.OrderItemExternal.AsNoTracking().Include(x=> x.OrderShopExternal).Where(x => x.Id == new Guid(request.ExternalOrderId)/* && x.OrderItemExternalStatus != 5*/).FirstOrDefaultAsync();
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
                        ExternalTypeId = externalOrder.OrderShopExternal.ExternalTypeId,
                        ExternalUrl = externalOrder.ExternalUrl,
                        Id = externalOrder.Id,
                        IsVariationProduct = externalOrder.IsVariationProduct,
                        JsonData = externalOrder.JsonData,
                        LastUpdatedAt = externalOrder.LastUpdatedAt,
                        LastUpdatedByUser = externalOrder.LastUpdatedByUser,
                        OrderId = externalOrder.OrderShopExternal.OrderId,
                        OrderItemExternalStatus = externalOrder.OrderItemExternalStatus,
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
                    externalOrderModel.ShipingInfo = new ShippingInfo();
                    if (externalOrder.OrderShopExternal.ShippingDetailsJson != null && externalOrder.OrderShopExternal.ShippingDetailsJson != "Loading...")
                    {
                        var jsonStr = externalOrder.OrderShopExternal.ShippingDetailsJson.Substring(1, externalOrder.OrderShopExternal.ShippingDetailsJson.Length - 2);
                        var shippingInfo = JsonConvert.DeserializeObject<ShippingJsonObj>(jsonStr);

                        externalOrderModel.ShipingInfo.DatePurchased = UnixTimeStampToNormalDateTime(shippingInfo.statuses.orderPlacedAt);
                        externalOrderModel.ShipingInfo.ExternalOrderId = shippingInfo.statuses.orderSerialNumber;
                    }
                    if (externalOrder.OrderShopExternal.AdminAccountDetail != null && externalOrder.OrderShopExternal.AdminAccountDetail != "Loading...")
                    {
                        //var jsonStr = externalOrder.OrderShopExternal.AdminAccountDetail.Substring(1, externalOrder.OrderShopExternal.AdminAccountDetail.Length - 2);
                        var shippingUserInfo = JsonConvert.DeserializeObject<ShippingUserJsonObj>(externalOrder.OrderShopExternal.AdminAccountDetail);
                        externalOrderModel.ShipingInfo.ShippingAdminInfo = shippingUserInfo;
                    }
                    externalOrderModel.ShipingInfo.ExternalShippingId = externalOrder.OrderShopExternal.ExternalOrderId;
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
        private DateTime UnixTimeStampToNormalDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public class ExternalOrderDetailsModel
        {
           public Voupon.Database.Postgres.RewardsEntities.Orders Order { get; set; }
            public ExternalOrderItemModel ExternalOrderItemsViewModel { get; set; }
            public ShippingInfo ShipingInfo { get; set; }

        }
        public class ExternalOrderItemModel
        {
            public Guid Id { get; set; }
            public Guid OrderId { get; set; }
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public short ExternalTypeId { get; set; }
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
            public short OrderItemExternalStatus { get; set; }
            public string ShortId { get; set; }
            public decimal DiscountedAmount { get; set; }
            public decimal SubtotalPrice { get; set; }
            public decimal ShippingCost { get; set; }

        }

        public class ShippingInfo
        {
            public ShippingUserJsonObj ShippingAdminInfo { get; set; }
            public string ExternalOrderId { get; set; }
            public string ExternalShippingId { get; set; }
            public DateTime? DatePurchased { get; set; }

        }
        public class ShippingJsonObj
        {
            public int numberOfParcel { get; set; }
            public CourierModel courier { get; set; }
            public AddressModel address { get; set; }
            public statuses statuses { get; set; }
            public List<JsonTrackingList> trackingList { get; set; }
            public string trackingNumber { get; set; }

        }
        public class ShippingUserJsonObj
        {
            public string userId { get; set; }
            public string username { get; set; }
            public string email { get; set; }
            public string phone { get; set; }

        }

        public class statuses
        {
            public double paidAt { get; set; }
            public double paidAmount { get; set; }
            public bool isRated { get; set; }
            public string orderSerialNumber { get; set; }
            public double orderPlacedAt { get; set; }
            public double orderShipppedOutAt { get; set; }
            public double orderReceivedAt { get; set; }
            public double completedAt { get; set; }

        }

        public class DeliveryStatusesModel
        {
            public DateTime? PaidAt { get; set; }
            public decimal PaidAmount { get; set; }
            public bool IsRated { get; set; }
            public string OrderSerialNumber { get; set; }
            public DateTime? OrderPlacedAt { get; set; }
            public DateTime? OrderShipppedOutAt { get; set; }
            public DateTime? OrderReceivedAt { get; set; }
            public DateTime? CompletedAt { get; set; }

        }
        public class AddressModel
        {
            public string shippingAddress { get; set; }
            public string shippingName { get; set; }
            public string shippingPhone { get; set; }

        }
        public class CourierModel
        {
            public string text { get; set; }

        }

        public class JsonTrackingList
        {
            public double createdAt { get; set; }
            public string description { get; set; }

        }
        public class ShippingInfoModel
        {
            public List<TrackingInfoModel> TrackingList { get; set; }
            public List<TrackingStatusModel> TrackingLStatusist { get; set; }
            public string TrackingNumber { get; set; }
            public string Courier { get; set; }
            public int NumberOfParcel { get; set; }
            public UserShippingInfoModel UserInfo { get; set; }
            public DateTime CreatedAt { get; set; }
            public DeliveryStatusesModel DeliveryStatuses { get; set; }


        }
        public class TrackingInfoModel
        {
            public DateTime CreatedAt { get; set; }
            public string Description { get; set; }

        }

        public class TrackingStatusModel
        {
            public byte Id { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool IsDone { get; set; }

        }
        public class UserShippingInfoModel
        {
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }

        }
    }
}
