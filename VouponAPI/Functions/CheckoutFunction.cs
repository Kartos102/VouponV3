using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;

using Voupon.API.Util;
using Voupon.API.ViewModels;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IO;

namespace Voupon.API.Functions
{
    public class CheckoutFunction
    {
        private readonly VodusV2Context _vodusV2Context;
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly IAzureBlobStorage _azureBlobStorage;

        private int _masterMemberProfileId;

        private int _multiTireVpointThreshold = 3;
        private string _errorMessage { get; set; }

        private string _successMessage { get; set; }
        private bool _isProductDataValid { get; set; }
        public string _email { get; set; }

        private string _orderTable { get; set; }

        private string _aggregatorUrl { get; set; }
        private AppConfig _appConfig { get; set; }

        private Database.Postgres.RewardsEntities.PromoCodes _promo { get; set; }
        public CheckoutFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IAzureBlobStorage azureBlobStorage)
        {
            this._rewardsDBContext = rewardsDBContext;
            this._vodusV2Context = vodusV2Context;
            this._azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Checkout cart products", tags: new[] { "Cart" }, Description = "Checkout cart products", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CreateOrderResponseViewModel), Summary = "Get cart products")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to checkout")]
        [OpenApiRequestBody("application/json", typeof(CheckoutRequestModel), Description = "JSON request body ")]

        [FunctionName("CheckoutFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cart/checkout")] HttpRequest req, ILogger log) {


            var response = new CreateOrderResponseViewModel();


            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }


            CheckoutRequestModel request = HttpRequestHelper.DeserializeModel<CheckoutRequestModel>(req);
            _masterMemberProfileId = auth.MasterMemberProfileId;
            request.MasterMemberProfileId = auth.MasterMemberProfileId;
            request.Email = auth.Email;

            _errorMessage = await SetConfig();
            if (_errorMessage != "") {

                response.Code = -1;
                response.ErrorMessage = _errorMessage;
            }

            var logData = new LogData
            {
                Request = request
            };



            _rewardsDBContext.Database.BeginTransaction();
            _vodusV2Context.Database.BeginTransaction();
            _isProductDataValid = true;
            var aggregatorUrl = await _vodusV2Context.AggregatorApiUrls.ToListAsync();
            _aggregatorUrl = aggregatorUrl.OrderBy(x => x.LastUpdatedAt).First().Url;

            var masterMemberProfile = await _vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.Id == request.MasterMemberProfileId).FirstOrDefaultAsync();

            if (masterMemberProfile == null)
            {
                var activity = new LogActivity
                {
                    ActionName = "Checkout API",
                    ActionFunction = "Retrieving master profile",
                    ActionData = JsonConvert.SerializeObject(request),
                    ActionId = new Guid().ToString(),
                    TriggerBy = request.Email,
                    Message = "Invalid master profile",
                    TriggerFor = request.Email,
                    IsSuccessful = false

                };

                await Logger(activity);

                response.Code = -1;
                response.ErrorMessage = "Invalid request [001]";
                return new BadRequestObjectResult(response);

            }
            var user = masterMemberProfile.User;

            if (user == null)
            {
                var activity = new LogActivity
                {
                    ActionName = "Checkout API",
                    ActionFunction = "Retrieving user data",
                    ActionData = JsonConvert.SerializeObject(request),
                    ActionId = new Guid().ToString(),
                    TriggerBy = request.Email,
                    Message = "request.UserName return null",
                    TriggerFor = request.Email,
                    IsSuccessful = false

                };
                await Logger(activity);

                response.Code = -1;
                response.ErrorMessage = "Invalid request [002]";
                return new BadRequestObjectResult(response);
            }

            _email = user.Email;
            request.Email = user.Email;
            request.BillingEmail = user.Email;
            request.ShippingEmail = user.Email;

            var pendingOrders = await _rewardsDBContext.Orders.Where(x => x.MasterMemberProfileId == masterMemberProfile.Id && x.OrderStatus == 1).ToListAsync();
            var pendingPointsToBeDeducted = pendingOrders.Sum(x => x.TotalPoints);

            var availablePoints = masterMemberProfile.AvailablePoints - pendingPointsToBeDeducted;

            logData.MasterMemberProfileId = masterMemberProfile.Id;
            logData.Email = user.Email;
            logData.BeforeOrderPoints = masterMemberProfile.AvailablePoints;
            logData.PendingOrderToBeDeductedPoints = pendingPointsToBeDeducted;

            var orderId = Guid.NewGuid();

            _appConfig = await _rewardsDBContext.AppConfig.FirstOrDefaultAsync();

            //  Generate ShortId from GUID
            var shortId = DateTime.Now.ToString("dd") + orderId.ToString().Split("-")[0];

            var masterMemberProfilePoints = 0;

            var order = new Database.Postgres.RewardsEntities.Orders();

            try
            {
                order = new Database.Postgres.RewardsEntities.Orders
                {
                    Id = orderId,
                    ShortId = shortId,
                    MasterMemberProfileId = masterMemberProfile.Id,
                    Email = request.Email,
                    OrderStatus = 1,
                    TotalItems = (int)request.ProductList.Sum(x => x.OrderQuantity),
                    TotalPoints = (int)request.ProductList.Where(x => x.AdditionalDiscount != null).Sum(x => x.AdditionalDiscount.PointsRequired),
                    TotalPrice = (decimal)request.ProductList.Sum(x => decimal.Parse(x.TotalPrice)),
                    BillingAddressLine1 = request.BillingAddressLine1,
                    BillingAddressLine2 = request.BillingAddressLine2,
                    BillingCity = request.BillingCity,
                    BillingPostcode = request.BillingPostcode,
                    BillingState = request.BillingState,
                    BillingCountry = request.BillingCountry,
                    BillingMobileCountryCode = request.BillingMobileCountryCode,
                    BillingMobileNumber = request.BillingMobileNumber,
                    BillingEmail = request.BillingEmail,
                    BillingPersonFirstName = request.BillingPersonFirstName,
                    BillingPersonLastName = request.BillingPersonLastName,
                    ShippingAddressLine1 = request.BillingAddressLine1,
                    ShippingAddressLine2 = request.BillingAddressLine2,
                    ShippingCity = request.BillingCity,
                    ShippingPostcode = request.BillingPostcode,
                    ShippingState = request.BillingState,
                    ShippingCountry = request.BillingCountry,
                    ShippingMobileCountryCode = request.BillingMobileCountryCode,
                    ShippingMobileNumber = request.BillingMobileNumber,
                    ShippingEmail = request.BillingEmail,
                    ShippingPersonFirstName = request.BillingPersonFirstName,
                    ShippingPersonLastName = request.BillingPersonLastName,
                    CreatedAt = DateTime.Now,
                    TotalShippingCost = request.OrderShippingCost
                };
                var frontEndOrderPoints = order.TotalPoints;
                order.TotalPoints = 0;

                foreach (var item in request.ProductList.Where(x => x.ExternalItemId == null))
                {
                    if (string.IsNullOrEmpty(item.ExternalItemId))
                    {
                        int result = await SetupOrderItems(item, request, order, orderId, _multiTireVpointThreshold);
                        //Calculate the multi quantity points per item capped at 10
                        order.TotalPoints += result;

                    }

                }

                if (request.ProductList.Where(x => x.ExternalItemId != null).Any())
                {
                    var shopList = request.ProductList.Where(x => x.ExternalItemId != null).GroupBy(x => x.ExternalShopId).Select(x => x.Key);
                    if (shopList != null)
                    {
                        foreach (var shopId in shopList)
                        {
                            var orderShopExternal = new OrderShopExternal
                            {
                                Id = Guid.NewGuid(),
                                OrderId = orderId,
                                OrderShippingExternalStatus = 1,
                                OrderItemExternal = new List<OrderItemExternal>()
                            };

                            foreach (var item in request.ProductList.Where(x => x.ExternalShopId == shopId))
                            {
                                //var orderShopExternal = await _rewardsDBContext.OrderShopExternal.Where(x => x.OrderId == orderId && x.ExternalShopId == item.ExternalShopId).FirstOrDefaultAsync();
                                var result = SetupExternalOrderItems(item, request, order, orderShopExternal, _multiTireVpointThreshold);
                                if (result == null)
                                {
                                    break;
                                }
                                orderShopExternal.OrderItemExternal.Add(result);
                            }

                            orderShopExternal.ShippingCost = request.ProductList.Where(x => x.ExternalShopId == shopId).Sum(x => x.ShippingCost);
                            orderShopExternal.ExternalShopId = request.ProductList.Where(x => x.ExternalShopId == shopId).First().ExternalShopId;
                            orderShopExternal.ExternalTypeId = request.ProductList.Where(x => x.ExternalShopId == shopId).First().ExternalTypeId;
                            orderShopExternal.ExternalShopName = request.ProductList.Where(x => x.ExternalShopId == shopId).First().Merchant.Name;
                            orderShopExternal.ExternalShopUrl = request.ProductList.Where(x => x.ExternalShopId == shopId).First().ExternalShopUrl;

                            orderShopExternal.TotalItems = orderShopExternal.OrderItemExternal.Sum(x => x.Quantity);
                            orderShopExternal.TotalPoints = orderShopExternal.OrderItemExternal.Sum(x => x.Points);
                            orderShopExternal.TotalPrice = orderShopExternal.OrderItemExternal.Sum(x => x.TotalPrice);
                            order.OrderShopExternal.Add(orderShopExternal);
                            //_rewardsDBContext.OrderShopExternal.Add(orderShopExternal);
                            //order.TotalPoints += orderShopExternal.TotalPoints;
                            //order.TotalItems += orderShopExternal.TotalItems;
                            //order.TotalPrice += orderShopExternal.TotalPrice;
                        }
                    }
                }

                if (!_isProductDataValid)
                {
                    var activity = new LogActivity
                    {
                        ActionName = "Checkout API",
                        ActionFunction = "Invalid product data",
                        ActionData = JsonConvert.SerializeObject(request),
                        ActionId = new Guid().ToString(),
                        TriggerBy = request.Email,
                        Message = $"!_isProductDataValid {_errorMessage}",
                        TriggerFor = request.Email,
                        IsSuccessful = false

                    };


                    await Logger(activity);

                    if (string.IsNullOrEmpty(_errorMessage))
                    {
                        response.ErrorMessage = "Invalid request";
                    }
                    else
                    {
                        response.ErrorMessage = _errorMessage;
                    }
                    response.Code = -1;
                    return new UnprocessableEntityObjectResult(response);
                }

                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    var activity = new LogActivity
                    {
                        ActionName = "Checkout API",
                        ActionFunction = "Invalid product data",
                        ActionData = JsonConvert.SerializeObject(request),
                        ActionId = new Guid().ToString(),
                        TriggerBy = request.Email,
                        Message = _errorMessage,
                        TriggerFor = request.Email,
                        IsSuccessful = false

                    };
                    await Logger(activity);


                    _rewardsDBContext.Database.RollbackTransaction();
                    _vodusV2Context.Database.RollbackTransaction();

                    response.ErrorMessage = _errorMessage;
                    response.Code = -1;
                    return new UnprocessableEntityObjectResult(response);

                }

                order.TotalItems = order.OrderItems.Count();
                order.ShippingCostSubTotal += order.OrderItems.Sum(x => x.ShippingCost);

                order.TotalPrice = order.OrderItems.Sum(x => x.Price);
                //order.TotalPoints = order.OrderItems.Sum(x => x.Points);

                //  Add external items if any
                if (order.OrderShopExternal != null && order.OrderShopExternal.Any())
                {
                    order.ShippingCostSubTotal += order.OrderShopExternal.Sum(x => x.ShippingCost);
                    order.TotalItems += order.OrderShopExternal.Sum(x => x.TotalItems);
                    order.TotalPrice += order.OrderShopExternal.Sum(x => x.TotalPrice);
                    order.TotalPoints += order.OrderShopExternal.Sum(x => x.TotalPoints);
                }
                order.TotalShippingCost = order.ShippingCostSubTotal;
                //order.TotalPrice += order.TotalShippingCost;

                //  Check if there's enough points if any point is required
                if (order.TotalPoints > 0)
                {
                    if (availablePoints < order.TotalPoints)
                    {
                        if (pendingPointsToBeDeducted > 0)
                        {
                            //apiResponseViewModel.Message = "You don't have enough VPoints to proceed with checkout.<br/> Free up your VPoints by removing your orders that are pending payment. <a href='/Order/Pending'>View My Pending Payment Orders</a>";
                            response.ErrorMessage = "Invalid request [010]";

                        }
                        else
                        {
                            response.ErrorMessage = "You do not have enough VPoints to proceed with checkout.";
                        }
                        //apiResponseViewModel.Message = "You do not have enough VPoints to proceed with checkout. If you have previously added items to cart but did not complete the payment, cancel that order to free up your points.";
                        response.Code = -1;
                        return new UnprocessableEntityObjectResult(response);
                    }
                }

                //  Update status to completed and deduct points
                if (order.TotalPrice == 0 && order.TotalShippingCost == 0)
                {
                    masterMemberProfile.AvailablePoints = masterMemberProfile.AvailablePoints - order.TotalPoints;
                    order.OrderStatus = 2;
                    _successMessage = "Your order have been created and VPoints will be deducted from your account.";

                    _vodusV2Context.MasterMemberProfiles.Update(masterMemberProfile);
                    await _vodusV2Context.SaveChangesAsync();
                }
                else
                {
                    order.OrderStatus = 1;
                    _successMessage = "Your order have been created. Please complete the payment to complete the purchase";
                }

                masterMemberProfilePoints = masterMemberProfile.AvailablePoints;

                logData.TotalUsedPoints = order.TotalPoints;
                logData.AfterOrderPoints = masterMemberProfile.AvailablePoints /*- order.TotalPoints*/;
                logData.TotalPrice = order.TotalPrice;// + order.TotalShippingCost;
                logData.TotalItems = order.TotalItems;
                order.TotalPriceBeforePromoCodeDiscount = order.TotalPrice;
                //  Check Promo Code

                _errorMessage = await ValidatePromoCode(request, order.TotalPrice);

                if (!string.IsNullOrEmpty(_errorMessage)) { 
                
                    response.ErrorMessage = _errorMessage;
                    response.Code = -1;
                    return new BadRequestObjectResult(response);
                }

                decimal discountWithPromoCode = 0;
                if (_promo != null) {
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


                order.Logs = JsonConvert.SerializeObject(logData);
                var orderProductIds = order.OrderItems.Select(x => x.ProductId).ToList();
                var orderProductvariations = order.OrderItems.Select(x => x.VariationId).ToList();
                var orderCartProducts = _rewardsDBContext.CartProducts.Where(x => x.MasterMemberProfileId == order.MasterMemberProfileId && orderProductIds.Contains(x.ProductId)).ToList();
                List<int> execludedIds = new List<int>();
                foreach (var orderCartProduct in orderCartProducts)
                {
                    if (!orderProductvariations.Contains(orderCartProduct.VariationId))
                    {
                        execludedIds.Add(orderCartProduct.Id);
                    }
                }
                orderCartProducts = orderCartProducts.Where(x => !execludedIds.Contains(x.Id)).ToList();
                _rewardsDBContext.Orders.Add(order);
                _rewardsDBContext.CartProducts.RemoveRange(orderCartProducts);

                //  Remove externals
                var externalIdList = request.ProductList.Select(x => x.ExternalId);
                var orderCartExternal = _rewardsDBContext.CartProductExternal.Where(x => x.MasterMemberProfileId == order.MasterMemberProfileId && externalIdList.Contains(x.Id)).ToList();

                _rewardsDBContext.CartProductExternal.RemoveRange(orderCartExternal);

                await _rewardsDBContext.SaveChangesAsync();

                _rewardsDBContext.Database.CommitTransaction();
                _vodusV2Context.Database.CommitTransaction();

                //masterMemberProfile.User.Email = "some test email";

                //  If the items is only consists of points items sent thank you email without needing payment step
                if (order.TotalPrice == 0 && order.TotalShippingCost == 0)
                {
                    //  Generate Receipt
                    var apiKey = Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.API_KEY);
                    var noReply = Environment.GetEnvironmentVariable(EnvironmentKey.EMAIL.FROM.NO_REPLY);
                    var sendGridClient = new SendGridClient(apiKey);
                    var msg = new SendGridMessage();
                    msg.SetTemplateId(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.TEMPLATES.ORDER_CONFIRMATION));
                    msg.SetFrom(new EmailAddress(noReply, "Vodus No-Reply"));
                    msg.SetSubject("Thank you for your order");
                    msg.AddTo(new EmailAddress(masterMemberProfile.User.Email));
                    msg.AddTo("merchant@vodus.my");
                    msg.AddSubstitution("-customerName-", masterMemberProfile.User.FirstName + " " + masterMemberProfile.User.LastName);
                    msg.AddSubstitution("-orderNumber-", "#" + order.ShortId);
                    msg.AddSubstitution("-orderDate-", order.CreatedAt.ToString("dd/MM/yyyy"));
                    msg.AddSubstitution("-orderTable-", _orderTable);
                    var res = sendGridClient.SendEmailAsync(msg).Result;

                    //  Generate Token
                    if (order.OrderItems != null && order.OrderItems.Any())
                    {
                        foreach (var item in order.OrderItems)
                        {
                            var tokenMessage = await CreateToken(item.Id, item.ProductId, order.MasterMemberProfileId, masterMemberProfile.User.Email, masterMemberProfile.User.FirstName + " " + masterMemberProfile.User.LastName);
                            if (tokenMessage != null)
                            {
                                msg = tokenMessage;
                                var resp = sendGridClient.SendEmailAsync(msg).Result;
                            }

                            //  Generate notification email to merchant
                            var merchantEmail = _rewardsDBContext.UserRoles.Include(x => x.User).Where(x => x.MerchantId == item.MerchantId && x.RoleId == new Guid("1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefault().User.Email;
                            var sendGridClientForMerchant = new SendGridClient(apiKey);
                            var msgForMerchant = new SendGridMessage();
                            msgForMerchant.AddTo(merchantEmail);
                            msgForMerchant.SetFrom(new EmailAddress(noReply, "Vodus No-Reply"));
                            msgForMerchant.SetSubject("Vodus - New order");

                            //  In Store
                            if (item.ExpirationTypeId == 2)
                            {
                                msgForMerchant.SetTemplateId(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.TEMPLATES.IN_STORE_MERCHANT_SALES_NOTIFICATION_EMAIL));
                            }
                            // Digital
                            else if (item.ExpirationTypeId == 4)
                            {
                                msgForMerchant.SetTemplateId(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.TEMPLATES.DIGITAL_MERCHANT_SALES_NOTIFICATION_EMAIL));
                            }
                            // Delivery
                            else if (item.ExpirationTypeId == 5)
                            {
                                msgForMerchant.SetTemplateId(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.TEMPLATES.DELIVERY_MERCHANT_SALES_NOTIFICATION_EMAIL));
                                msgForMerchant.AddSubstitution("-name-", masterMemberProfile.User.FirstName + " " + masterMemberProfile.User.LastName);
                                if (order.ShippingAddressLine2 != null || order.ShippingAddressLine2 != "")
                                {
                                    msgForMerchant.AddSubstitution("-address-", order.ShippingAddressLine1 + ", " + order.ShippingAddressLine2 + ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry);
                                }
                                else
                                {
                                    msgForMerchant.AddSubstitution("-address-", order.ShippingAddressLine1 + ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry);
                                }
                                msgForMerchant.AddSubstitution("-phone-", "+(" + order.ShippingMobileCountryCode + ")" + order.ShippingMobileNumber);
                            }
                            msgForMerchant.AddSubstitution("-orderTable-", _orderTable);
                            var merchantEmailResponse = await sendGridClientForMerchant.SendEmailAsync(msgForMerchant);
                            //  Add to activity log

                            var activity = new LogActivity
                            {
                                ActionName = "Checkout API",
                                ActionFunction = "MerchantEmail",
                                ActionData = JsonConvert.SerializeObject(request),
                                ActionId = order.Id.ToString(),
                                TriggerBy = _email,
                                Message = await merchantEmailResponse.Body.ReadAsStringAsync(),
                                TriggerFor = merchantEmail,
                                IsSuccessful = true

                            };
                            await Logger(activity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _vodusV2Context.Database.RollbackTransaction();
                _rewardsDBContext.Database.RollbackTransaction();

                var errorLogs = new Database.Postgres.RewardsEntities.ErrorLogs
                {
                    TypeId = 2,
                    ActionName = "CreateOrderCommand",
                    ActionRequest = JsonConvert.SerializeObject(request),
                    CreatedAt = DateTime.Now,
                    Errors = ex.ToString()
                };

                await _rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                await _rewardsDBContext.SaveChangesAsync();

                response.Code = -1;
                return new UnprocessableEntityObjectResult(response);
            }

            response.Data = new CreateOrderResponse
            {
                OrderId = order.Id,
                MasterMemberPoints = masterMemberProfilePoints,
                RedirectUrl = order.OrderStatus == 2 ? "" : $"{Environment.GetEnvironmentVariable(EnvironmentKey.VOUPON_URL)}/checkout/payment/{order.Id}",
                OrderStatus = order.OrderStatus,
                Message = _successMessage
            };
            response.ErrorMessage = "";
            var sucessLog = new LogActivity
            {
                ActionName = "Checkout API",
                ActionFunction = "Order created",
                ActionData = JsonConvert.SerializeObject(request),
                ActionId = order.Id.ToString(),
                TriggerBy = request.Email,
                Message = "Successfully created order",
                TriggerFor = request.Email,
                IsSuccessful = true

            };

            await Logger(sucessLog);

            response.Code = 0;
            return new OkObjectResult(response);
        }


        private async Task<int> SetupOrderItems(ProductList item, CheckoutRequestModel request, Database.Postgres.RewardsEntities.Orders order, Guid orderId, int multiTireVpointThreshold)
        {
            var product = await _rewardsDBContext.Products.Include(x => x.DealExpirations).Include(x => x.ProductDiscounts).Where(z => z.Id == item.ProductId).FirstOrDefaultAsync();

            var productVaruiation = await _rewardsDBContext.ProductVariation.Where(x => x.Id == item.VariationId).FirstOrDefaultAsync();

            var productDiscountList = await _rewardsDBContext.ProductDiscounts.Where(x => x.ProductId == item.ProductId).ToListAsync();

            List<Database.Postgres.RewardsEntities.OrderItems> addedItems = new List<Database.Postgres.RewardsEntities.OrderItems>();

            if (product == null)
            {
                _isProductDataValid = false;
                return 0;
            }

            if (productVaruiation == null && item.IsVariationProduct)
            {
                _isProductDataValid = false;
                return 0;
            }
            if (!item.IsVariationProduct)
            {
                var pendingOrderQuantity = _rewardsDBContext.OrderItems.Where(x => x.ProductId == item.ProductId && x.Order.OrderStatus == 1).Count();
                if (product.AvailableQuantity < (item.OrderQuantity + pendingOrderQuantity))
                {
                    _errorMessage = "Sorry, insufficient available quantity for " + product.Title + ". Only left " + (product.AvailableQuantity - pendingOrderQuantity);
                    _isProductDataValid = false;
                    return 0;
                }
            }
            else
            {
                var pendingOrderQuantity = _rewardsDBContext.OrderItems.Where(x => x.ProductId == item.ProductId && x.VariationId == item.VariationId && x.Order.OrderStatus == 1).Count();
                if (productVaruiation.AvailableQuantity < (item.OrderQuantity + pendingOrderQuantity))
                {
                    _errorMessage = "Sorry, insufficient available quantity for " + product.Title + ". Only left " + (productVaruiation.AvailableQuantity - pendingOrderQuantity);
                    _isProductDataValid = false;
                    return 0;
                }
            }
            if (product.DealExpiration.ExpirationTypeId == 2 && product.DealExpiration.ExpiredDate <= DateTime.Now)
            {
                _errorMessage = "Sorry, The deal for " + product.Title + " has expired. Please remove the item from your cart and checkout again";
                _isProductDataValid = false;
                return 0;
            }

            var imageUrl = "";
            if (!item.IsVariationProduct)
            {
                imageUrl = await GetFromBlob((int)item.ProductId);

                item.Price = (double)product.Price;
                item.DiscountedPrice = (double)product.DiscountedPrice;
            }
            else
            {

                imageUrl = item.ProductCartPreviewSmallImage;
                if (!imageUrl.Contains("https"))
                {
                    imageUrl = imageUrl.Replace("http", "https");
                }

                item.Price = (double)productVaruiation.Price;
                item.DiscountedPrice = (double)productVaruiation.DiscountedPrice;
            }
            if (item.ProductCartPreviewSmallImage != null) {
                if (!item.ProductCartPreviewSmallImage.Contains("https"))
                {
                    item.ProductCartPreviewSmallImage = item.ProductCartPreviewSmallImage.Replace("http", "https");
                }
            }

            if (item.OrderQuantity > 1)
            {
                for (var i = 0; i < item.OrderQuantity; i++)
                {
                    var itemPrice = "";
                    var discountedAmount = "0";
                    var subTotal = "";

                    var id = Guid.NewGuid();
                    var newOrderItem = new Database.Postgres.RewardsEntities.OrderItems
                    {
                        Id = id,
                        Commision = 0,
                        ExpirationTypeId = product.DealExpiration.ExpirationTypeId.Value,
                        ProductImageFolderUrl = imageUrl,
                        ProductTitle = product.Title,
                        Status = 1,
                        MerchantId = (int)item.Merchant.Id,
                        MerchantDisplayName = item.Merchant.Name,
                        OrderId = orderId,
                        ProductId = (int)item.ProductId,
                        Points = item.PointsRequired,
                        ShortId = DateTime.Now.ToString("dd") + id.ToString().Split("-")[0]
                    };
                    if (!item.IsVariationProduct)
                    {
                        newOrderItem.Price = (decimal)product.DiscountedPrice.Value;
                        itemPrice = product.DiscountedPrice.Value.ToString();
                        if (i == 0)
                        {
                            var ordershippingCost = request.OrderShippingCosts.Where(x => x.ProductId == newOrderItem.ProductId).FirstOrDefault();
                            if (ordershippingCost != null)
                            {
                                newOrderItem.ShippingCost = ordershippingCost.OrderShippingCost;
                            }
                            else
                            {
                                newOrderItem.ShippingCost = 0;
                            }
                        }
                        else
                        {
                            newOrderItem.ShippingCost = 0;
                        }
                    }
                    else
                    {
                        newOrderItem.Price = (decimal)productVaruiation.DiscountedPrice.Value;
                        newOrderItem.IsVariationProduct = true;
                        newOrderItem.VariationId = (int)item.VariationId;
                        newOrderItem.VariationText = item.VariationText;
                        itemPrice = productVaruiation.DiscountedPrice.Value.ToString();
                        if (i == 0)
                        {
                            var ordershippingCost = request.OrderShippingCosts.Where(x => x.ProductId == newOrderItem.ProductId && x.VariationId == newOrderItem.VariationId).FirstOrDefault();
                            if (ordershippingCost != null)
                            {
                                newOrderItem.ShippingCost = ordershippingCost.OrderShippingCost;
                            }
                            else
                            {
                                newOrderItem.ShippingCost = 0;
                            }
                        }
                        else
                        {
                            newOrderItem.ShippingCost = 0;
                        }
                    }

                    if (item.AdditionalDiscount != null && item.AdditionalDiscount.Id > 0)
                    {
                        if (productDiscountList == null)
                        {
                            _isProductDataValid = false;
                            return 0;
                        }

                        var productDiscounts = product.ProductDiscounts.Where(x => x.Id == item.AdditionalDiscount.Id).FirstOrDefault();
                        if (productDiscounts == null)
                        {
                            _isProductDataValid = false;
                            return 0;
                        }
                        // If point item then add required points
                        if (item.TypeId == 1)
                        {
                            newOrderItem.Points += productDiscounts.PointRequired;
                        }
                        else if (item.TypeId == 2)
                        {
                            newOrderItem.Points += productDiscounts.PointRequired;

                        }

                        if (productDiscounts.DiscountTypeId == 1 && productDiscounts.PriceValue == 0)
                        {

                            if (_appConfig.IsVPointsMultiplierEnabled)
                            {
                                if (!newOrderItem.ProductTitle.Contains("voucher"))
                                {
                                    if (productDiscounts.PercentageValue > 0)
                                    {
                                        var newMultiplier = _appConfig.VPointsMultiplier * productDiscounts.PercentageValue;
                                        if (newMultiplier > _appConfig.VPointsMultiplierCap)
                                        {
                                            productDiscounts.PercentageValue = _appConfig.VPointsMultiplierCap;

                                        }
                                        else if (newMultiplier <= _appConfig.VPointsMultiplierCap)
                                        {
                                            productDiscounts.PercentageValue = newMultiplier;
                                        }
                                        newOrderItem.VPointsMultiplier = _appConfig.VPointsMultiplier;
                                        newOrderItem.VPointsMultiplierCap = _appConfig.VPointsMultiplierCap;
                                    }
                                }
                            }

                            newOrderItem.Price -= newOrderItem.Price * productDiscounts.PercentageValue / 100;
                            discountedAmount = (newOrderItem.Price * productDiscounts.PercentageValue / 100).ToString();
                        }
                        else
                        {
                            newOrderItem.Price -= productDiscounts.PriceValue;
                            discountedAmount = productDiscounts.PriceValue.ToString();
                        }

                        subTotal = newOrderItem.Price.ToString();
                    }
                    newOrderItem.DiscountedAmount = Convert.ToDecimal(discountedAmount);
                    newOrderItem.SubtotalPrice = Convert.ToDecimal(itemPrice);
                    newOrderItem.ProductDetail = JsonConvert.SerializeObject(item);
                    order.TotalPrice = order.OrderItems.Sum(x => x.Price) + order.TotalShippingCost;
                    //order.TotalPoints = order.OrderItems.Sum(x => x.Points);
                    addedItems.Add(newOrderItem);
                    order.OrderItems.Add(newOrderItem);

                    var address = order.ShippingAddressLine1 + ", ";
                    if (order.ShippingAddressLine2 != null && order.ShippingAddressLine2 != "")
                        address += order.ShippingAddressLine2 + ", ";
                    address += order.ShippingPostcode + " ";
                    address += order.ShippingCity + " ";
                    address += order.ShippingState + ", ";
                    address += order.ShippingCountry;

                    _orderTable += $"<table border='0' cellpadding='0' cellspacing='0' width='100%' class='x_478386741tableButton'>" +
                             "<tbody style = 'font-family:Calibri'>" +
                             "<tr>" +
                             "<td align='center' valign='top' style='padding-top:20px; padding-bottom:20px; width:100%;'>" +
                             "<table style = 'width:100%; max-width :  800px; '>" +
                             "<tbody>" +
                       "<tr style = 'background-color:#eee;font-weight:bold;'>" +
                          "<td style = 'padding: 0 15px'> Item </td>" +
                           "<td></td>" +
                           "<td style = 'padding:5px;text-align:center;'> Qty </td>" +
                            "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;'> Price </td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td width = '15%'>" +
                            $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductCartPreviewSmallImage}'>" + "</td>" +
                                "<td style = 'padding:5px;font-size:12px;'>" +
                                $"<a style = 'font-size:14px;color:#666;text-decoration:none;' href ='https://vodus.my/Order/History' target = '_blank'>" +
                                        $"{item.Title}" +
                                "</a>" +
                            "</td> " +
                            "<td style = 'padding:5px;text-align:center;'>1</td>" +
                            "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;color:#8C37F6;font-weight:bold;'>RM " +
                                    newOrderItem.SubtotalPrice.ToString("F") +
                            "</td>" +
                            "</tr>" +
                        "</tbody> " +
                    "</table>" +
                    "<table style='border:1px #ddd solid;border-left:0;border-right:0;'>" +
                                    "<tbody>" +
                                      "<tr>" +
                                    "<td width='50%' style='padding:15px;padding-right:0;'> " +
                                        "<p style = 'color:#000;font-size:16px;font-weight:bold;text-align:left;margin:0'> Shipping Address </p>" +
                                        "<p style = 'text-align:left;font-size:13px;margin:5px 0'>" +
                                            $"{address}" +
                                        "</p>" +
                                    "</td>" +
                                    "<td width='50%' style='min-width:150px;padding:15px;padding-left:0;'>" +
                                    "<table style='width:100%;'> " +
                                       "<tbody style='padding:0 15px;font-size:16px;'>  " +
                                           "<tr>" +
                                               "<td width='50%' style='text-align: right;'> VPoints:</td> " +
                                              $"<td width='50%' style='text-align: right;'>{newOrderItem.Points} </td> " +
                                           "</tr>  " +
                                           "<tr>  " +
                                               "<td width='50%' style='text-align:right;'> Subtotal: </td> " +
                                               $"<td width='50%' style='text-align:right;'> RM {newOrderItem.SubtotalPrice.ToString("F")}</td>" +
                                           "</tr> " +
                                           "<tr style='width: 100%; vertical-align: top; '> " +
                                               "<td width = '50%' style='text-align:right;'> Discount:</td>" +
                                               $"<td width = '50%' style='text-align:right;'> -RM {newOrderItem.DiscountedAmount.ToString("F")}</td>" +
                                           "</tr>  " +
                                               "<tr style='width: 100%; vertical-align: top; '> " +
                                               "<td width = '50%' style='text-align:right;'> Shipping Cost:</td>" +
                                               $"<td width = '50%' style='text-align:right;'> RM {newOrderItem.ShippingCost.ToString("F")}</td>" +
                                           "</tr>  " +
                                           "<tr style='vertical-align:top;font-weight:bold;'> " +
                                               "<td width='50%' style='text-align:right;'> Total:</td> " +
                                                $"<td width='50%' style='text-align:right;color:#8C37F6;'> RM {(newOrderItem.Price + newOrderItem.ShippingCost).ToString("F")} </td>  " +
                                           "</tr> " +
                                       "</tbody> " +
                                   " </table> " +
                                   " </td> " +
                                   " </tr> " +
                                   " </tbody> " +
                                "</table> " +
                            "</td> " +
                        "</tr> " +
                    "</tbody> " +
                "</table> ";

                }
            }
            else
            {
                var itemPrice = "";
                var discountedAmount = "0";
                var subTotal = "";

                var id = Guid.NewGuid();
                var newOrderItem = new Database.Postgres.RewardsEntities.OrderItems
                {
                    Id = id,
                    Commision = 0,
                    ExpirationTypeId = product.DealExpiration.ExpirationTypeId.Value,
                    ProductImageFolderUrl = imageUrl,
                    ProductTitle = item.Title,
                    Status = 1,
                    MerchantId = (int)item.Merchant.Id,
                    MerchantDisplayName = item.Merchant.Name,
                    OrderId = orderId,
                    ProductId = (int)item.ProductId,
                    Points = item.PointsRequired,
                    ShortId = DateTime.Now.ToString("dd") + id.ToString().Split("-")[0]
                };

                if (!item.IsVariationProduct)
                {
                    newOrderItem.Price = (decimal)product.DiscountedPrice.Value;
                    itemPrice = product.DiscountedPrice.Value.ToString();
                    var ordershippingCost = request.OrderShippingCosts.Where(x => x.ProductId == newOrderItem.ProductId).FirstOrDefault();
                    if (ordershippingCost != null)
                    {
                        newOrderItem.ShippingCost = ordershippingCost.OrderShippingCost;
                    }
                    else
                    {
                        newOrderItem.ShippingCost = 0;
                    }
                }
                else
                {
                    newOrderItem.Price = (decimal)productVaruiation.DiscountedPrice.Value;
                    newOrderItem.IsVariationProduct = true;
                    newOrderItem.VariationId = (int)item.VariationId;
                    newOrderItem.VariationText = item.VariationText;
                    itemPrice = productVaruiation.DiscountedPrice.Value.ToString();

                    if (newOrderItem.VariationId.HasValue && newOrderItem.VariationId == 0)
                    {
                        var ordershippingCost = request.OrderShippingCosts.Where(x => x.ProductId == newOrderItem.ProductId).FirstOrDefault();
                        if (ordershippingCost != null)
                        {
                            newOrderItem.ShippingCost = ordershippingCost.OrderShippingCost;
                        }
                        else
                        {
                            newOrderItem.ShippingCost = 0;
                        }
                    }
                    else
                    {
                        var ordershippingCost = request.OrderShippingCosts.Where(x => x.ProductId == newOrderItem.ProductId && x.VariationId == newOrderItem.VariationId).FirstOrDefault();
                        if (ordershippingCost != null)
                        {
                            newOrderItem.ShippingCost = ordershippingCost.OrderShippingCost;
                        }
                        else
                        {
                            newOrderItem.ShippingCost = 0;
                        }
                    }

                }

                if (item.AdditionalDiscount != null && item.AdditionalDiscount.Id > 0)
                {
                    if (productDiscountList == null)
                    {
                        _isProductDataValid = false;
                        return 0;
                    }

                    var productDiscounts = product.ProductDiscounts.Where(x => x.Id == item.AdditionalDiscount.Id).FirstOrDefault();
                    if (productDiscounts == null)
                    {
                        _isProductDataValid = false;
                        return 0;
                    }
                    // If point item then add required points
                    if (item.TypeId == 1)
                    {
                        newOrderItem.Points += productDiscounts.PointRequired;
                    }
                    else if (item.TypeId == 2)
                    {
                        newOrderItem.Points = productDiscounts.PointRequired;

                    }

                    if (productDiscounts.DiscountTypeId == 1 && productDiscounts.PriceValue == 0)
                    {
                        if (_appConfig.IsVPointsMultiplierEnabled)
                        {
                            if (!newOrderItem.ProductTitle.Contains("voucher"))
                            {
                                if (productDiscounts.PercentageValue > 0)
                                {
                                    var newMultiplier = _appConfig.VPointsMultiplier * productDiscounts.PercentageValue;
                                    if (newMultiplier > _appConfig.VPointsMultiplierCap)
                                    {
                                        productDiscounts.PercentageValue = _appConfig.VPointsMultiplierCap;

                                    }
                                    else if (newMultiplier <= _appConfig.VPointsMultiplierCap)
                                    {
                                        productDiscounts.PercentageValue = newMultiplier;
                                    }

                                    newOrderItem.VPointsMultiplier = _appConfig.VPointsMultiplier;
                                    newOrderItem.VPointsMultiplierCap = _appConfig.VPointsMultiplierCap;

                                }
                            }
                        }

                        newOrderItem.Price -= newOrderItem.Price * productDiscounts.PercentageValue / 100;
                        discountedAmount = (newOrderItem.Price * productDiscounts.PercentageValue / 100).ToString();
                    }
                    else
                    {
                        newOrderItem.Price -= productDiscounts.PriceValue;
                        discountedAmount = productDiscounts.PriceValue.ToString();
                    }
                }

                subTotal = newOrderItem.Price.ToString();

                newOrderItem.DiscountedAmount = Convert.ToDecimal(discountedAmount);
                newOrderItem.SubtotalPrice = Convert.ToDecimal(itemPrice);
                newOrderItem.ProductDetail = JsonConvert.SerializeObject(item);
                order.OrderItems.Add(newOrderItem);
                order.TotalPrice = order.OrderItems.Sum(x => x.Price) + order.TotalShippingCost;
                //order.TotalPoints = order.OrderItems.Sum(x => x.Points);

                addedItems.Add(newOrderItem);
                order.OrderItems.Add(newOrderItem);

                var address = order.ShippingAddressLine1 + ", ";
                if (order.ShippingAddressLine2 != null && order.ShippingAddressLine2 != "")
                    address += order.ShippingAddressLine2 + ", ";
                address += order.ShippingPostcode + " ";
                address += order.ShippingCity + " ";
                address += order.ShippingState + ", ";
                address += order.ShippingCountry;

                _orderTable += $"<table border='0' cellpadding='0' cellspacing='0' width='100%' class='x_478386741tableButton'>" +
                             "<tbody style = 'font-family:Calibri'>" +
                             "<tr>" +
                             "<td align='center' valign='top' style='padding-top:20px; padding-bottom:20px; width:100%;'>" +
                             "<table style = 'width:100%; max-width :  800px; '>" +
                             "<tbody>" +
                       "<tr style = 'background-color:#eee;font-weight:bold;'>" +
                          "<td style = 'padding: 0 15px'> Item </td>" +
                           "<td></td>" +
                           "<td style = 'padding:5px;text-align:center;'> Qty </td>" +
                            "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;'> Price </td>" +
                        "</tr>" +
                        "<tr>" +
                        "<td width = '15%'>" +
                            $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductCartPreviewSmallImage}'>" + "</td>" +
                                "<td style = 'padding:5px;font-size:12px;'>" +
                                $"<a style = 'font-size:14px;color:#666;text-decoration:none;' href ='https://vodus.my/Order/History' target = '_blank'>" +
                                        $"{item.Title}" +
                                "</a>" +
                            "</td> " +
                            "<td style = 'padding:5px;text-align:center;'>1</td>" +
                            "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;color:#8C37F6;font-weight:bold;'>RM " +
                                    newOrderItem.SubtotalPrice.ToString("F") +
                            "</td>" +
                            "</tr>" +
                        "</tbody> " +
                    "</table>" +
                    "<table style='border:1px #ddd solid;border-left:0;border-right:0;'>" +
                                    "<tbody>" +
                                      "<tr>" +
                                    "<td width='50%' style='padding:15px;padding-right:0;'> " +
                                        "<p style = 'color:#000;font-size:16px;font-weight:bold;text-align:left;margin:0'> Shipping Address </p>" +
                                        "<p style = 'text-align:left;font-size:13px;margin:5px 0'>" +
                                            $"{address}" +
                                        "</p>" +
                                    "</td>" +
                                    "<td width='50%' style='min-width:150px;padding:15px;padding-left:0;'>" +
                                    "<table style='width:100%;'> " +
                                       "<tbody style='padding:0 15px;font-size:16px;'>  " +
                                           "<tr>" +
                                               "<td width='50%' style='text-align: right;'> VPoints:</td> " +
                                              $"<td width='50%' style='text-align: right;'>{newOrderItem.Points} </td> " +
                                           "</tr>  " +
                                           "<tr>  " +
                                               "<td width='50%' style='text-align:right;'> Subtotal: </td> " +
                                               $"<td width='50%' style='text-align:right;'> RM {newOrderItem.SubtotalPrice.ToString("F")}</td>" +
                                           "</tr> " +
                                           "<tr style='width: 100%; vertical-align: top; '> " +
                                               "<td width = '50%' style='text-align:right;'> Discount:</td>" +
                                               $"<td width = '50%' style='text-align:right;'> -RM {newOrderItem.DiscountedAmount.ToString("F")}</td>" +
                                           "</tr>  " +
                                           "<tr style='width: 100%; vertical-align: top; '> " +
                                               "<td width = '50%' style='text-align:right;'> Shipping Cost:</td>" +
                                               $"<td width = '50%' style='text-align:right;'> RM {newOrderItem.ShippingCost.ToString("F")}</td>" +
                                           "</tr>  " +
                                           "<tr style='vertical-align:top;font-weight:bold;'> " +
                                               "<td width='50%' style='text-align:right;'> Total:</td> " +
                                                $"<td width='50%' style='text-align:right;color:#8C37F6;'> RM {(newOrderItem.Price + newOrderItem.ShippingCost).ToString("F")} </td>  " +
                                           "</tr> " +
                                       "</tbody> " +
                                   " </table> " +
                                   " </td> " +
                                   " </tr> " +
                                   " </tbody> " +
                                "</table> " +
                            "</td> " +
                        "</tr> " +
                    "</tbody> " +
                "</table> ";


            }
            //int requiredPoints = (int)(addedItems.FirstOrDefault().Points * (Math.Ceiling((decimal)addedItems.Count / multiTireVpointThreshold)));
            return addedItems.Sum(x => x.Points);
        }

        private OrderItemExternal SetupExternalOrderItems(ProductList item, CheckoutRequestModel request, Database.Postgres.RewardsEntities.Orders order, OrderShopExternal orderShopExternal, int multiTireVpointThreshold)
        {
            var itemPrice = "";
            var discountedAmount = "0";
            var subTotal = "";

            var id = Guid.NewGuid();
            var newOrderItem = new OrderItemExternal
            {
                Id = id,
                ProductCartPreviewSmallImageURL = item.ProductCartPreviewSmallImage,
                ProductTitle = item.Title,
                ExternalItemId = item.ExternalItemId,
                ExternalUrl = item.ExternalItemUrl,
                Points = 0,
                ShortId = DateTime.Now.ToString("dd") + id.ToString().Split("-")[0],
                Quantity = (int)item.OrderQuantity,
                OrderItemExternalStatus = 1,
            };

            newOrderItem.Price = (decimal)item.DiscountedPrice;
            newOrderItem.IsVariationProduct = true;
            newOrderItem.VariationText = item.VariationText;
            newOrderItem.OriginalPrice = (decimal)item.DiscountedPrice;
            itemPrice = item.DiscountedPrice.ToString();

            if (item.AdditionalDiscount != null && item.AdditionalDiscount.Type > 0)
            {
                newOrderItem.Points = (int)(item.AdditionalDiscount.PointsRequired * Math.Ceiling((decimal)item.OrderQuantity / multiTireVpointThreshold));

                if (item.AdditionalDiscount.Type == 1)
                {
                    discountedAmount = Math.Round((newOrderItem.Price * item.AdditionalDiscount.ExternalItemDiscountPercentage / 100), 2).ToString();
                    newOrderItem.Price -= Math.Round(newOrderItem.Price * item.AdditionalDiscount.ExternalItemDiscountPercentage / 100, 2);
                }
            }

            subTotal = newOrderItem.Price.ToString();

            newOrderItem.DiscountedAmount = Convert.ToDecimal(discountedAmount);
            newOrderItem.SubtotalPrice = Convert.ToDecimal(newOrderItem.Price);
            newOrderItem.TotalPrice = Convert.ToDecimal(newOrderItem.Price) * newOrderItem.Quantity;

            var address = order.ShippingAddressLine1 + ", ";
            if (order.ShippingAddressLine2 != null && order.ShippingAddressLine2 != "")
                address += order.ShippingAddressLine2 + ", ";
            address += order.ShippingPostcode + " ";
            address += order.ShippingCity + " ";
            address += order.ShippingState + ", ";
            address += order.ShippingCountry;

            _orderTable += $"<table border='0' cellpadding='0' cellspacing='0' width='100%' class='x_478386741tableButton'>" +
                         "<tbody style = 'font-family:Calibri'>" +
                         "<tr>" +
                         "<td align='center' valign='top' style='padding-top:20px; padding-bottom:20px; width:100%;'>" +
                         "<table style = 'width:100%; max-width :  800px; '>" +
                         "<tbody>" +
                   "<tr style = 'background-color:#eee;font-weight:bold;'>" +
                      "<td style = 'padding: 0 15px'> Item </td>" +
                       "<td></td>" +
                       "<td style = 'padding:5px;text-align:center;'> Qty </td>" +
                        "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;'> Price </td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td width = '15%'>" +
                        $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductCartPreviewSmallImage}'>" + "</td>" +
                            "<td style = 'padding:5px;font-size:12px;'>" +
                            $"<a style = 'font-size:14px;color:#666;text-decoration:none;' href ='https://vodus.my/Order/History' target = '_blank'>" +
                                    $"{item.Title}" +
                            "</a>" +
                        "</td> " +
                        "<td style = 'padding:5px;text-align:center;'>" + newOrderItem.Quantity + "</td>" +
                        "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;color:#8C37F6;font-weight:bold;'>RM " +
                                newOrderItem.SubtotalPrice.ToString("F") +
                        "</td>" +
                        "</tr>" +
                    "</tbody> " +
                "</table>" +
                "<table style='border:1px #ddd solid;border-left:0;border-right:0;'>" +
                                "<tbody>" +
                                  "<tr>" +
                                "<td width='50%' style='padding:15px;padding-right:0;'> " +
                                    "<p style = 'color:#000;font-size:16px;font-weight:bold;text-align:left;margin:0'> Shipping Address </p>" +
                                    "<p style = 'text-align:left;font-size:13px;margin:5px 0'>" +
                                        $"{address}" +
                                    "</p>" +
                                "</td>" +
                                "<td width='50%' style='min-width:150px;padding:15px;padding-left:0;'>" +
                                "<table style='width:100%;'> " +
                                   "<tbody style='padding:0 15px;font-size:16px;'>  " +
                                       "<tr>" +
                                           "<td width='50%' style='text-align: right;'> VPoints:</td> " +
                                          $"<td width='50%' style='text-align: right;'>{newOrderItem.Points} </td> " +
                                       "</tr>  " +
                                       "<tr>  " +
                                           "<td width='50%' style='text-align:right;'> Subtotal: </td> " +
                                           $"<td width='50%' style='text-align:right;'> RM {newOrderItem.SubtotalPrice.ToString("F")}</td>" +
                                       "</tr> " +
                                       "<tr style='width: 100%; vertical-align: top; '> " +
                                           "<td width = '50%' style='text-align:right;'> Discount:</td>" +
                                           $"<td width = '50%' style='text-align:right;'> -RM {newOrderItem.DiscountedAmount.ToString("F")}</td>" +
                                       "</tr>  " +
                                       "<tr style='width: 100%; vertical-align: top; '> " +
                                           "<td width = '50%' style='text-align:right;'> Shipping Cost:</td>" +
                                           $"<td width = '50%' style='text-align:right;'> RM {orderShopExternal.ShippingCost.ToString("F")}</td>" +
                                       "</tr>  " +
                                       "<tr style='vertical-align:top;font-weight:bold;'> " +
                                           "<td width='50%' style='text-align:right;'> Total:</td> " +
                                            $"<td width='50%' style='text-align:right;color:#8C37F6;'> RM {((newOrderItem.Price * newOrderItem.Quantity) + orderShopExternal.ShippingCost).ToString("F")} </td>  " +
                                       "</tr> " +
                                   "</tbody> " +
                               " </table> " +
                               " </td> " +
                               " </tr> " +
                               " </tbody> " +
                            "</table> " +
                        "</td> " +
                    "</tr> " +
                "</tbody> " +
            "</table> ";

            return newOrderItem;
        }

        private async Task<string> ValidatePromoCode(CheckoutRequestModel request,decimal totalPrice) {
            if (!string.IsNullOrEmpty(request.PromoCode))
            {
                if (string.IsNullOrEmpty(request.PromoCode))
                {
                    return "Promo code is missing";
                  
                }

                _promo = await _rewardsDBContext.PromoCodes.Where(x => x.PromoCode == request.PromoCode && x.Status == 1).FirstOrDefaultAsync();
                if (_promo == null)
                {
                    return "Incorrect or invalid promo code";
                }

                //  Check if user qualified if its selected users only
                if (_promo.IsSelectedUserOnly)
                {
                    var promoCodeSelectedUsers = await _rewardsDBContext.PromoCodeSelectedUsers.Where(x => x.PromoCodeId == _promo.Id && x.Email == request.UserName).FirstOrDefaultAsync();
                    if (promoCodeSelectedUsers == null)
                    {
                        return "Incorrect or invalid promo code";
                    }
                }
                var orders = await _rewardsDBContext.Orders.AsTracking().Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.OrderStatus == 2).ToListAsync();
                if (orders.Where(x => x.PromoCodeId == _promo.Id).Count() >= _promo.TotalAllowedPerUser)
                {
                    return "You have reach the promo code allowed for this code or you are not qualified to use this promo code";
                }

                //  Check items 
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


                var hasError = false;
                foreach (var item in request.ProductList)
                {
                    if (item.ProductId != 0)
                    {
                        if (voucherItemId.Contains(item.ProductId))
                        {
                            hasError = true;
                            _errorMessage = "Promo code cannot be use with cash voucher items";
                            break;
                        }
                    }
                }
                if (hasError)
                {
                    return _errorMessage;
                }

                if (totalPrice <= _promo.MinSpend)
                {
                    return $"The mininum spending required for this promo code is RM{_promo.MinSpend}";

                }

                if (_promo.TotalRedeemed >= _promo.TotalRedemptionAllowed)
                {
                    return $"The promo code is no longer available";
                }
               
            }

            return null;
        }


        private async Task<SendGridMessage> CreateToken(Guid orderItemId, int productId, int masterMemberProfile, string email, string fullName)
        {

            var product = await _rewardsDBContext.Products.Include(x => x.DealExpirations).FirstOrDefaultAsync(x => x.Id == productId);
            if (product != null)
            {
                var deallExpiration = await _rewardsDBContext.DealExpirations.FirstOrDefaultAsync(x => x.Id == product.DealExpirationId);// product.DealExpiration;
                var itemPrice = product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value : (product.Price.HasValue ? product.Price.Value : 0);

                var msg = new SendGridMessage();

                if (deallExpiration.ExpirationTypeId == 1)
                {
                    //instore redemption 
                    var item = new InStoreRedemptionTokens();
                    item.StartDate = DateTime.Now;
                    item.ExpiredDate = DateTime.Now.AddDays(deallExpiration.TotalValidDays.Value);
                    item.ProductId = productId;
                    item.IsActivated = true;
                    item.IsRedeemed = false;
                    item.MasterMemberProfileId = masterMemberProfile;
                    item.MerchantId = product.MerchantId;
                    item.OutletId = null;
                    item.ProductTitle = product.Title;
                    item.OrderItemId = orderItemId;
                    item.RedeemedAt = null;
                    item.CreatedAt = DateTime.Now;
                    item.RedemptionInfo = product.RedemptionInfo;
                    item.Revenue = itemPrice;// revenue;
                    item.Token = "";
                    item.Email = email;
                    _rewardsDBContext.InStoreRedemptionTokens.Add(item);
                    _rewardsDBContext.SaveChanges();

                    if (product.ThirdPartyTypeId.HasValue)
                    {

                    }
                    else
                    {
                        ;
                        item.Token = Voupon.Common.RedemptionTokenGenerator.InStoreRedemptionToken.GenerateToken(item.Id);
                        _rewardsDBContext.SaveChanges();

                        var url = Environment.GetEnvironmentVariable(EnvironmentKey.BASE_URL) + "/order/store-redemption/" + item.OrderItemId;

                        var image = GenerateQRCodeHtmlImageSrc(Environment.GetEnvironmentVariable(EnvironmentKey.VOUPON_URL) + "/qr/v/" + item.Token, 200, 200, 0);

                        msg.SetTemplateId(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.TEMPLATES.IN_STORE_REDEMPTION));
                        msg.SetFrom(new EmailAddress(Environment.GetEnvironmentVariable(EnvironmentKey.EMAIL.FROM.NO_REPLY), "Vodus No-Reply"));
                        msg.SetSubject("Vodus InStore Redemption");
                        msg.AddTo(new EmailAddress(email));
                        msg.AddTo("merchant@vodus.my");
                        msg.AddSubstitution("-productTitle-", product.Title);
                        msg.AddSubstitution("-voucherCode-", item.Token);
                        msg.AddSubstitution("-voucherUrl-", url);
                        msg.AddSubstitution("-image-", image);
                        msg.AddSubstitution("-redemptionFlow-", product.RedemptionInfo);
                        return msg;
                    }
                }
                else if (deallExpiration.ExpirationTypeId == 2)
                {
                    //instore redemption 
                    var item = new InStoreRedemptionTokens();
                    item.StartDate = deallExpiration.StartDate.Value;
                    item.ExpiredDate = deallExpiration.ExpiredDate.Value;
                    item.ProductId = productId;
                    item.IsActivated = true;
                    item.IsRedeemed = false;
                    item.MasterMemberProfileId = masterMemberProfile;
                    item.MerchantId = product.MerchantId;
                    item.OutletId = null;
                    item.ProductTitle = product.Title;
                    item.OrderItemId = orderItemId;
                    item.RedeemedAt = null;
                    item.CreatedAt = DateTime.Now;
                    item.RedemptionInfo = product.RedemptionInfo;
                    item.Revenue = itemPrice;// revenue;
                    item.Token = "";
                    item.Email = email;
                    _rewardsDBContext.InStoreRedemptionTokens.Add(item);
                    _rewardsDBContext.SaveChanges();

                    if (product.ThirdPartyTypeId.HasValue)
                    {
                    }
                    else
                    {
                        item.Token = Voupon.Common.RedemptionTokenGenerator.InStoreRedemptionToken.GenerateToken(item.Id);
                        _rewardsDBContext.SaveChanges();

                        var url = Environment.GetEnvironmentVariable(EnvironmentKey.BASE_URL) + "/order/store-redemption/" + item.OrderItemId;

                        var image = GenerateQRCodeHtmlImageSrc(Environment.GetEnvironmentVariable(EnvironmentKey.VOUPON_URL) + "/qr/v/" + item.Token, 200, 200, 0);

                        msg.SetTemplateId(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.TEMPLATES.IN_STORE_REDEMPTION));
                        msg.SetFrom(new EmailAddress(Environment.GetEnvironmentVariable(EnvironmentKey.EMAIL.FROM.NO_REPLY), "Vodus No-Reply"));
                        msg.SetSubject("Vodus InStore Redemption");
                        msg.AddTo(new EmailAddress(email));
                        msg.AddTo("merchant@vodus.my");
                        msg.AddSubstitution("-productTitle-", product.Title);
                        msg.AddSubstitution("-voucherCode-", item.Token);
                        msg.AddSubstitution("-voucherUrl-", url);
                        msg.AddSubstitution("-image-", image);
                        msg.AddSubstitution("-redemptionFlow-", product.RedemptionInfo);
                        return msg;

                    }
                }
                else if (deallExpiration.ExpirationTypeId == 4)
                {
                    //digital redemption
                    var item = new DigitalRedemptionTokens();
                    item.CreatedAt = DateTime.Now;
                    item.ProductId = productId;
                    item.StartDate = deallExpiration.StartDate.Value;
                    item.ExpiredDate = deallExpiration.ExpiredDate.Value;
                    item.IsActivated = true;
                    item.IsRedeemed = false;
                    item.MasterMemberProfileId = masterMemberProfile;
                    item.MerchantId = product.MerchantId;
                    item.ProductTitle = product.Title;
                    item.OrderItemId = orderItemId;
                    item.RedeemedAt = null;
                    item.RedemptionInfo = product.RedemptionInfo;
                    item.Revenue = itemPrice;// revenue;                       
                    item.Token = "";
                    item.Email = email;
                    item.TokenType = 1;
                    _rewardsDBContext.DigitalRedemptionTokens.Add(item);
                    _rewardsDBContext.SaveChanges();

                    return null;
                }
                else if (deallExpiration.ExpirationTypeId == 5)
                {
                    //delivery redemption
                    var item = new DeliveryRedemptionTokens();
                    item.CreatedAt = DateTime.Now;
                    item.ProductId = productId;
                    item.ExpiredDate = null;
                    item.IsActivated = true;
                    item.IsRedeemed = false;
                    item.MasterMemberProfileId = masterMemberProfile;
                    item.MerchantId = product.MerchantId;
                    item.ProductTitle = product.Title;
                    item.OrderItemId = orderItemId;
                    item.RedeemedAt = null;
                    item.RedemptionInfo = product.RedemptionInfo;
                    item.Revenue = itemPrice;// revenue;
                    item.StartDate = null;
                    item.CourierProvider = "";
                    item.Token = "";
                    item.Email = email;
                    _rewardsDBContext.DeliveryRedemptionTokens.Add(item);
                    _rewardsDBContext.SaveChanges();
                    return null;
                }
            }
            return null;
        }


        private static string GenerateQRCodeHtmlImageSrc(string token, int height, int width, int margin)
        {
            string imageSrc = "";
            var qrCodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = margin
                }
            };
            var pixelData = qrCodeWriter.Write(token);

            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image    
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                // save to stream as PNG    
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageSrc = String.Format("data:image/png;base64,{0}", Convert.ToBase64String(ms.ToArray()));
            }
            return imageSrc;

        }

        public async Task<string> GetFromBlob(int id)
        {
            var AzureBlob = new AzureBlob();
            var imageResult = await AzureBlob.BlobSmallImagesListQuery(new AzureBlob.BlobSmallImagesListModel
            {
                AzureBlobStorage = _azureBlobStorage,
                FilePath = FilePathEnum.Products_Images,
                ContainerName = ContainerNameEnum.Products,
                GetIListBlobItem = false,
                Id = id
            });

            string imageUrl = "images/not-available-stamp.jpg";
            if (imageResult != null && imageResult.Any())
            {
                imageUrl = imageResult.Count > 0 ? imageResult[0] : "";
            }

            return imageUrl;
        }
        public class CreateOrderResponseViewModel : ApiResponseViewModel
        {
          public CreateOrderResponse Data { get; set; }
        }



        public class CreateOrderResponse {
            public Guid OrderId { get; set; }
            public int MasterMemberPoints { get; set; }

            public string Message { get; set; }
            public int OrderStatus { get; set; }

            public string RedirectUrl { get; set; }
        }


        private async Task<string> SetConfig() {

            var config = await _rewardsDBContext.AppConfig.FirstOrDefaultAsync();

            if (!config.IsCheckoutEnabled.Value)
            {
                
                return "Checkout is not available at the moment. Please try again later and sorry for the inconvenience caused.";
            }


            if (config != null)
            {
                _multiTireVpointThreshold = config.MaxQuantityPerVPounts;
            }

            return "";
        }

        private async Task<bool> Logger(LogActivity request) {

                var activityLogs = new Database.Postgres.RewardsEntities.ActivityLogs
                {
                    Id = new Guid(),
                    ActionData = request.ActionData,
                    ActionId = request.ActionId,
                    ActionName = request.ActionName,
                    ActionFunction = request.ActionFunction,
                    IsSuccessful = request.IsSuccessful,
                    Message = request.Message,
                    TriggerBy = request.TriggerBy,
                    TriggerFor = request.TriggerFor,
                    CreatedAt = DateTime.Now
                };

                await _rewardsDBContext.ActivityLogs.AddAsync(activityLogs);
                await _rewardsDBContext.SaveChangesAsync();
                return true;
            /*await new CreateActivityLogCommand(_rewardsDBContext).Create(new CreateActivityLogCommand.CreateActivityRequest
            {
                ActionName = "CreateOrderCommand",
                ActionFunction = "Retrieving master profile",
                ActionData = JsonConvert.SerializeObject(request),
                ActionId = new Guid().ToString(),
                TriggerBy = request.Email,
                Message = "Invalid master profile",
                TriggerFor = request.Email,
                IsSuccessful = false

            });*/
        }

        public class LogActivity
        {
            public Guid Id { get; set; }
            public string ActionName { get; set; }
            public string ActionFunction { get; set; }
            public string ActionId { get; set; }
            public string ActionData { get; set; }
            public string Message { get; set; }
            public string Email { get; set; }
            public bool IsSuccessful { get; set; }
            public string TriggerFor { get; set; }
            public string TriggerBy { get; set; }
            public DateTime CreatedAt { get; set; }
        }
        private class LogData
        {
            public CheckoutRequestModel Request { get; set; }
            public string Email { get; set; }
            public int MasterMemberProfileId { get; set; }
            public int BeforeOrderPoints { get; set; }
            public int PendingOrderToBeDeductedPoints { get; set; }
            public int TotalUsedPoints { get; set; }
            public decimal TotalPrice { get; set; }
            public int TotalItems { get; set; }
            public int AfterOrderPoints { get; set; }

        }

        private class CheckoutRequestModel
        {
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }

            public string UserName { get; set; }

            public string BillingPersonFirstName { get; set; }
            public string BillingPersonLastName { get; set; }
            public string BillingEmail { get; set; }
            public string BillingMobileNumber { get; set; }
            public string BillingMobileCountryCode { get; set; }
            public string BillingAddressLine1 { get; set; }
            public string BillingAddressLine2 { get; set; }
            public string BillingPostcode { get; set; }
            public string BillingCity { get; set; }
            public string BillingState { get; set; }
            public string BillingCountry { get; set; }
            public string ShippingPersonFirstName { get; set; }
            public string ShippingPersonLastName { get; set; }
            public string ShippingEmail { get; set; }
            public string ShippingMobileNumber { get; set; }
            public string ShippingMobileCountryCode { get; set; }
            public string ShippingAddressLine1 { get; set; }
            public string ShippingAddressLine2 { get; set; }
            public string ShippingPostcode { get; set; }
            public string ShippingCity { get; set; }
            public string ShippingState { get; set; }
            public string ShippingCountry { get; set; }
            public decimal OrderShippingCost { get; set; }
            public string PromoCode { get; set; }

            [JsonProperty("productList")]
            public List<ProductList> ProductList { get; set; }

            public List<OrderShippingCostForPoductIdAndVariationIdModel> OrderShippingCosts { get; set; }
        }

        private class Merchant
        {
            [JsonProperty("id")]
            public long Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
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

        public partial class DealExpiration
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
        public class ShippingCostQuery
        {
            public int MasterMemberProfileId { get; set; }
            public int[] ProductIds { get; set; }
            public List<ProductIdAndVariationIdModel> ProductVariationIds { get; set; }
            public int ProvinceId { get; set; }

        }


        public class ProductIdAndVariationIdModel
        {
            public int ProductId { get; set; }
            public int VariationId { get; set; }
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalVariationText { get; set; }
            public string ProductTitle { get; set; }
            public int Quantity { get; set; }
            //public decimal ProductPrice { get; set; }

        }


        public class OrderShippingCostForPoductIdAndVariationIdModel
        {
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalItemVariationText { get; set; }
            public int ProductId { get; set; }
            public int VariationId { get; set; }
            public decimal OrderShippingCost { get; set; }
            public string ProductTitle { get; set; }

            //public decimal ProductPrice { get; set; }
        }


    }
}
