using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.API.Util;
using Voupon.API.ViewModels;
using System.ComponentModel;

namespace Voupon.API.Functions
{
    public class GetOrderHistoryFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly IAzureBlobStorage azureBlobStorage;
        private int _masterMemberProfileId;
        public GetOrderHistoryFunction(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get order history", tags: new[] { "order" }, Description = "Get order history", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "limit of items", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "offset", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "offset after number of items", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "search", In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "search query", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "orderStatus", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "order status", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderHistoryResponseModel), Summary = "The paginated result of purchase history")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no id or merchant id is supplied")]
        [FunctionName("GetOrderHistoryFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/history")] HttpRequest req, ILogger log)
        {
            int _limit = 0;
            int _offset = 0;
            string _search = req.Query["search"];
            var response = new OrderHistoryResponseModel
            {
                Data = new OrderHistoryData()
            };
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }

            int orderStatus = 0;
            int.TryParse(req.Query["orderStatus"], out orderStatus);

            if (orderStatus < 1 && orderStatus > 5)
            {
                response.ErrorMessage = "Invalid Order Status Request. ";
                return new BadRequestObjectResult(response);
            }

            _masterMemberProfileId = auth.MasterMemberProfileId;
            try
            {
                int.TryParse(req.Query["offset"], out _offset);

                int.TryParse(req.Query["limit"], out _limit);

                var query = rewardsDBContext.Orders.Include(x => x.OrderItems).Include(x => x.OrderShopExternal).
                    ThenInclude(x => x.OrderItemExternal).
                    Where(x => x.MasterMemberProfileId == _masterMemberProfileId).OrderByDescending(x => x.CreatedAt).AsNoTracking();

                List<int> statuses = GenerateOrderStatus(orderStatus);

                if (statuses.Count != 0)
                {
                    query = query.Where(x => statuses.Contains(x.OrderStatus));

                }

                if (!string.IsNullOrEmpty(_search))
                {
                    query = query.Select(x => new Orders
                    {

                        OrderItems = x.OrderItems.Where(z => z.ProductTitle.Contains(_search) && statuses.Contains(z.Status)).ToList(),
                        ShortId = x.ShortId,
                        CreatedAt = x.CreatedAt,
                        OrderStatus = x.OrderStatus,
                    });
                }

                var items = query.Skip(_limit).Take(_offset).ToList();

                List<OrderModel> orders = new List<OrderModel>();

                var listOrderGroupBy = items.GroupBy(x => x.Id);
                foreach (var orderGrouped in listOrderGroupBy)
                {
                    OrderModel odr = new OrderModel();
                    foreach (var order in orderGrouped)
                    {
                        odr.OrderId = order.Id;
                        odr.ShortId = order.ShortId;
                        odr.CreatedAt = order.CreatedAt;
                        odr.Status = order.OrderStatus;
                        if (order.OrderItems != null && order.OrderItems.Count > 0)
                        {
                            odr.Shops = await GetMerchantItem(order);
                        }
                        else if (order.OrderShopExternal != null && order.OrderShopExternal.Count > 0)
                        {
                            odr.Shops = await GetExternalShopItem(order);

                        }

                    }

                    orders.Add(odr);

                }
                response.Code = 0;
                response.Data.OrderHistory = orders.OrderBy(x => x.CreatedAt).ToList();
                return new OkObjectResult(response); ;
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    ErrorMessage = ex.Message
                }); ;
            }


        }


        private async Task<List<Merchant>> GetMerchantItem(Orders order)
        {
            var merchantGrouped = order.OrderItems.GroupBy(x => x.MerchantId);
            List<Merchant> listMerchant = new List<Merchant>();
            foreach (var m in merchantGrouped)
            {
                var merchant = new Merchant();
                merchant.MerchantId = m.FirstOrDefault().MerchantId.ToString();
                merchant.Name = m.FirstOrDefault().MerchantDisplayName;
                List<OrderItemModel> itms = new List<OrderItemModel>();
                itms = await OrderItemMapper(m.ToList(), order.CreatedAt, order.ShortId, order.OrderStatus);
                merchant.Items = itms;

                listMerchant.Add(merchant);
            }

            return listMerchant;

        }

        private async Task<List<Merchant>> GetExternalShopItem(Orders order)
        {
            var merchantGrouped = order.OrderShopExternal.GroupBy(x => x.ExternalShopId);
            List<Merchant> listMerchant = new List<Merchant>();
            foreach (var m in merchantGrouped)
            {
                var merchant = new Merchant();
                merchant.MerchantId = m.FirstOrDefault().ExternalShopId;
                merchant.MerchantExternalTypeId = m.FirstOrDefault().ExternalTypeId;
                merchant.Name = m.FirstOrDefault().ExternalShopName;
                List<OrderItemModel> itms = new List<OrderItemModel>();
                itms = OrderItemExternalMapper(m.ToList(), order.OrderShopExternal.FirstOrDefault().OrderId, order.CreatedAt, order.ShortId, order.OrderStatus);
                merchant.Items = itms;

                listMerchant.Add(merchant);
            }

            return listMerchant;

        }

        private class OrderHistoryResponseModel : ApiResponseViewModel
        {
            public OrderHistoryData Data { get; set; }
        }

        private class OrderHistoryData
        {
            public List<OrderModel> OrderHistory { get; set; }
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
            public Merchant Merchant { get; set; }

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
        private class Merchant
        {

            [JsonProperty("name")]
            public string Name { get; set; }

            public int MerchantExternalTypeId { get; set; }
            public string MerchantId { get; set; }

            public List<OrderItemModel> Items { get; set; }
        }


        private class OrderModel
        {
            public Guid OrderId { get; set; }
            public string ShortId { get; set; }

            public short Status { get; set; }

            public DateTime CreatedAt { get; set; }
            public List<Merchant> Shops { get; set; }

        }
        private class OrderItemModel
        {
            public Guid Id { get; set; }

            public int CartId { get; set; }
            public Guid OrderId { get; set; }
            public int ProductId { get; set; }
            public string ProductExternalId { get; set; }
            public int MerchantExternalTypeId { get; set; }
            public int MerchantId { get; set; }
            public string MerchantExternalId { get; set; }
            public string MerchantDisplayName { get; set; }

            //Commented out for Backlog 1776

            // public decimal Commision { get; set; }
            // public decimal Price { get; set; }

            //public decimal DiscountRate { get; set; }
            //public decimal DiscountedPrice { get; set; }
            //public decimal OriginalPrice { get; set; }

            public decimal DashedPrice { get; set; }
            public decimal FianlPrice { get; set; }
            //public decimal OriginaPrice { get; set; }

            public int PointsRequired { get; set; }
            public int Points { get; set; }
            public string ProductDetail { get; set; }
            public int ExpirationTypeId { get; set; }
            public string ProductTitle { get; set; }
            public string ProductImageFolderUrl { get; set; }
            public short Status { get; set; }
            public bool IsRedeemed { get; set; }
            public bool IsReviewed { get; set; }
            public int? VariationId { get; set; }
            public string VariationText { get; set; }
            public bool IsVariationProduct { get; set; }
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public short ExternalTypeId { get; set; }
            /*public ProductList Product { get; set; }*/
            public int Quantity { get; set; }
            //Customised for History Page

            //public AdditionalDiscount AdditionalDiscount { get; set; }
            public DateTime CreatedAt { get; set; }
            public string ShortId { get; set; }
            public short OrderStatus { get; set; }
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

        private async Task<List<OrderItemModel>> OrderItemMapper(List<OrderItems> items, DateTime created, string sortId, short orderStatus)
        {
            List<Guid> ids = items.Select(x => x.Id).ToList();

            var redemptions = await rewardsDBContext.InStoreRedemptionTokens.AsNoTracking().Where(x => ids.Contains(x.OrderItemId)).ToListAsync();
            var digitalRedemptions = await rewardsDBContext.DigitalRedemptionTokens.AsNoTracking().Where(x => ids.Contains(x.OrderItemId)).ToListAsync();
            var deliveryRedemptions = await rewardsDBContext.DeliveryRedemptionTokens.AsNoTracking().Where(x => ids.Contains(x.OrderItemId)).ToListAsync();
            var reviews = await rewardsDBContext.ProductReview.AsNoTracking().Where(x => ids.Contains(x.OrderItemId)).ToListAsync();

            var cart = await rewardsDBContext.CartProducts.Where(x => x.MasterMemberProfileId == _masterMemberProfileId).Select(x => new KeyValuePair<int, int>(x.ProductId, x.Id)).ToListAsync();

            List<OrderItemModel> result = new List<OrderItemModel>();
            foreach (var orderItemDb in items)
            {
                var redemption = redemptions.Where(x => x.OrderItemId == orderItemDb.Id).FirstOrDefault();
                var review = reviews.Where(x => x.OrderItemId == orderItemDb.Id).FirstOrDefault();

                var deliveryRedemption = deliveryRedemptions.Where(x => x.OrderItemId == orderItemDb.Id).FirstOrDefault();
                var digitalRedemption = digitalRedemptions.Where(x => x.OrderItemId == orderItemDb.Id).FirstOrDefault();

                OrderItemModel orderItem = new OrderItemModel();
                orderItem.Id = orderItemDb.Id;
                orderItem.OrderId = orderItemDb.OrderId;
                orderItem.ProductId = orderItemDb.ProductId;
                orderItem.MerchantId = orderItemDb.MerchantId;
                orderItem.MerchantDisplayName = orderItemDb.MerchantDisplayName;

                //Commented out as discussed to Achive backlog 1776

                //orderItem.Commision = orderItemDb.Commision;
                //orderItem.Price = orderItemDb.Price;
                orderItem.Points = orderItemDb.Points;
                orderItem.ProductDetail = orderItemDb.ProductDetail;
                orderItem.CartId = cart.Where(x => x.Key == orderItem.ProductId).FirstOrDefault().Value;

                ProductList product = new ProductList();
                try
                {

                    //Commented out as discussed to Achive backlog 1776

                    product = JsonConvert.DeserializeObject<ProductList>(orderItemDb.ProductDetail);
                    //orderItem.OriginalPrice = (decimal)product.Price;
                    //orderItem.DiscountedPrice = (decimal)product.DiscountedPrice;
                    orderItem.Quantity = (int)product.OrderQuantity;
                    
                    //orderItem.AdditionalDiscount = product.AdditionalDiscount;

                    var OriginalPrice = product.Price;
                    var DiscountedPrice = product.DiscountedPrice;
                    var FinaPrice = orderItemDb.Price;

                    if (OriginalPrice.Equals(DiscountedPrice))
                    {

                        orderItem.DashedPrice = (decimal)OriginalPrice;
                        orderItem.FianlPrice = (decimal)FinaPrice;
                    }

                    else if (DiscountedPrice.Equals(FinaPrice))
                    {

                        orderItem.DashedPrice = (decimal)OriginalPrice;
                        orderItem.FianlPrice = (decimal)FinaPrice;

                    }

                    else
                    {
                        orderItem.DashedPrice = (decimal)DiscountedPrice;
                        orderItem.FianlPrice = (decimal)FinaPrice;

                    }


                }


                catch (Exception ex)
                {
                    //  Log
                }

                //orderItemDb.Price --- > Final Calculated price 
                //product.Price ---- > The orignal price of the product without any discount
                //orderItem.DiscountedPric --- > The discount that given by merchan for the original price







                orderItem.ExpirationTypeId = orderItemDb.ExpirationTypeId;
                orderItem.ProductTitle = orderItemDb.ProductTitle;
                orderItem.ProductImageFolderUrl = orderItemDb.ProductImageFolderUrl;
                orderItem.Status = orderItemDb.Status;
                orderItem.IsVariationProduct = orderItemDb.IsVariationProduct;
                orderItem.VariationId = orderItemDb.VariationId;
                orderItem.VariationText = orderItemDb.VariationText;
                orderItem.Status = orderItemDb.Status;
                orderItem.IsRedeemed = false;
                orderItem.CreatedAt = created;
                orderItem.ShortId = sortId;
                orderItem.OrderStatus = orderStatus;
                orderItem.IsReviewed = review != null ? true : false;

                if (orderItem.ExpirationTypeId == 1 || orderItem.ExpirationTypeId == 2)
                {
                    orderItem.IsRedeemed = redemption != null ? redemption.IsRedeemed : false;
                }
                else if (orderItem.ExpirationTypeId == 4)
                {
                    orderItem.IsRedeemed = digitalRedemption != null ? digitalRedemption.IsRedeemed : false;
                }
                else
                {
                    orderItem.IsRedeemed = deliveryRedemption != null ? deliveryRedemption.IsRedeemed : false;
                }


                orderItem.ProductImageFolderUrl = await GetBlobImage(orderItem.ProductId);
                result.Add(orderItem);
            }

            return result;
        }

        private List<OrderItemModel> OrderItemExternalMapper(List<OrderShopExternal> items, Guid orderId, DateTime created, string sortId, short orderStatus)
        {
            List<OrderItemModel> result = new List<OrderItemModel>();
            foreach (var orderItemShopExternal in items)
            {
                if (orderItemShopExternal.OrderItemExternal != null && orderItemShopExternal.OrderItemExternal.Count > 0)
                {
                    foreach (var orderExternalItemDb in orderItemShopExternal.OrderItemExternal)
                    {
                        OrderItemModel orderItem = new OrderItemModel();
                        orderItem.Id = orderExternalItemDb.Id;
                        orderItem.OrderId = orderId;

                        //orderItem.Price = orderExternalItemDb.Price;
                        orderItem.Points = orderExternalItemDb.Points;
                        //orderItem.DiscountedPrice = orderExternalItemDb.DiscountPriceValue != null ? (decimal)orderExternalItemDb.DiscountPriceValue : orderExternalItemDb.Price;
                        // orderItem.OriginalPrice = orderExternalItemDb.OriginalPrice;
                        orderItem.ExternalShopId = orderItemShopExternal.Id.ToString();
                        orderItem.PointsRequired = orderExternalItemDb.Points;
                        //if (orderItem.Price != orderItem.DiscountedPrice)
                        //{
                        //    orderItem.DiscountRate = (orderExternalItemDb.OriginalPrice - (decimal)orderItem.DiscountedPrice) / orderExternalItemDb.OriginalPrice * 100;
                        //}


                        var OriginalPrice = (decimal)orderExternalItemDb.OriginalPrice;
                        var DiscountedPrice = orderExternalItemDb.DiscountPriceValue != null ? (decimal)orderExternalItemDb.DiscountPriceValue : orderExternalItemDb.Price;
                        var FinaPrice = (decimal)orderExternalItemDb.Price;


                        if (OriginalPrice.Equals(DiscountedPrice))
                        {

                            orderItem.DashedPrice = (decimal)OriginalPrice;
                            orderItem.FianlPrice = (decimal)FinaPrice;
                        }

                        else if (DiscountedPrice.Equals(FinaPrice))
                        {

                            orderItem.DashedPrice = (decimal)OriginalPrice;
                            orderItem.FianlPrice = (decimal)FinaPrice;

                        }

                        else
                        {
                            orderItem.DashedPrice = (decimal)DiscountedPrice;
                            orderItem.FianlPrice = (decimal)FinaPrice;

                        }




                        orderItem.Quantity = orderExternalItemDb.Quantity;
                        orderItem.ProductTitle = orderExternalItemDb.ProductTitle;
                        orderItem.ProductImageFolderUrl = orderExternalItemDb.ProductCartPreviewSmallImageURL;
                        orderItem.Status = orderExternalItemDb.OrderItemExternalStatus;
                        orderItem.IsVariationProduct = orderExternalItemDb.IsVariationProduct;
                        orderItem.ExpirationTypeId = orderExternalItemDb.DealExpirationId;
                        orderItem.VariationText = orderExternalItemDb.VariationText;
                        orderItem.IsRedeemed = false;
                        orderItem.IsReviewed = false;
                        orderItem.CreatedAt = created;
                        orderItem.ShortId = sortId;
                        orderItem.OrderStatus = orderStatus;
                        
                        //Mapping Order external details (Based on OrderHistoryListQuery)
                        //External Products

                        orderItem.ProductExternalId = orderExternalItemDb.ExternalItemId;
                        orderItem.MerchantExternalId = orderItemShopExternal.ExternalShopId;
                        orderItem.MerchantExternalTypeId = orderItemShopExternal.ExternalTypeId;
                        orderItem.MerchantDisplayName = orderItemShopExternal.ExternalShopName;



                        result.Add(orderItem);
                    }
                }
            }

            return result;

        }

        private List<int> GenerateOrderStatus(int orderStatus)
        {
            List<int> listOrderStatus = new List<int>();
            switch (orderStatus)
            {
                // To Pay Status
                case 1:
                    listOrderStatus.Add((int)OrderStatus.Pending);
                    listOrderStatus.Add((int)OrderStatus.PendingPayment);
                    break;
                // To Receive
                case 2:
                    listOrderStatus.Add((int)OrderStatus.Sent);
                    listOrderStatus.Add((int)OrderStatus.RefundInProgress);
                    break;
                // Completed
                case 3:
                    listOrderStatus.Add((int)OrderStatus.Done);
                    listOrderStatus.Add((int)OrderStatus.Completed);
                    break;
                // Cancelled
                case 4:
                    listOrderStatus.Add((int)OrderStatus.Refunded);
                    listOrderStatus.Add((int)OrderStatus.PendingRefund);
                    listOrderStatus.Add((int)OrderStatus.RefundRejected);
                    break;
            }
            return listOrderStatus;
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

        private enum OrderStatus : byte
        {
            [Description("Pending")]
            Pending = 1,
            [Description("Sent")]
            Sent = 2,
            [Description("Done")]
            Done = 3,
            [Description("Refund in progress")]
            RefundInProgress = 4,
            [Description("Refund")]
            Refunded = 5,
            [Description("Completed")]
            Completed = 6,
            [Description("Pending Payment")]
            PendingPayment = 7,
            [Description("Pending Refund")]
            PendingRefund = 8,
            [Description("Refund Rejected")]
            RefundRejected = 9,

        }
    }





}
