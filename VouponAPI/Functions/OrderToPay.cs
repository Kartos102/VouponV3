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
    public class OrderToPay
    {
        RewardsDBContext rewardsDBContext;
        private int _masterMemberProfileId;
        private readonly IAzureBlobStorage azureBlobStorage;
        public OrderToPay(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.azureBlobStorage = azureBlobStorage;
        }
        [OpenApiOperation(operationId: "Get Order to pay", tags: new[] { "order" }, Description = "GetGet Order to pay", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = true, Type = typeof(Guid), Description = "order id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(OrderToPayResponseModel), Summary = "the result is order detail")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no order id is supplied")]
        [FunctionName("GetToPayOrder")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "order/to-pay")] HttpRequest req, ILogger log)
        {
            var response = new OrderToPayResponseModel
            {
                Data = new OrderToPayData()
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

            var order = await rewardsDBContext.Orders
                .Include(x => x.OrderItems)
                .Include(x => x.OrderShopExternal)
                .ThenInclude(x => x.OrderItemExternal)
                .Where(x=> x.MasterMemberProfileId == _masterMemberProfileId && x.OrderStatus == 1 && x.Id == orderId)
                .FirstOrDefaultAsync();
            if (order != null)
            {

                OrderDetail toPay = new OrderDetail();
                toPay.TotalPrice = order.TotalPrice;
                toPay.TotalPoint = order.TotalPoints;
                toPay.OrderedAt = order.CreatedAt;
                toPay.ShippingDetail = GenerateShipping(order);
                List<OrderItemModel> items = await GenerateItem(order.OrderItems.ToList());
                foreach (var ext in order.OrderShopExternal)
                {
                    var external = GenerateExternalItem(ext.OrderItemExternal.ToList());
                    items.AddRange(external);
                }
                toPay.Items = items.OrderBy(x => x.ProductTitle).ToList();

                response.Data.OrderDetail = toPay;
                response.Code = 0;

                return new OkObjectResult(response);
            }
            else {
                return new NotFoundObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "Order Not Found"
                });
            }

        }


        private async Task<List<OrderItemModel>> GenerateItem(List<OrderItems> items) {
            List<OrderItemModel> result = new List<OrderItemModel>();
            foreach (var item in items)
            {
                OrderItemModel prod = new OrderItemModel();
                prod.ProductTitle = item.ProductTitle;
                prod.Id = item.Id;
                prod.ProductCartPreviewSmallImageURL = await GetBlobImage(item.ProductId);
                prod.Price = item.Price;
                prod.ShortId = item.ShortId;
                if (!String.IsNullOrEmpty(item.ProductDetail))
                {
                    ProductList product = JsonConvert.DeserializeObject<ProductList>(item.ProductDetail);
                    prod.Quantity = (int)product.OrderQuantity;
                    prod.OriginalPrice = (decimal)product.Price;
                    prod.DiscountPriceValue = (decimal)product.DiscountedPrice;
                }
                result.Add(prod);
            }
            return result;
        }

        private  List<OrderItemModel> GenerateExternalItem(List<OrderItemExternal> items) {
            List<OrderItemModel> result = new List<OrderItemModel>();
            foreach (var item in items)
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

                result.Add(externalItem);

            }
            return result;

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

        private ShippingDetail GenerateShipping(Orders order) {
            var shipping = new ShippingDetail();
            shipping.Name = order.ShippingPersonFirstName + " " + order.ShippingPersonLastName;
            shipping.PhoneNumber = "(+" + order.ShippingMobileCountryCode + ") " + order.ShippingMobileNumber;
            shipping.Address = order.ShippingAddressLine1;
            if (order.ShippingAddressLine2 != "" && order.ShippingAddressLine2 != null)
            {
                shipping.Address += ", " + order.ShippingAddressLine2 + ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry;
            }
            else
            {
                shipping.Address += ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry;
            }

            return shipping;
        }

        private class OrderToPayResponseModel : ApiResponseViewModel
        {
            public OrderToPayData Data { get; set; }
        }

        private class OrderToPayData
        {
            public OrderDetail OrderDetail { get; set; }
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }
        private class OrderDetail
        {

            public List<OrderItemModel> Items { get; set; }

            public DateTime OrderedAt { get; set; }
            public decimal TotalPrice { get; set; }

            public decimal TotalPoint { get; set; }
            //public string Courier { get; set; }

            public ShippingDetail ShippingDetail { get; set; }

            //public MerchantOrder MerchantOrder { get; set; }

            //public ExternalOrder ExternalOrders { get; set; }
            //public MerchantDetails MerchantInfo { get; set; }

        }
        private class ShippingDetail
        {
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string Address { get; set; }

        }
        public class OrderItemModel
        {
            public Guid Id { get; set; }
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

        private class MerchantDetails
        {
            public int MerchantId { get; set; }

            public string MerchantExternalId { get; set; }
            public string Name { get; set; }

            public string ImageUrl { get; set; }
            public string Email { get; set; }

            public byte MerchantExternalTypeId { get; set; }
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

    }
}
