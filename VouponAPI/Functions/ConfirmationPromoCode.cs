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
    public class ConfirmationPromoCode
    {
        private readonly RewardsDBContext rewardsDBContext;
        private int _masterMemberProfileId;
        private string _username;
        public ConfirmationPromoCode(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        [OpenApiOperation(operationId:"Promo Code Discount", tags: new[] { "PromoCode" }, Description = "Promo Code Dicount", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(ConfirmationPromoCodeRequestModel), Description = "JSON request body ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ConfirmationPromoCodeResponseModel), Summary = "Logged in user data and JWT")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to authenticate")]
        [FunctionName("ConfirmationPromoCode")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "promocode/discount")] HttpRequest req, ILogger log) 
        {
            ConfirmationPromoCodeResponseModel response = new ConfirmationPromoCodeResponseModel() 
            { 
                Data = new PromoCodeData()
            };

            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            _masterMemberProfileId = auth.MasterMemberProfileId;
            _username = auth.Email;
            try
            {

                var request = HttpRequestHelper.DeserializeModel<ConfirmationPromoCodeRequestModel>(req);
                var promo = await rewardsDBContext.PromoCodes.Where(x => x.PromoCode == request.PromoCode && x.Status == 1).FirstOrDefaultAsync();
                var err = await Validate(request, promo);
                if (!string.IsNullOrEmpty(err)) 
                {
                    response.Code = -1;
                    response.ErrorMessage = err;
                    return new BadRequestObjectResult(response);
                }
                

                

                decimal discountWithPromoCode = 0;

                if (request.PromoCode.ToUpper() == "2XPROMO" || request.PromoCode.ToUpper() == "EVERYWEDNESDAY2XDISCOUNT")
                {
                    foreach (var product in request.Products)
                    {
                        if (product.AdditionalDiscount != null)
                        {
                            decimal newMultiplier = 0;
                            decimal discountPc = 0;
                            if (product.Id == 0)
                            {
                                newMultiplier = promo.DiscountValue * product.AdditionalDiscount.ExternalItemDiscountPercentage;
                                discountPc = product.AdditionalDiscount.ExternalItemDiscountPercentage;
                            }
                            else
                            {
                                newMultiplier = (decimal)(promo.DiscountValue * Convert.ToInt32(product.AdditionalDiscount.Value));
                                discountPc = Convert.ToInt32(product.AdditionalDiscount.Value);
                            }

                            if (newMultiplier > promo.MaxDiscountValue)
                            {
                                continue;
                            }
                            else if (newMultiplier <= promo.MaxDiscountValue)
                            {
                                discountWithPromoCode += Convert.ToDecimal(product.SubTotal) * discountPc / 100;
                            }
                        }

                    }
                }
                else
                {
                    if (promo.TotalRedeemed >= promo.TotalRedemptionAllowed)
                    {
                        response.Code = -1;
                        response.Data.Message = $"The promo code \"{request.PromoCode}\" is no longer available";
                        return new BadRequestObjectResult(response);
                    }

                    discountWithPromoCode = (request.SubTotal * promo.DiscountValue) / 100;
                    if (discountWithPromoCode > promo.MaxDiscountValue)
                    {
                        discountWithPromoCode = promo.MaxDiscountValue;
                    }


                }


                response.Code = 0;
                response.Data.Message = $"\"{request.PromoCode}\" has been applied and you've got RM{discountWithPromoCode.ToString("0.00")} discount";
                response.Data.DiscountPromoCode = discountWithPromoCode.ToString("0.00");
                response.Data.IsSuccessful = true;
                return new OkObjectResult(response); ;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Data.Message = "We are busy at the moment. Please try again later";
                return new BadRequestObjectResult (response);
            }
        }


        private async Task<string> Validate(ConfirmationPromoCodeRequestModel request, PromoCodes promo) 
        {

            if (string.IsNullOrEmpty(request.PromoCode))
            {
               return "Promo Code is required!";

            }

            request.PromoCode = request.PromoCode.ToUpper();
            if (promo == null)
            {
                return "Incorrect or invalid promo code";

            }

            //  Check if user qualified if its selected users only
            if (promo.IsSelectedUserOnly)
            {
                var promoCodeSelectedUsers = await rewardsDBContext.PromoCodeSelectedUsers.Where(x => x.PromoCodeId == promo.Id && x.Email == _username).FirstOrDefaultAsync();
                if (promoCodeSelectedUsers == null)
                {
                    return "Incorrect or invalid promo code";

                }
            }

            var orders = await rewardsDBContext.Orders.AsTracking().Where(x => x.Email == _username && x.OrderStatus == 2).ToListAsync();
            if (orders.Where(x => x.PromoCodeId == promo.Id).Count() >= promo.TotalAllowedPerUser)
            {

                return  "You have reach the promo code allowed for this code or you are not qualified to use this promo code"; ;
            }


            var voucherItemId = new List<long>();

            if (Environment.GetEnvironmentVariable(EnvironmentKey.ENV) == "LIVE")
            {
                //  LIVE
                voucherItemId.Add(126);
                voucherItemId.Add(128);
                voucherItemId.Add(129);
                voucherItemId.Add(130);
                voucherItemId.Add(131);
                voucherItemId.Add(132);
                voucherItemId.Add(133);
                voucherItemId.Add(134);
                voucherItemId.Add(135);
                voucherItemId.Add(136);
                voucherItemId.Add(137);
                voucherItemId.Add(138);
                voucherItemId.Add(139);
                voucherItemId.Add(140);
                voucherItemId.Add(142);
                voucherItemId.Add(143);
                voucherItemId.Add(203);
                voucherItemId.Add(557);
            }
            else
            {
                //  UAT
                voucherItemId.Add(76);
                voucherItemId.Add(77);
                voucherItemId.Add(78);
                voucherItemId.Add(84);
                voucherItemId.Add(108);
                voucherItemId.Add(113);
                voucherItemId.Add(124);
            }

            foreach (var item in request.Products)
            {
                if (item.ProductId != 0)
                {
                    if (voucherItemId.Contains(item.ProductId))
                    {
                        return $"Promo code \"{request.PromoCode}\" cannot be use with cash voucher items";
                    }
                }
            }


            if (request.SubTotal <= promo.MinSpend)
            {
                return $"The mininum spending required for this promo code \"{request.PromoCode}\" is RM{promo.MinSpend}";
            }

            return null;
        }
        private class ConfirmationPromoCodeResponseModel : ApiResponseViewModel
        {
            public PromoCodeData Data { get; set; }
        }

        private class PromoCodeData
        {
            public string DiscountPromoCode { get; set; }
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }
        private class ConfirmationPromoCodeRequestModel
        {
            public decimal SubTotal { get; set; }
            public string PromoCode { get; set; }

            [JsonProperty("products")]
            public List<ProductList> Products { get; set; }
        }

        private partial class ProductList
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("productId")]
            public long ProductId { get; set; }

            [JsonProperty("additionalDiscount")]
            public AdditionalDiscount AdditionalDiscount { get; set; }

            [JsonProperty("subTotal")]
            public decimal SubTotal { get; set; }

        }

        private partial class AdditionalDiscount
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


    }
}
