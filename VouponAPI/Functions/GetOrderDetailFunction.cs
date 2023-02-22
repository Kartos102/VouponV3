using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

using System.Linq;

using System.Threading.Tasks;

using Voupon.Database.Postgres.RewardsEntities;

using Voupon.API.ViewModels;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;
using Voupon.API.Util;
using Voupon.Common.Azure.Blob;

namespace Voupon.API.Functions
{
    public class GetOrderDetailFunction
    {
        RewardsDBContext rewardsDBContext;
        private int _masterMemberProfileId;
        private readonly IAzureBlobStorage azureBlobStorage;
        public GetOrderDetailFunction(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.azureBlobStorage = azureBlobStorage;
        }
        [OpenApiOperation(operationId: "Get Order Detail", tags: new[] { "order" }, Description = "Get order detail by id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "type", In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "type 1 merchant order, 2 external order", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(Guid), Description = "order item id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "merchantId", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "merchant id", Visibility = OpenApiVisibilityType.Important)]

        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderDetailModel), Summary = "the result is order detail")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no order id is supplied")]

        [FunctionName("GetOrderDetail")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/detail")] HttpRequest req, ILogger log)
        {
            try
            {
                var response = new OrderDetailModel
                {
                    Data = new OrderDetailData()
                };

                var auth = new Authentication(req);
                if (!auth.IsValid)
                {
                    response.RequireLogin = true;
                    response.ErrorMessage = "Invalid token provided. Please re-login first.";
                    return new BadRequestObjectResult(response);
                }
                _masterMemberProfileId = auth.MasterMemberProfileId;

                var id = req.Query["id"];
                Guid orderId = new Guid();
                try
                {
                    orderId = new Guid(id);
                }
                catch (Exception ex) // There might be a more appropriate exception to catch
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid Order Id"
                    });
                }
                var merchantId = req.Query["merchantId"];
                int typeId = 0;
                try
                {
                    int.TryParse(req.Query["type"], out typeId);
                }


                catch (Exception ex) // There might be a more appropriate exception to catch
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid Type"
                    });
                }




                if (typeId == 1)
                {
                    int _merchantId = 0;
                    try
                    {
                        int.TryParse(merchantId, out _merchantId);
                    }
                    catch  {

                        //  log
                        response.Code = -1;
                        response.Data.Message = "Invalid Merchant Id";
                        return new NotFoundObjectResult(response);
                    }

                    response.Data.OrderDetail = await GetOrderMerchantDetail(orderId, _merchantId);
                }
                else if (typeId == 2)
                {

                    response.Data.OrderDetail = await GetExternalOrderDetail(orderId, merchantId);
                }

                if (response.Data.OrderDetail == null)
                {
                    //  log
                    response.Code = 0;
                    response.Data.Message = "Order Not Found";
                    return new NotFoundObjectResult(response);
                }
                response.Code = 0;
                response.Data.IsSuccessful = true;
                response.Data.Message = "Successfully get order detail";


                return new OkObjectResult(response);

            }
            catch (Exception ex)
            {
                //  log
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "Fail to get data [099]"
                });

            }
        }

        private async Task<OrderDetail> GetExternalOrderDetail(Guid orderId, string merchantId)
        {
            var externalOrderItems = await rewardsDBContext.OrderItemExternal.Include(x => x.OrderShopExternal).ThenInclude(x => x.Order)
                .Where(x=> x.OrderShopExternal.OrderId == orderId &&  x.OrderShopExternal.ExternalShopId == merchantId && x.OrderShopExternal.Order.MasterMemberProfileId == _masterMemberProfileId)
                .ToListAsync();
            if (externalOrderItems != null && externalOrderItems.Count > 0)
            {
                var merchant = externalOrderItems.FirstOrDefault();
                var externalShopOrder = externalOrderItems.FirstOrDefault().OrderShopExternal;

                OrderDetail detail = new OrderDetail()
                {
                    ExternalOrders = new ExternalOrder()
                    {
                        TrackingNumber = externalShopOrder.TrackingNo,
                        TrackingList = new List<TrackingInfoModel>(),
                        TrackingLStatusist = new List<TrackingStatusModel>()
                    },

                    CreatedAt = externalShopOrder.Order.CreatedAt.ToString("dd-M-yyyy HH:mm"),
                    ShippingDetail = new ShippingDetail()
                    {
                        Name = externalShopOrder.Order.ShippingPersonFirstName + " " + externalShopOrder.Order.ShippingPersonLastName,
                        PhoneNumber = "(+" + externalShopOrder.Order.ShippingMobileCountryCode + ") " + externalShopOrder.Order.ShippingMobileNumber,
                        Address = externalShopOrder.Order.ShippingAddressLine1,
                    },
                    MerchantInfo = new MerchantDetails()
                    {
                        MerchantExternalId = externalShopOrder.ExternalShopId,
                        MerchantExternalTypeId = externalShopOrder.ExternalTypeId,
                        Name = externalShopOrder.ExternalShopName
                    },
                };
                var orderItems = externalShopOrder.OrderItemExternal.ToList();

                List<OrderItemModel> orderItemsExternal = new List<OrderItemModel>();
                foreach (var item in orderItems)
                {
                    var externalItem = new OrderItemModel();
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
                detail.Items = orderItemsExternal;
                if (externalShopOrder.Order.ShippingAddressLine1 != "" && externalShopOrder.Order.ShippingAddressLine1 != null)
                {
                    string shippingAddress2 = "";
                    if (externalShopOrder.Order.ShippingAddressLine2 != "" && externalShopOrder.Order.ShippingAddressLine2 != null)
                    {
                        shippingAddress2 = ", " + externalShopOrder.Order.ShippingAddressLine2;
                    }
                    detail.ShippingDetail.Address += shippingAddress2 + ", " + externalShopOrder.Order.ShippingCity + ", " + externalShopOrder.Order.ShippingPostcode + ", " + externalShopOrder.Order.ShippingState + ", " + externalShopOrder.Order.ShippingCountry;

                }
                else
                {
                    detail.ShippingDetail.Address += ", " + externalShopOrder.Order.ShippingCity + ", " + externalShopOrder.Order.ShippingPostcode + ", " + externalShopOrder.Order.ShippingState + ", " + externalShopOrder.Order.ShippingCountry;
                }
                var jsonStr = "";
                if (externalShopOrder.ShippingDetailsJson != null)
                {
                    jsonStr = externalShopOrder.ShippingDetailsJson.Substring(1, externalShopOrder.ShippingDetailsJson.Length - 2);
                }

                var jsonShippingDetails = JsonConvert.DeserializeObject<ShippingJsonObj>(jsonStr);
                if (jsonShippingDetails != null)
                {
                    detail.ExternalOrders.DeliveryStatuses = new DeliveryStatusesModel();
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
                        detail.ExternalOrders.TrackingList.Add(trackingInfoModel);

                    }
                    detail.Courier = jsonShippingDetails.courier.text;
                    detail.ExternalOrders.NumberOfParcel = jsonShippingDetails.numberOfParcel;

                    detail.ExternalOrders.DeliveryStatuses.CompletedAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.completedAt);
                    detail.ExternalOrders.DeliveryStatuses.IsRated = jsonShippingDetails.statuses.isRated;
                    detail.ExternalOrders.DeliveryStatuses.OrderPlacedAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.orderPlacedAt);
                    detail.ExternalOrders.DeliveryStatuses.OrderSerialNumber = jsonShippingDetails.statuses.orderSerialNumber;
                    if (jsonShippingDetails.statuses.orderShipppedOutAt == 0 || jsonShippingDetails.statuses.orderShipppedOutAt == jsonShippingDetails.statuses.paidAt)
                    {
                        detail.ExternalOrders.DeliveryStatuses.OrderShipppedOutAt = null;

                    }
                    else
                    {
                        detail.ExternalOrders.DeliveryStatuses.OrderShipppedOutAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.orderShipppedOutAt);
                    }
                    if (jsonShippingDetails.statuses.orderReceivedAt == 0)
                    {
                        detail.ExternalOrders.DeliveryStatuses.OrderReceivedAt = null;
                    }
                    else
                    {
                        detail.ExternalOrders.DeliveryStatuses.OrderReceivedAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.orderReceivedAt);
                    }
                    detail.ExternalOrders.DeliveryStatuses.PaidAt = UnixTimeStampToNormalDateTime(jsonShippingDetails.statuses.paidAt);
                }
                if (externalShopOrder.OrderItemExternal.Count() > 0)
                {
                    detail.PaidAmount = externalShopOrder.OrderItemExternal.Sum((x => x.TotalPrice));
                }
                detail.PaidAmount = detail.PaidAmount + externalShopOrder.ShippingCost;

                return detail;
            }
            return null;
        }

        private async Task<OrderDetail> GetOrderMerchantDetail(Guid OrderId, int merchantId)
        {
            var orderItems = await rewardsDBContext.OrderItems.Include(x=> x.Order)
                .Where(x => x.OrderId == OrderId && x.MerchantId == merchantId && x.Order.MasterMemberProfileId == _masterMemberProfileId).ToListAsync();
            OrderDetail detail = new OrderDetail()
            {
                Items = new List<OrderItemModel>(),
                MerchantOrder = new MerchantOrder(),
                ShippingDetail = new ShippingDetail(),
            };
            if (orderItems != null && orderItems.Count > 0)
            {
                var order = orderItems.FirstOrDefault().Order;
                List<Guid> ids = orderItems.Select(x=> x.Id).ToList();
                var reviews = await rewardsDBContext.ProductReview.AsNoTracking().Where(x => ids.Contains(x.OrderItemId)).ToListAsync();

                foreach (var item in orderItems) {

                    var review = reviews.Where(x => x.OrderItemId == item.Id).FirstOrDefault();
                    OrderItemModel prod = new OrderItemModel();
                    prod.ProductTitle = item.ProductTitle;
                    prod.Id = item.Id;
                    prod.ProductId = item.ProductId;
                    prod.ProductTitle = item.ProductTitle;
                    prod.ProductCartPreviewSmallImageURL = await GetBlobImage(item.ProductId);
                    prod.Price = item.Price;
                    prod.ShortId = item.ShortId;
                    prod.IsReviewed = review != null ? true : false;
                    if (!String.IsNullOrEmpty(item.ProductDetail)) 
                    {
                        ProductList product = JsonConvert.DeserializeObject<ProductList>(item.ProductDetail);
                        prod.Quantity = (int)product.OrderQuantity;
                        prod.OriginalPrice = (decimal)product.Price;
                        prod.DiscountPriceValue = (decimal)product.DiscountedPrice;
                        detail.MerchantOrder.RedemptionType = product.DealExpiration.Type;
                    }

                    switch (detail.MerchantOrder.RedemptionType)
                    {
                        case 4: GetDigitalRedemption(ref detail, item.Id); break;
                        case 5: GetDeliveryRedemption(ref detail, item.Id); break;
                    }

                    detail.Items.Add(prod);
                }
                
     


                detail.ShippingDetail.Name = order.ShippingPersonFirstName + " " + order.ShippingPersonLastName;
                detail.ShippingDetail.Address = "(+" + order.ShippingMobileCountryCode + ") " + order.ShippingMobileNumber + "  "+//+ "<br>" +
                   order.ShippingAddressLine1;
                if (order.ShippingAddressLine2 != "" && order.ShippingAddressLine2 != null)
                {
                    detail.ShippingDetail.Address += ", " + order.ShippingAddressLine2 + ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry;
                }
                else
                {
                    detail.ShippingDetail.Address += ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry;
                }
                detail.OrderedAt = order.CreatedAt.ToString("dd-M-yyyy HH:mm");

                detail.MerchantInfo = new MerchantDetails();

                detail.MerchantInfo.Name = orderItems.FirstOrDefault().MerchantDisplayName;
                var merchant = rewardsDBContext.Merchants.Where(x => x.Id == orderItems.FirstOrDefault().MerchantId).FirstOrDefault();
                if (merchant != null)
                {
                    detail.MerchantInfo.ImageUrl = merchant.LogoUrl;

                    var merchantManagerUser = rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.UserRoles.Any(y => y.MerchantId == merchant.Id && y.RoleId.ToString() == "1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefault();
                    if (merchantManagerUser != null)
                    {
                        detail.MerchantInfo.Email = merchantManagerUser.Email;
                    }
                }
                detail.Status = order.OrderStatus;

            }
            else
            {
                return null;
            }
            return detail;
        }

        private void GetDigitalRedemption(ref OrderDetail detail, Guid OrderItemId)
        {
            //Digital Redemption
            var digitalRedemption = rewardsDBContext.DigitalRedemptionTokens.FirstOrDefault(x => x.OrderItemId == OrderItemId);
            if (digitalRedemption != null)
            {
                detail.Courier = "";
                detail.CreatedAt = digitalRedemption.CreatedAt.ToString("dd-M-yyyy HH:mm");
                detail.MerchantOrder.IsRedeemed = digitalRedemption.IsRedeemed;
                detail.MerchantOrder.RedeemedAt = digitalRedemption.RedeemedAt.HasValue ? digitalRedemption.RedeemedAt.Value.ToString("dd-M-yyyy HH:mm") : "";
                detail.MerchantOrder.TokenType = digitalRedemption.TokenType;
                detail.MerchantOrder.Token = digitalRedemption.Token;
                detail.MerchantOrder.ValidFrom = digitalRedemption.StartDate.ToString("dd-M-yyyy HH:mm");
                detail.MerchantOrder.ValidEnded = digitalRedemption.ExpiredDate.ToString("dd-M-yyyy HH:mm");
                detail.PaidAmount = digitalRedemption.Revenue.Value;

            }

        }

        private void GetDeliveryRedemption(ref OrderDetail detail, Guid OrderItemId)
        {
            var deliveryRedemption = rewardsDBContext.DeliveryRedemptionTokens.FirstOrDefault(x => x.OrderItemId == OrderItemId);
            if (deliveryRedemption != null)
            {
                detail.PaidAmount = deliveryRedemption.Revenue.Value;
                detail.Courier = deliveryRedemption.CourierProvider;
                detail.CreatedAt = deliveryRedemption.CreatedAt.ToString("dd-M-yyyy HH:mm");
                detail.MerchantOrder.IsRedeemed = deliveryRedemption.IsRedeemed;
                detail.MerchantOrder.RedeemedAt = deliveryRedemption.RedeemedAt.HasValue ? deliveryRedemption.RedeemedAt.Value.ToString("dd-M-yyyy HH:mm") : "";
                detail.MerchantOrder.Token = deliveryRedemption.Token;
                detail.MerchantOrder.ValidFrom = deliveryRedemption.StartDate.HasValue ? deliveryRedemption.StartDate.Value.ToString("dd-M-yyyy HH:mm") : "";
                detail.MerchantOrder.ValidEnded = deliveryRedemption.ExpiredDate.HasValue ? deliveryRedemption.ExpiredDate.Value.ToString("dd-M-yyyy HH:mm") : "";
                detail.MerchantOrder.ExpectedDeliveryDate = deliveryRedemption.CreatedAt.AddDays(14).ToString("dd-M-yyyy HH:mm");
            }

        }


        private class OrderDetailModel : ApiResponseViewModel
        {
            public OrderDetailData Data { get; set; }
        }

        private class OrderDetailData
        {
            public OrderDetail OrderDetail { get; set; }
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }

        private class OrderDetail
        {

            public List<OrderItemModel> Items { get; set; }

            public string CreatedAt { get; set; }
            public string OrderedAt { get; set; }
            public decimal? PaidAmount { get; set; }

            public string Courier { get; set; }

            public short Status { get; set; }

            public ShippingDetail ShippingDetail { get; set; }

            public MerchantOrder MerchantOrder { get; set; }

            public ExternalOrder ExternalOrders { get; set; }
            public MerchantDetails MerchantInfo { get; set; }

        }

        private class ProductDetails { 
            public string Title { get; set; }
            public string ImageUrl { get; set; }
        }

        public class ExternalOrder
        {
            public List<TrackingInfoModel> TrackingList { get; set; }
            public List<TrackingStatusModel> TrackingLStatusist { get; set; }

            public string TrackingNumber { get; set; }
            public int NumberOfParcel { get; set; }
            

            public DeliveryStatusesModel DeliveryStatuses { get; set; }
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
        private class MerchantOrder
        {

            public string ValidFrom { get; set; }
            public string ValidEnded { get; set; }
            public string RedeemedAt { get; set; }
            public bool IsRedeemed { get; set; }

            public string Token { get; set; }

            public short TokenType { get; set; }

            public string Url { get; set; }

            public int RedemptionType { get; set; }

            public string ExpectedDeliveryDate { get; set; }
        }
        private class ShippingDetail
        {
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }

        }

        private class MerchantDetails
        {
            public int MerchantId { get; set; }

            public string MerchantExternalId { get; set; }
            public string Name { get; set; }

            public string ImageUrl { get; set; }
            public string Email { get; set; }

            public short MerchantExternalTypeId { get; set; }
        }
        private class ProductList
        {

            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("productId")]
            public long ProductId { get; set; }

            [JsonProperty("isVariationProduct")]
            public bool IsVariationProduct { get; set; }

            [JsonProperty("variationId")]
            public long VariationId { get; set; }

            [JsonProperty("variationText")]
            public string VariationText { get; set; }

            [JsonProperty("typeId")]
            public long TypeId { get; set; }

            [JsonProperty("productCategory")]
            public string ProductCategory { get; set; }

            [JsonProperty("productSubCategory")]
            public string ProductSubCategory { get; set; }

            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("merchant")]
            public MerchantDetails Merchant { get; set; }

            [JsonProperty("pointsRequired")]
            public int PointsRequired { get; set; }

            [JsonProperty("price")]
            public double Price { get; set; }

            [JsonProperty("discountedPrice")]
            public double DiscountedPrice { get; set; }

            [JsonProperty("discountRate")]
            public long DiscountRate { get; set; }

            [JsonProperty("productImages")]
            public List<Uri> ProductImages { get; set; }

            [JsonProperty("productCartPreviewSmallImage")]
            public string ProductCartPreviewSmallImage { get; set; }

            [JsonProperty("subTotal")]
            public string SubTotal { get; set; }

            [JsonProperty("totalPrice")]
            public string TotalPrice { get; set; }

            [JsonProperty("orderQuantity")]
            [JsonConverter(typeof(ParseStringConverter))]
            public long OrderQuantity { get; set; }

            [JsonProperty("additionalDiscount")]
            public AdditionalDiscount AdditionalDiscount { get; set; }

            [JsonProperty("dealExpiration", NullValueHandling = NullValueHandling.Ignore)]
            public DealExpiration? DealExpiration { get; set; }

            [JsonProperty("externalTypeId", NullValueHandling = NullValueHandling.Ignore)]
            public byte ExternalTypeId { get; set; }

            [JsonProperty("externalItemId", NullValueHandling = NullValueHandling.Ignore)]
            public string ExternalItemId { get; set; }

            [JsonProperty("externalShopId", NullValueHandling = NullValueHandling.Ignore)]
            public string ExternalShopId { get; set; }

            [JsonProperty("externalId", NullValueHandling = NullValueHandling.Ignore)]
            public Guid? ExternalId { get; set; }

            [JsonProperty("externalShopUrl", NullValueHandling = NullValueHandling.Ignore)]
            public string ExternalShopUrl { get; set; }

            [JsonProperty("externalItemUrl", NullValueHandling = NullValueHandling.Ignore)]
            public string ExternalItemUrl { get; set; }

            [JsonProperty("shippingCost")]
            public decimal ShippingCost { get; set; }
        }

        private class ParseStringConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                long l;
                if (Int64.TryParse(value, out l))
                {
                    return l;
                }
                throw new Exception("Cannot unmarshal type long");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (long)untypedValue;
                serializer.Serialize(writer, value.ToString());
                return;
            }

            public static readonly ParseStringConverter Singleton = new ParseStringConverter();
        }
        private class AdditionalDiscount
        {
            [JsonProperty("type")]
            [JsonConverter(typeof(DecodingChoiceConverter))]
            public long Type { get; set; }

            [JsonProperty("id")]
            [JsonConverter(typeof(DecodingChoiceConverter))]
            public long Id { get; set; }

            [JsonProperty("value")]
            public decimal Value { get; set; }

            [JsonProperty("pointsRequired")]
            [JsonConverter(typeof(DecodingChoiceConverter))]
            public long PointsRequired { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("ExternalItemDiscount")]
            public int ExternalItemDiscountPercentage { get; set; }
        }

        private class DecodingChoiceConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                switch (reader.TokenType)
                {
                    case JsonToken.Integer:
                        var integerValue = serializer.Deserialize<long>(reader);
                        return integerValue;
                    case JsonToken.String:
                    case JsonToken.Date:
                        var stringValue = serializer.Deserialize<string>(reader);
                        long l;
                        if (Int64.TryParse(stringValue, out l))
                        {
                            return l;
                        }
                        break;
                }
                throw new Exception("Cannot unmarshal type long");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (long)untypedValue;
                serializer.Serialize(writer, value);
                return;
            }

            public static readonly DecodingChoiceConverter Singleton = new DecodingChoiceConverter();
        }

        private class DealExpiration
        {

            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public int Type { get; set; }

            [JsonProperty("totalValidDays", NullValueHandling = NullValueHandling.Ignore)]
            public int? TotalValidDays { get; set; }

            [JsonProperty("startDate")]
            public string StartDate { get; set; }

            [JsonProperty("expiredDate")]
            public string ExpiredDate { get; set; }

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

        public class ShippingJsonObj
        {
            public int numberOfParcel { get; set; }
            public CourierModel courier { get; set; }
            public statuses statuses { get; set; }
            public List<JsonTrackingList> trackingList { get; set; }
            public string trackingNumber { get; set; }

        }

        public class JsonTrackingList
        {
            public double createdAt { get; set; }
            public string description { get; set; }

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

        public class CourierModel
        {
            public string text { get; set; }

        }

        public class OrderItemModel
        {
            public Guid Id { get; set; }

            public int ProductId { get; set; }
            public Guid OrderShopExternalId { get; set; }
            public string ExternalItemId { get; set; }
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
            public bool IsVariationProduct { get; set; }
            public int DealExpirationId { get; set; }
            public int CartProductType { get; set; }
            public int Quantity { get; set; }
            public string DiscountName { get; set; }
            public int? DiscountTypeId { get; set; }
            public decimal? DiscountPriceValue { get; set; }
            public int? DiscountPointRequired { get; set; }
            public decimal Price { get; set; }
            public int Points { get; set; }
            public byte OrderItemExternalStatus { get; set; }
            public string ShortId { get; set; }
            public decimal DiscountedAmount { get; set; }
            public decimal SubtotalPrice { get; set; }
            public decimal OriginalPrice { get; set; }
            public decimal? VPointsMultiplier { get; set; }
            public decimal? VPointsMultiplierCap { get; set; } 

            public bool IsReviewed { get; set; }
        }

        private DateTime UnixTimeStampToNormalDateTime(double unixTimeStamp)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private async Task<string> GetBlobImage(int productId)
        {
            var imageUrl = "";
            var azureBlobResult = await azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Products, productId + "/" + FilePathEnum.Products_Images);
            if (azureBlobResult != null && azureBlobResult.Any())
            {
                var fileList = new List<string>();

                foreach (var file in azureBlobResult)
                {
                    if (file.StorageUri.PrimaryUri.OriginalString.Contains("big") || file.StorageUri.PrimaryUri.OriginalString.Contains("normal") || file.StorageUri.PrimaryUri.OriginalString.Contains("org"))
                    {
                        continue;
                    }
                    fileList.Add(file.StorageUri.PrimaryUri.OriginalString);

                }
                imageUrl = fileList.FirstOrDefault();

                if (imageUrl == "" || imageUrl == null)
                {
                    imageUrl = azureBlobResult.FirstOrDefault().StorageUri.PrimaryUri.OriginalString;
                }
            }
            return imageUrl;
        }
    }
}
