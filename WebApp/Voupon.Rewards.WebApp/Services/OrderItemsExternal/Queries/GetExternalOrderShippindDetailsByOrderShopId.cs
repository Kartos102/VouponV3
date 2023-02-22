using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries
{
    public class GetExternalOrderShippindDetailsByOrderShopId : IRequest<ApiResponseViewModel>
    {
        public Guid OrderShopId { get; set; }

    }
    public class GetExternalOrderShippindDetailsByOrderShopIdHandler : IRequestHandler<GetExternalOrderShippindDetailsByOrderShopId, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public GetExternalOrderShippindDetailsByOrderShopIdHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(GetExternalOrderShippindDetailsByOrderShopId request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var externalShopOrder = await rewardsDBContext.OrderShopExternal.Include(x => x.Order).Include(x => x.OrderItemExternal).Where(x => x.Id == request.OrderShopId).FirstOrDefaultAsync();
                if (externalShopOrder != null)
                {
                    var merchant = externalShopOrder.Order.OrderItems.FirstOrDefault();
                    ShippingInfoModel shippingInfoModel = new ShippingInfoModel()
                    {
                        TrackingList = new List<TrackingInfoModel>(),
                        TrackingLStatusist = new List<TrackingStatusModel>(),
                        TrackingNumber = externalShopOrder.TrackingNo,
                        CreatedAt = externalShopOrder.Order.CreatedAt,
                        ProductTitle = externalShopOrder.OrderItemExternal.FirstOrDefault().ProductTitle,
                        ProductImageUrl = externalShopOrder.OrderItemExternal.FirstOrDefault().ProductCartPreviewSmallImageURL,
                        UserInfo = new UserShippingInfoModel()
                        {
                            Name = externalShopOrder.Order.ShippingPersonFirstName + " " + externalShopOrder.Order.ShippingPersonLastName,
                            PhoneNumber = "(+" + externalShopOrder.Order.ShippingMobileCountryCode + ") " + externalShopOrder.Order.ShippingMobileNumber,
                            Address = externalShopOrder.Order.ShippingAddressLine1,
                        },
                        DeliveryStatuses = new DeliveryStatusesModel(),
                        Merchant = new MerchantInfo()
                        {
                            MerchantExternalId = externalShopOrder.ExternalShopId,
                            MerchantExternalTypeId = externalShopOrder.ExternalTypeId,
                            MerchantName = externalShopOrder.ExternalShopName
                        },
                    };
                    var orderItems = externalShopOrder.OrderItemExternal.ToList();

                    List<OrderItemExternal> orderItemsExternal = new List<OrderItemExternal>();
                    foreach (var item in orderItems)
                    {
                        OrderItemExternal externalItem = new OrderItemExternal();
                        externalItem.Id = item.Id;
                        externalItem.ExternalItemId = item.ExternalItemId;
                        externalItem.ProductTitle = item.ProductTitle;
                        externalItem.ProductCartPreviewSmallImageURL = item.ProductCartPreviewSmallImageURL;
                        externalItem.Price = item.Price;
                        externalItem.Quantity = item.Quantity;
                        externalItem.OriginalPrice = item.OriginalPrice;
                        externalItem.OrderShopExternalId = item.OrderShopExternalId;
                        externalItem.ShortId = item.ShortId;

                        orderItemsExternal.Add(externalItem);

                    }
                    shippingInfoModel.OrderItem = orderItemsExternal;
                    if (externalShopOrder.Order.ShippingAddressLine1 != "" && externalShopOrder.Order.ShippingAddressLine1 != null)
                    {
                        string shippingAddress2 = "";
                        if (externalShopOrder.Order.ShippingAddressLine2 != "" && externalShopOrder.Order.ShippingAddressLine2 != null)
                        {
                            shippingAddress2 = ", " + externalShopOrder.Order.ShippingAddressLine2;
                        }
                        shippingInfoModel.UserInfo.Address += shippingAddress2 + ", " + externalShopOrder.Order.ShippingCity + ", " + externalShopOrder.Order.ShippingPostcode + ", " + externalShopOrder.Order.ShippingState + ", " + externalShopOrder.Order.ShippingCountry;

                    }
                    else
                    {
                        shippingInfoModel.UserInfo.Address += ", " + externalShopOrder.Order.ShippingCity + ", " + externalShopOrder.Order.ShippingPostcode + ", " + externalShopOrder.Order.ShippingState + ", " + externalShopOrder.Order.ShippingCountry;
                    }
                    var jsonStr = "";
                    if (externalShopOrder.ShippingDetailsJson != null)
                    {
                        jsonStr = externalShopOrder.ShippingDetailsJson.Substring(1, externalShopOrder.ShippingDetailsJson.Length - 2);
                    }

                    var jsonShippingDetails = JsonConvert.DeserializeObject<ShippingJsonObj>(jsonStr);
                    if (jsonShippingDetails != null)
                    {
                        if (jsonShippingDetails.trackingNumber != null && jsonShippingDetails.trackingNumber != "")
                        {
                            externalShopOrder.TrackingNo = jsonShippingDetails.trackingNumber;
                        }
                        //rewardsDBContext.SaveChanges();

                        foreach (var trackingRecord in jsonShippingDetails.trackingList)
                        {

                            TrackingInfoModel trackingInfoModel = new TrackingInfoModel
                            {
                                CreatedAt = UnixTimeStampToNormalDateTime(trackingRecord.createdAt),
                                Description = trackingRecord.description
                            };
                            shippingInfoModel.TrackingList.Add(trackingInfoModel);

                        }
                        shippingInfoModel.Courier = jsonShippingDetails.courier.text;
                        shippingInfoModel.NumberOfParcel = jsonShippingDetails.numberOfParcel;

                        shippingInfoModel.DeliveryStatuses.CompletedAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.completedAt);
                        shippingInfoModel.DeliveryStatuses.IsRated = jsonShippingDetails.statuses.isRated;
                        shippingInfoModel.DeliveryStatuses.OrderPlacedAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.orderPlacedAt);
                        shippingInfoModel.DeliveryStatuses.OrderSerialNumber = jsonShippingDetails.statuses.orderSerialNumber;
                        if (jsonShippingDetails.statuses.orderShipppedOutAt == 0 || jsonShippingDetails.statuses.orderShipppedOutAt == jsonShippingDetails.statuses.paidAt)
                        {
                            shippingInfoModel.DeliveryStatuses.OrderShipppedOutAt = null;

                        }
                        else
                        {
                            shippingInfoModel.DeliveryStatuses.OrderShipppedOutAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.orderShipppedOutAt);
                        }
                        if (jsonShippingDetails.statuses.orderReceivedAt == 0)
                        {
                            shippingInfoModel.DeliveryStatuses.OrderReceivedAt = null;
                        }
                        else
                        {
                            shippingInfoModel.DeliveryStatuses.OrderReceivedAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.orderReceivedAt);
                        }
                        shippingInfoModel.DeliveryStatuses.PaidAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.paidAt);
                    }
                    if (externalShopOrder.OrderItemExternal.Count() > 0)
                    {
                        shippingInfoModel.PaidAmount = externalShopOrder.OrderItemExternal.Sum((x => x.TotalPrice));
                    }
                    shippingInfoModel.PaidAmount = shippingInfoModel.PaidAmount + externalShopOrder.ShippingCost;
                    //shippingInfoModel.DeliveryStatuses.PaidAmount = jsonShippingDetails.statuses.paidAmount;
                    //TrackingStatusModel trackingStatusModel = new TrackingStatusModel()
                    //{
                    //    CreatedAt = externalShopOrder.Order.CreatedAt,
                    //    Id = 1,
                    //    IsDone = true
                    //};

                    //shippingInfoModel.TrackingLStatusist.Add(trackingStatusModel);

                    //trackingStatusModel = new TrackingStatusModel()
                    //{
                    //    CreatedAt = externalShopOrder.Order.CreatedAt,
                    //    Id = 2,
                    //    IsDone = true
                    //};

                    //shippingInfoModel.TrackingLStatusist.Add(trackingStatusModel);

                    //trackingStatusModel = new TrackingStatusModel()
                    //{
                    //    CreatedAt = externalShopOrder.Order.CreatedAt,
                    //    Id = 3,
                    //    IsDone = true
                    //};

                    //shippingInfoModel.TrackingLStatusist.Add(trackingStatusModel);



                    response.Successful = true;
                    response.Message = "Get Order Shipping Details Successfully";
                    response.Data = shippingInfoModel;
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

        private DateTime UnixTimeStampToNormalDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public class ShippingJsonObj
        {
            public int numberOfParcel { get; set; }
            public CourierModel courier { get; set; }
            public statuses statuses { get; set; }
            public List<JsonTrackingList> trackingList { get; set; }
            public string trackingNumber { get; set; }

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

            public ICollection<OrderItemExternal> OrderItem { get; set; }
            public string TrackingNumber { get; set; }
            public string Courier { get; set; }
            public string ProductTitle { get; set; }
            public string ProductImageUrl { get; set; }
            public int NumberOfParcel { get; set; }
            public UserShippingInfoModel UserInfo { get; set; }
            public DateTime CreatedAt { get; set; }
            public DeliveryStatusesModel DeliveryStatuses { get; set; }
            public decimal PaidAmount { get; set; }

            public MerchantInfo Merchant { get; set; }
        }

        public class MerchantInfo
        {
            public int MerchantId { get; set; }

            public string MerchantExternalId { get; set; }

            public string MerchantName { get; set; }

            public short MerchantExternalTypeId { get; set; }
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
