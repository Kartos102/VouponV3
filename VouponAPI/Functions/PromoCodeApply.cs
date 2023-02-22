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
    public class PromoCodeApply
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private int _masterMemberProfileId;
        private string _username;
        private string _errorMessage;
        private PromoCodes _promo { get; set; }
        public PromoCodeApply(RewardsDBContext rewardsDBContext)
        {
            this._rewardsDBContext = rewardsDBContext;
        }

        [OpenApiOperation(operationId: "Promo Code Apply", tags: new[] { "PromoCode" }, Description = "Promo code apply", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(PromoCodeApplyRequestModel), Description = "JSON request body ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PromoCodeApplyResponseModel), Summary = "Logged in user data and JWT")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to authenticate")]
        [FunctionName("PromoCodeApply")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "promocode/apply")] HttpRequest req, ILogger log)
        {
            PromoCodeApplyRequestModel request = HttpRequestHelper.DeserializeModel<PromoCodeApplyRequestModel>(req);

            var response = new PromoCodeApplyResponseModel();
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.Code = -1;
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }

            _masterMemberProfileId = auth.MasterMemberProfileId;
            _username = auth.Email;

            _errorMessage = await ValidatePromoCode(request.PromoCode);
            if (!string.IsNullOrEmpty(_errorMessage)) {
                response.Code = -1;
                response.ErrorMessage = _errorMessage;
                return new BadRequestObjectResult(response);
            }

            var order = _rewardsDBContext.Orders.AsTracking().Where(x => x.Id == request.OrderId && x.MasterMemberProfileId == _masterMemberProfileId).FirstOrDefault();
            _errorMessage = ValidateOrder(order);
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                response.Code = -1;
                response.ErrorMessage = _errorMessage;
                return new BadRequestObjectResult(response);
            }

            //  Check items 

            decimal discountWithPromoCode = 0;
            if (_promo != null)
            {
                if (_promo.PromoCode == "2XPROMO")
                {
                    foreach (var shopExternal in order.OrderShopExternal)
                    {
                        foreach (var externalitem in shopExternal.OrderItemExternal)
                        {
                            if (externalitem.DiscountedAmount != 0)
                            {
                                decimal newMultiplier = 0;
                                decimal discountPc = 0;

                                newMultiplier = Math.Round(_promo.DiscountValue * externalitem.DiscountedAmount * 100) / externalitem.OriginalPrice;
                                discountPc = externalitem.DiscountedAmount;


                                if (newMultiplier > _promo.MaxDiscountValue)
                                {
                                    continue;
                                }
                                else if (newMultiplier <= _promo.MaxDiscountValue)
                                {
                                    discountWithPromoCode += discountPc;
                                }
                            }
                        }
                    }

                    foreach (var orderItem in order.OrderItems)
                    {
                        if (orderItem.DiscountedAmount != 0)
                        {
                            decimal newMultiplier = 0;
                            decimal discountPc = 0;

                            newMultiplier = Math.Round((_promo.DiscountValue * (orderItem.SubtotalPrice - orderItem.Price) * 100) / orderItem.SubtotalPrice);
                            discountPc = orderItem.SubtotalPrice - orderItem.Price;


                            if (newMultiplier > _promo.MaxDiscountValue)
                            {
                                continue;
                            }
                            else if (newMultiplier <= _promo.MaxDiscountValue)
                            {
                                discountWithPromoCode += discountPc;//price
                            }
                        }
                    }
                }
                else
                {
                    discountWithPromoCode = (order.TotalPrice * _promo.DiscountValue) / 100;
                    if (discountWithPromoCode > _promo.MaxDiscountValue)
                    {
                        discountWithPromoCode = _promo.MaxDiscountValue;
                    }
                }
                order.PromoCodeId = _promo.Id;
                order.PromoCodeValue = _promo.PromoCode;
                order.PromoCodeExpireOn = _promo.ExpireOn;
                order.PromoCodeMinSpend = _promo.MinSpend;
                order.PromoCodeMaxDiscountValue = _promo.MaxDiscountValue;
                order.PromoCodeDiscountType = _promo.DiscountType;
                order.PromoCodeDiscountValue = discountWithPromoCode;

                _promo.TotalRedeemed += 1;
                _rewardsDBContext.PromoCodes.Update(_promo);
            }
            var priceAfterDiscount = order.TotalPrice - discountWithPromoCode;
            order.TotalPromoCodeDiscount = discountWithPromoCode;
            order.TotalPrice = priceAfterDiscount;

            await _rewardsDBContext.SaveChangesAsync();

            response.Data = new ApplyPromoCode();
            response.Data.Discount = discountWithPromoCode;
            response.Data.Message = $"\"{request.PromoCode}\" has been applied and you've got RM{discountWithPromoCode.ToString("0.00")} discount";

            return new OkObjectResult(response);
        }

        private string ValidateOrder(Orders order) 
        {
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

           
            foreach (var item in order.OrderItems)
            {
                if (item.ProductId != 0)
                {
                    if (voucherItemId.Contains(item.ProductId))
                    {
                        return "Promo code cannot be use with cash voucher items";
                    }
                }
            }

            if (order.TotalPrice <= _promo.MinSpend)
            {
                return $"The mininum spending required for this promo code is RM{_promo.MinSpend}";
            }

            if (_promo.TotalRedeemed >= _promo.TotalRedemptionAllowed)
            {
                return $"The promo code is no longer available";
            }

            if (order.OrderStatus != 1)
            {
                return $"Order is already paid";
            }
            return null;
        }
        private async Task<string> ValidatePromoCode(string promoCode)
        {
            if (!string.IsNullOrEmpty(promoCode))
            {
                if (string.IsNullOrEmpty(promoCode))
                {
                    return "Promo code is missing";

                }

                _promo = await _rewardsDBContext.PromoCodes.Where(x => x.PromoCode == promoCode && x.Status == 1).FirstOrDefaultAsync();
                if (_promo == null)
                {
                    return "Incorrect or invalid promo code";
                }

                //  Check if user qualified if its selected users only
                if (_promo.IsSelectedUserOnly)
                {
                    var promoCodeSelectedUsers = await _rewardsDBContext.PromoCodeSelectedUsers.Where(x => x.PromoCodeId == _promo.Id && x.Email == _username).FirstOrDefaultAsync();
                    if (promoCodeSelectedUsers == null)
                    {
                        return "Incorrect or invalid promo code";
                    }
                }

                var orders = await _rewardsDBContext.Orders.AsTracking().Where(x => x.MasterMemberProfileId == _masterMemberProfileId && x.OrderStatus == 2).ToListAsync();
                if (orders.Where(x => x.PromoCodeId == _promo.Id).Count() >= _promo.TotalAllowedPerUser)
                {
                    return "You have reach the promo code allowed for this code or you are not qualified to use this promo code";
                }

            }

            return null;
        }


        private class PromoCodeApplyRequestModel {
            public string PromoCode { get; set; }
            public Guid OrderId { get; set; }
        }

        private class PromoCodeApplyResponseModel : ApiResponseViewModel { 
            public ApplyPromoCode Data { get; set; } 
        }

        private class ApplyPromoCode {
            public decimal Discount;
            public string Message;

        }
    }
    
}
