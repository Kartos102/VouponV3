using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Products = Voupon.Database.Postgres.RewardsEntities.Products;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.Common.ProductCategories.Queries;
using Voupon.Rewards.WebApp.Common.ShippingCost.Models;
using static Voupon.Rewards.WebApp.Services.Deal.Page.DetailPage;
using Voupon.Common.Services.ActivityLogs.Commands;

namespace Voupon.Rewards.WebApp.Services.Cart.Commands
{
    public class CreateOrderCommand : CreateOrderCommandModel, IRequest<ApiResponseViewModel>
    {

    }
    public class CreateTempUserCommandHandler : IRequestHandler<CreateOrderCommand, ApiResponseViewModel>
    {
        private readonly VodusV2Context vodusV2Context;
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        private readonly IAzureBlobStorage azureBlobStorage;

        private string _errorMessage { get; set; }
        private bool _isProductDataValid { get; set; }
        public string _email { get; set; }

        private string _orderTable { get; set; }

        private string _aggregatorUrl { get; set; }
        private Voupon.Database.Postgres.RewardsEntities.AppConfig _appConfig { get; set; }

        public CreateTempUserCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, IAzureBlobStorage azureBlobStorage)
        {
            _rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.appSettings = appSettings;
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {

            var apiResponseViewModel = new ApiResponseViewModel();

            var logData = new LogData
            {
                Request = request
            };

            _rewardsDBContext.Database.BeginTransaction();
            vodusV2Context.Database.BeginTransaction();
            _isProductDataValid = true;
            var aggregatorUrl = await vodusV2Context.AggregatorApiUrls.ToListAsync();
            _aggregatorUrl = aggregatorUrl.OrderBy(x => x.LastUpdatedAt).First().Url;

            var config = await _rewardsDBContext.AppConfig.FirstOrDefaultAsync();

            if (!config.IsCheckoutEnabled.Value)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Checkout is not available at the moment. Please try again later and sorry for the inconvenience caused.";
                return apiResponseViewModel;
            }
            int multiTireVpointThreshold = 3;
            if(config != null)
            {
                multiTireVpointThreshold = config.MaxQuantityPerVPounts;
            }
            var masterMemberProfile = await vodusV2Context.MasterMemberProfiles.Include(x=>x.User).Where(x=>x.Id == request.MasterMemberProfileId).FirstOrDefaultAsync();

            if (masterMemberProfile == null)
            {
                await new CreateActivityLogCommand(_rewardsDBContext).Create(new CreateActivityLogCommand.CreateActivityRequest
                {
                    ActionName = "CreateOrderCommand",
                    ActionFunction = "Retrieving master profile",
                    ActionData = JsonConvert.SerializeObject(request),
                    ActionId = new Guid().ToString(),
                    TriggerBy = request.Email,
                    Message = "Invalid master profile",
                    TriggerFor = request.Email,
                    IsSuccessful = false

                });

                apiResponseViewModel.Message = "Invalid request [001]";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }

            var user = masterMemberProfile.User;

            if (user == null)
            {
                await new CreateActivityLogCommand(_rewardsDBContext).Create(new CreateActivityLogCommand.CreateActivityRequest
                {
                    ActionName = "CreateOrderCommand",
                    ActionFunction = "Retrieving user data",
                    ActionData = JsonConvert.SerializeObject(request),
                    ActionId = new Guid().ToString(),
                    TriggerBy = request.Email,
                    Message ="request.UserName return null",
                    TriggerFor = request.Email,
                    IsSuccessful = false

                });

                apiResponseViewModel.Message = "Invalid request [002]";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }

            _email = user.Email;
            request.Email = user.Email;
            request.BillingEmail = user.Email;
            request.ShippingEmail = user.Email;

           

            //  Deduct any pending order points.
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

            var order = new Voupon.Database.Postgres.RewardsEntities.Orders();

            try
            {
                order = new Voupon.Database.Postgres.RewardsEntities.Orders
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
                        int result = await SetupOrderItems(item, request, order, orderId, multiTireVpointThreshold);
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
                                var result = SetupExternalOrderItems(item, request, order, orderShopExternal, multiTireVpointThreshold);
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
                            
                            //1783 Place of change for 
                            
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
                    await new CreateActivityLogCommand(_rewardsDBContext).Create(new CreateActivityLogCommand.CreateActivityRequest
                    {
                        ActionName = "CreateOrderCommand",
                        ActionFunction = "Invalid product data",
                        ActionData = JsonConvert.SerializeObject(request),
                        ActionId = new Guid().ToString(),
                        TriggerBy = request.Email,
                        Message = $"!_isProductDataValid {_errorMessage}",
                        TriggerFor = request.Email,
                        IsSuccessful = false

                    });

                    if (string.IsNullOrEmpty(_errorMessage))
                    {
                        apiResponseViewModel.Message = "Invalid request";
                    }
                    else
                    {
                        apiResponseViewModel.Message = _errorMessage;
                    }
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

                if (!string.IsNullOrEmpty(_errorMessage))
                {
                    await new CreateActivityLogCommand(_rewardsDBContext).Create(new CreateActivityLogCommand.CreateActivityRequest
                    {
                        ActionName = "CreateOrderCommand",
                        ActionFunction = "Invalid product data",
                        ActionData = JsonConvert.SerializeObject(request),
                        ActionId = new Guid().ToString(),
                        TriggerBy = request.Email,
                        Message = _errorMessage,
                        TriggerFor = request.Email,
                        IsSuccessful = false

                    });

                    _rewardsDBContext.Database.RollbackTransaction();
                    vodusV2Context.Database.RollbackTransaction();

                    apiResponseViewModel.Message = _errorMessage;
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;

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
                            apiResponseViewModel.Message = "Invalid request [010]";

                        }
                        else
                        {
                            apiResponseViewModel.Message = "You do not have enough VPoints to proceed with checkout.";
                        }
                        //apiResponseViewModel.Message = "You do not have enough VPoints to proceed with checkout. If you have previously added items to cart but did not complete the payment, cancel that order to free up your points.";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }
                }



                masterMemberProfilePoints = masterMemberProfile.AvailablePoints;

                logData.TotalUsedPoints = order.TotalPoints;
                logData.AfterOrderPoints = masterMemberProfile.AvailablePoints /*- order.TotalPoints*/;
                logData.TotalPrice = order.TotalPrice;// + order.TotalShippingCost;
                logData.TotalItems = order.TotalItems;
                order.TotalPriceBeforePromoCodeDiscount = order.TotalPrice;
                //  Check Promo Code
                if (!string.IsNullOrEmpty(request.PromoCode))
                {
                    if (string.IsNullOrEmpty(request.PromoCode))
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Promo code is missing";
                        return apiResponseViewModel;
                    }

                    var promo = await _rewardsDBContext.PromoCodes.Where(x => x.PromoCode == request.PromoCode && x.Status == 1).FirstOrDefaultAsync();
                    if (promo == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Incorrect or invalid promo code";
                        return apiResponseViewModel;
                    }

                    //  Check if user qualified if its selected users only
                    if (promo.IsSelectedUserOnly)
                    {
                        var promoCodeSelectedUsers = await _rewardsDBContext.PromoCodeSelectedUsers.Where(x => x.PromoCodeId == promo.Id && x.Email == request.UserName).FirstOrDefaultAsync();
                        if (promoCodeSelectedUsers == null)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Incorrect or invalid promo code";
                            return apiResponseViewModel;
                        }
                    }
                    var orders = await _rewardsDBContext.Orders.AsTracking().Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.OrderStatus == 2).ToListAsync();
                    if (orders.Where(x => x.PromoCodeId == promo.Id).Count() >= promo.TotalAllowedPerUser)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "You have reach the promo code allowed for this code or you are not qualified to use this promo code";
                        return apiResponseViewModel;
                    }

                    //  Check items 
                    var voucherItemId = new List<long>();

                    if (appSettings.Value.App.BaseUrl == "https://vodus.my")
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
                                apiResponseViewModel.Successful = false;
                                apiResponseViewModel.Message = "Promo code cannot be use with cash voucher items";
                                break;
                            }
                        }
                    }
                    if (hasError)
                    {
                        return apiResponseViewModel;
                    }

                    if (order.TotalPrice <= promo.MinSpend)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = $"The mininum spending required for this promo code is RM{promo.MinSpend}";
                        return apiResponseViewModel;
                    }

                    if (promo.TotalRedeemed >= promo.TotalRedemptionAllowed)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = $"The promo code is no longer available";
                        return apiResponseViewModel;
                    }
                    decimal discountWithPromoCode = 0;
                    if (promo.PromoCode == "2XPROMO")
                    {
                        foreach (var shopExternal in order.OrderShopExternal)
                        {
                            foreach (var externalitem in shopExternal.OrderItemExternal)
                            {
                                if (externalitem.DiscountedAmount != 0)
                                {
                                    decimal newMultiplier = 0;
                                    decimal discountPc = 0;

                                    newMultiplier = Math.Round(promo.DiscountValue * externalitem.DiscountedAmount * 100)  / externalitem.OriginalPrice;
                                    discountPc = externalitem.DiscountedAmount;


                                    if (newMultiplier > promo.MaxDiscountValue)
                                    {
                                        continue;
                                    }
                                    else if (newMultiplier <= promo.MaxDiscountValue)
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

                                newMultiplier = Math.Round((promo.DiscountValue * (orderItem.SubtotalPrice - orderItem.Price)  * 100) / orderItem.SubtotalPrice);
                                discountPc = orderItem.SubtotalPrice - orderItem.Price;


                                if (newMultiplier > promo.MaxDiscountValue)
                                {
                                    continue;
                                }
                                else if (newMultiplier <= promo.MaxDiscountValue)
                                {
                                    discountWithPromoCode += discountPc;//price
                                }
                            }
                        }
                    }
                    else
                    {
                        discountWithPromoCode = (order.TotalPrice * promo.DiscountValue) / 100;
                        if (discountWithPromoCode > promo.MaxDiscountValue)
                        {
                            discountWithPromoCode = promo.MaxDiscountValue;
                        }
                    }
                    var priceAfterDiscount = order.TotalPrice - discountWithPromoCode;

                    order.TotalPromoCodeDiscount = discountWithPromoCode;
                    order.TotalPrice = priceAfterDiscount;

                    order.PromoCodeId = promo.Id;
                    order.PromoCodeValue = promo.PromoCode;
                    order.PromoCodeExpireOn = promo.ExpireOn;
                    order.PromoCodeMinSpend = promo.MinSpend;
                    order.PromoCodeMaxDiscountValue = promo.MaxDiscountValue;
                    order.PromoCodeDiscountType = promo.DiscountType;
                    order.PromoCodeDiscountValue = discountWithPromoCode;

                    promo.TotalRedeemed += 1;
                    _rewardsDBContext.PromoCodes.Update(promo);
                    await _rewardsDBContext.SaveChangesAsync();

                    //1781 Backlog
                    if(promo.IsShipCostDeduct)
                    {
                        order.TotalShippingCost = 0;
                    }
                    //1781 Backlog
                    //  Update status to completed and deduct points
                    if (order.TotalPrice == 0 && order.TotalShippingCost == 0)
                    {
                        masterMemberProfile.AvailablePoints = masterMemberProfile.AvailablePoints - order.TotalPoints;
                        order.OrderStatus = 2;
                        apiResponseViewModel.Message = "Your order have been created and VPoints will be deducted from your account.";

                        vodusV2Context.MasterMemberProfiles.Update(masterMemberProfile);
                        await vodusV2Context.SaveChangesAsync();
                    }
                    else
                    {
                        order.OrderStatus = 1;
                        apiResponseViewModel.Message = "Your order have been created. Please complete the payment to complete the purchase";
                    }

                }

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
                vodusV2Context.Database.CommitTransaction();

                //masterMemberProfile.User.Email = "some test email";

                //  If the items is only consists of points items sent thank you email without needing payment step
                if (order.TotalPrice == 0 && order.TotalShippingCost == 0)
                {
                    //  Generate Receipt
                    var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                    var msg = new SendGridMessage();
                    msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.OrderConfirmation);
                    msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                    msg.SetSubject("Thank you for your order");
                    msg.AddTo(new EmailAddress(masterMemberProfile.User.Email));
                    msg.AddTo("merchant@vodus.my");
                    msg.AddSubstitution("-customerName-", masterMemberProfile.User.FirstName + " " + masterMemberProfile.User.LastName);
                    msg.AddSubstitution("-orderNumber-", "#" + order.ShortId);
                    msg.AddSubstitution("-orderDate-", order.CreatedAt.ToString("dd/MM/yyyy"));
                    msg.AddSubstitution("-orderTable-", _orderTable);
                    var response = sendGridClient.SendEmailAsync(msg).Result;

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
                            var sendGridClientForMerchant = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                            var msgForMerchant = new SendGridMessage();
                            msgForMerchant.AddTo(merchantEmail);
                            msgForMerchant.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                            msgForMerchant.SetSubject("Vodus - New order");

                            //  In Store
                            if (item.ExpirationTypeId == 2)
                            {
                                msgForMerchant.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.InStoreMerchantSalesNotificationEmail);
                            }
                            // Digital
                            else if (item.ExpirationTypeId == 4)
                            {
                                msgForMerchant.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.DigitalMerchantSalesNotificationEmail);
                            }
                            // Delivery
                            else if (item.ExpirationTypeId == 5)
                            {
                                msgForMerchant.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.DeliveryMerchantSalesNotificationEmail);
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
                            await new CreateActivityLogCommand(_rewardsDBContext).Create(new CreateActivityLogCommand.CreateActivityRequest
                            {
                                ActionName = "CreateOrderCommand",
                                ActionFunction  = "MerchantEmail",
                                ActionData = JsonConvert.SerializeObject(request),
                                ActionId = order.Id.ToString(),
                                TriggerBy = _email,
                                Message = await merchantEmailResponse.Body.ReadAsStringAsync(),
                                TriggerFor = merchantEmail,
                                IsSuccessful = true

                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                vodusV2Context.Database.RollbackTransaction();
                _rewardsDBContext.Database.RollbackTransaction();

                var errorLogs = new Voupon.Database.Postgres.RewardsEntities.ErrorLogs
                {
                    TypeId = CreateErrorLogCommand.Type.Service,
                    ActionName = "CreateOrderCommand",
                    ActionRequest = JsonConvert.SerializeObject(request),
                    CreatedAt = DateTime.Now,
                    Errors = ex.ToString()
                };

                await _rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                await _rewardsDBContext.SaveChangesAsync();

                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }

            apiResponseViewModel.Data = new CreateOrderResponseViewModel
            {
                OrderId = order.Id,
                MasterMemberPoints = masterMemberProfilePoints,
                OrderStatus = order.OrderStatus
            };

            await new CreateActivityLogCommand(_rewardsDBContext).Create(new CreateActivityLogCommand.CreateActivityRequest
            {
                ActionName = "CreateOrderCommand",
                ActionFunction = "Order created",
                ActionData = JsonConvert.SerializeObject(request),
                ActionId = order.Id.ToString(),
                TriggerBy = request.Email,
                Message = "Successfully created order",
                TriggerFor = request.Email,
                IsSuccessful = true

            });

            apiResponseViewModel.Successful = true;
            return apiResponseViewModel;
        }


        private async Task<int> SetupOrderItems(ProductList item, CreateOrderCommand request, Voupon.Database.Postgres.RewardsEntities.Orders order, Guid orderId, int multiTireVpointThreshold)
        {
            var product = await _rewardsDBContext.Products.Include(x => x.DealExpirations).Include(x => x.ProductDiscounts).Where(z => z.Id == item.ProductId).FirstOrDefaultAsync();

            var productVaruiation = await _rewardsDBContext.ProductVariation.Where(x => x.Id == item.VariationId).FirstOrDefaultAsync();

            var productDiscountList = await _rewardsDBContext.ProductDiscounts.Where(x => x.ProductId == item.ProductId).ToListAsync();

            List<Voupon.Database.Postgres.RewardsEntities.OrderItems> addedItems = new List<Voupon.Database.Postgres.RewardsEntities.OrderItems>();

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
                var filename = await azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Products, (int)item.ProductId + "/" + FilePathEnum.Products_Images);
                var fileList = new List<string>();
                foreach (var file in filename)
                {
                    fileList.Add(file.StorageUri.PrimaryUri.OriginalString);
                }

                if (fileList.Count > 0)
                {
                    imageUrl = fileList.Where(x => x.Contains("small")).First();
                    if (imageUrl == "")
                    {
                        imageUrl = fileList.First();
                    }
                    if (!imageUrl.Contains("https"))
                    {
                        imageUrl = imageUrl.Replace("http", "https");
                    }
                }

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
            if (!item.ProductCartPreviewSmallImage.Contains("https"))
            {
                item.ProductCartPreviewSmallImage = item.ProductCartPreviewSmallImage.Replace("http", "https");
            }

          

            var discountType = "";

            //1783 TASK NEW LOGIC - DEDUCTION OF VPOINTS FOR ORDERS - INTERNAL PRODUCTS

            //======================================================================================

            // Declare a variable to store the vpoints multiplier
            int vpointsMultiplier;

            // Calculate the vpoints multiplier based on the OrderQuantity of the item
            vpointsMultiplier = (int)Math.Ceiling((double)item.OrderQuantity / 2.0);
            if (item.OrderQuantity % 2 == 0)
            {
                vpointsMultiplier -= 1;
            }



            // The calculation works as follows:
            // - Adding 1 to OrderQuantity accounts for the fact that OrderQuantity starts at 1 instead of 0.
            // - Dividing the result by 2 gives the expected vpoints multiplier value.

            int totalPoints = addedItems.Sum(x => x.Points);
            int itemPerPoint = 0;
          
            int finalPointsPerOneItemAllQuantity = 0;


            if (item.OrderQuantity > 1)
            {
                for (var i = 0; i < item.OrderQuantity; i++)
                {
                    var itemPrice = "";
                    var discountedAmount = "0";
                    var subTotal = "";

                    var id = Guid.NewGuid();
                    var newOrderItem = new Voupon.Database.Postgres.RewardsEntities.OrderItems
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

                    

                    //1783 - Starting point
                    if (item.AdditionalDiscount != null && item.AdditionalDiscount.Id > 0)
                    {
                        //1783 - Starting point
                        itemPerPoint = (int)item.AdditionalDiscount.PointsRequired;

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
                            discountType = "percentage_based";
                        }
                        else
                        {
                            discountType = "value_based";
                            newOrderItem.Price -= productDiscounts.PriceValue;
                            discountedAmount = productDiscounts.PriceValue.ToString();
                        }



                        // If point item then add required points
                        if (item.TypeId == 1)
                        {
                       
                            //Final points calculation 

                            //If discount is percentage
                            if (discountType.Equals("percentage_based"))
                            {
                                finalPointsPerOneItemAllQuantity = itemPerPoint * vpointsMultiplier;
                                //Item points will received each divident of total points per item based on total item points
                                newOrderItem.Points = (int)Math.Round((double)finalPointsPerOneItemAllQuantity / (int)item.OrderQuantity);

                            }

                            //If Discount is a value
                            else if (discountType.Equals("value_based"))
                            {
                                finalPointsPerOneItemAllQuantity = itemPerPoint * (int)item.OrderQuantity;
                                //Item points will received each divident of total points per item based on total item points
                                newOrderItem.Points = itemPerPoint;
                            }

                      
                        }
                        else if (item.TypeId == 2)
                        {


                            //If discount is percentage
                            if (discountType.Equals("percentage_based"))
                            {
                                finalPointsPerOneItemAllQuantity = itemPerPoint * vpointsMultiplier;
                                //Item points will received each divident of total points per item based on total item points

                                //Why Round off here ??? 
                                //Pecentage discount deduction points were 
                                newOrderItem.Points = (int)Math.Round((double)finalPointsPerOneItemAllQuantity / (int)item.OrderQuantity);

                            }

                            //If Discount is a value
                            else if (discountType.Equals("value_based"))
                            {
                                finalPointsPerOneItemAllQuantity = itemPerPoint * (int)item.OrderQuantity;
                                //Item points will received each divident of total points per item based on total item points
                                newOrderItem.Points = itemPerPoint;
                            }


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
                            $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductCartPreviewSmallImage.Replace("http://", "https://")}'>" + "</td>" +
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
                                              $"<td width='50%' style='text-align: right;'>{finalPointsPerOneItemAllQuantity} </td> " +
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
                var newOrderItem = new Voupon.Database.Postgres.RewardsEntities.OrderItems
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
                    //Points = item.PointsRequired,
                    Points = itemPerPoint,

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

                    if(newOrderItem.VariationId.HasValue && newOrderItem.VariationId == 0)
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
                    itemPerPoint = (int)item.AdditionalDiscount.PointsRequired;
                   


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


                        discountType = "percentage_based";
                        newOrderItem.Price -= newOrderItem.Price * productDiscounts.PercentageValue / 100;
                        discountedAmount = (newOrderItem.Price * productDiscounts.PercentageValue / 100).ToString();
                    }
                    else
                    {
                        discountType = "value_based";
                        newOrderItem.Price -= productDiscounts.PriceValue;
                        discountedAmount = productDiscounts.PriceValue.ToString();
                    }

                    if (item.TypeId == 1)
                    {

                        //Final points calculation 

                        //If discount is percentage
                        if (discountType.Equals("percentage_based"))
                        {
                            finalPointsPerOneItemAllQuantity = itemPerPoint * vpointsMultiplier;
                        }

                        //If Discount is a value
                        else if (discountType.Equals("value_based"))
                        {
                            finalPointsPerOneItemAllQuantity = itemPerPoint * (int)item.OrderQuantity;
                        }

                        newOrderItem.Points = finalPointsPerOneItemAllQuantity;
                    }
                    else if (item.TypeId == 2)
                    {


                        //Final points calculation 

                        //If discount is percentage
                        if (discountType.Equals("percentage_based"))
                        {
                            finalPointsPerOneItemAllQuantity = itemPerPoint * vpointsMultiplier;
                        }

                        //If Discount is a value
                        else if (discountType.Equals("value_based"))
                        {
                            finalPointsPerOneItemAllQuantity = itemPerPoint * (int)item.OrderQuantity;
                        }

                        newOrderItem.Points = finalPointsPerOneItemAllQuantity;

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
                            $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductCartPreviewSmallImage.Replace("http://", "https://")}'>" + "</td>" +
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
                                              $"<td width='50%' style='text-align: right;'>{finalPointsPerOneItemAllQuantity} </td> " +
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







            return finalPointsPerOneItemAllQuantity;
        }
        private OrderItemExternal SetupExternalOrderItems(ProductList item, CreateOrderCommand request, Voupon.Database.Postgres.RewardsEntities.Orders order, OrderShopExternal orderShopExternal, int multiTireVpointThreshold)
        {
            var product = new DetailPageViewModel();

            /*
            try
            {
                var externalProductRequest = new ExternalProductRequest
                {
                    ExternalItemId = item.ExternalItemId,
                    ExternalShopId = item.ExternalShopId,
                    ExternalTypeId = item.ExternalTypeId
                };

                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(externalProductRequest), System.Text.Encoding.UTF8, "application/json");
                var httpClient = new HttpClient();
                var result = await httpClient.PostAsync($"{_aggregatorUrl}/v1/product", httpContent);
                var resultString = await result.Content.ReadAsStringAsync();
                var crawlerResult = JsonConvert.DeserializeObject<ApiResponseViewModel>(resultString);
                if (crawlerResult.Successful)
                {
                    product = JsonConvert.DeserializeObject<DetailPageViewModel>(crawlerResult.Data.ToString());
                }
                else
                {
                    _errorMessage = crawlerResult.Message;
                    return null;
                }
            }
            catch (Exception ex)
            {
                _errorMessage = ex.ToString();
                return null;
            }
            */
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

                //newOrderItem.Points = (int)(item.AdditionalDiscount.PointsRequired * Math.Ceiling((decimal)item.OrderQuantity / multiTireVpointThreshold));

                //1783 External Product Vpoints Calculation

                //======================================================================================

                // Declare a variable to store the vpoints multiplier
                int vpointsMultiplier;

                // Calculate the vpoints multiplier based on the OrderQuantity of the item
                vpointsMultiplier = (int)Math.Ceiling((double)item.OrderQuantity / 2.0);
                if (item.OrderQuantity % 2 == 0)
                {
                    vpointsMultiplier -= 1;
                }


                newOrderItem.Points = vpointsMultiplier * (int)item.AdditionalDiscount.PointsRequired;


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
                        $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductCartPreviewSmallImage.Replace("http://", "https://")}'>" + "</td>" +
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
                                          $"<td width='50%' style='text-align: right;'>{ newOrderItem.Points} </td> " +
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

        private async Task<SendGridMessage> CreateToken(Guid orderItemId, int productId, int masterMemberProfile, string email, string fullName)
        {

            var product = _rewardsDBContext.Products.Include(x => x.DealExpirations).FirstOrDefault(x => x.Id == productId);
            if (product != null)
            {
                var deallExpiration = _rewardsDBContext.DealExpirations.FirstOrDefault(x => x.Id == product.DealExpirationId);// product.DealExpiration;
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
                        //var result = await ThirdPartyRedemption(product);

                        //if (result != null)
                        //{
                        //    if (result.IsSuccessful)
                        //    {
                        //        item.Token = result.Url;
                        //        item.TokenType = 2;
                        //        _rewardsDBContext.SaveChanges();

                        //        var sendGridClient2 = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                        //        var msg2 = new SendGridMessage();
                        //        msg2.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.ThirdPartyRedemption);
                        //        msg2.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                        //        msg2.SetSubject("Vodus Redemption Url");
                        //        msg2.AddTo(new EmailAddress(email));
                        //        msg2.AddTo("merchant@vodus.my");

                        //        msg2.AddSubstitution("-customerName-", fullName);
                        //        msg2.AddSubstitution("-item-", product.Title);
                        //        msg2.AddSubstitution("-redemptionUrl-", result.Url);
                        //        msg2.AddSubstitution("-imageUrl-", product.ImageFolderUrl);
                        //        var response2 = sendGridClient2.SendEmailAsync(msg2).Result;
                        //        return msg;
                        //    }
                        //}
                        //return null;
                    }
                    else
                    {
                        item.Token = Voupon.Common.RedemptionTokenGenerator.InStoreRedemptionToken.GenerateToken(item.Id);
                        _rewardsDBContext.SaveChanges();

                        var url = appSettings.Value.App.BaseUrl + "/order/store-redemption/" + item.OrderItemId;

                        var image = GenerateQRCodeHtmlImageSrc(appSettings.Value.App.VouponMerchantAppBaseUrl + "/qr/v/" + item.Token, 200, 200, 0);

                        msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.InStoreRedemption);
                        msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
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
                        //var result = await ThirdPartyRedemption(product);

                        //if (result != null)
                        //{
                        //    if (result.IsSuccessful)
                        //    {
                        //        item.TokenType = 2;
                        //        item.Token = result.Url;
                        //        _rewardsDBContext.SaveChanges();

                        //        var sendGridClient2 = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                        //        var msg2 = new SendGridMessage();
                        //        msg2.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.ThirdPartyRedemption);
                        //        msg2.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                        //        msg2.SetSubject("Vodus Redemption Url");
                        //        msg2.AddTo(new EmailAddress(email));
                        //        msg2.AddTo("merchant@vodus.my");

                        //        msg2.AddSubstitution("-customerName-", fullName);
                        //        msg2.AddSubstitution("-item-", product.Title);
                        //        msg2.AddSubstitution("-redemptionUrl-", result.Url);
                        //        msg2.AddSubstitution("-imageUrl-", product.ImageFolderUrl);
                        //        var response2 = sendGridClient2.SendEmailAsync(msg2).Result;
                        //        return msg;
                        //    }
                        //}
                        //return null;
                    }
                    else
                    {
                        item.Token = Voupon.Common.RedemptionTokenGenerator.InStoreRedemptionToken.GenerateToken(item.Id);
                        _rewardsDBContext.SaveChanges();

                        var url = appSettings.Value.App.BaseUrl + "/order/store-redemption/" + item.OrderItemId;

                        var image = GenerateQRCodeHtmlImageSrc(appSettings.Value.App.VouponMerchantAppBaseUrl + "/qr/v/" + item.Token, 200, 200, 0);

                        msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.InStoreRedemption);
                        msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
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

                    if (product.ThirdPartyTypeId.HasValue)
                    {
                        //var result = await ThirdPartyRedemption(product);

                        //if (result != null)
                        //{
                        //    if (result.IsSuccessful)
                        //    {
                        //        item.TokenType = 2;
                        //        item.Token = result.Url;
                        //        _rewardsDBContext.SaveChanges();

                        //        var sendGridClient2 = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                        //        var msg2 = new SendGridMessage();
                        //        msg2.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.ThirdPartyRedemption);
                        //        msg2.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                        //        msg2.SetSubject("Vodus Redemption Url");
                        //        msg2.AddTo(new EmailAddress(email));
                        //        msg2.AddTo("merchant@vodus.my");

                        //        msg2.AddSubstitution("-customerName-", fullName);
                        //        msg2.AddSubstitution("-item-", product.Title);
                        //        msg2.AddSubstitution("-redemptionUrl-", result.Url);
                        //        msg2.AddSubstitution("-imageUrl-", product.ImageFolderUrl);
                        //        var response2 = sendGridClient2.SendEmailAsync(msg2).Result;
                        //        return msg;
                        //    }
                        //}
                        //return null;
                    }
                    else
                    {
                        //    Send email?
                    }
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

        private async Task<GifteeResponseModel> ThirdPartyRedemption(Voupon.Database.Postgres.RewardsEntities.Products product)
        {
            var type = await _rewardsDBContext.ThirdPartyTypes.Where(x => x.Id == product.ThirdPartyTypeId).FirstOrDefaultAsync();

            if (type.Name.ToUpper() == "GIFTEE")
            {
                var thirdPartyProduct = await _rewardsDBContext.ThirdPartyProducts.Where(x => x.Id == product.ThirdPartyProductId).FirstOrDefaultAsync();
                if (thirdPartyProduct != null)
                {
                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(appSettings.Value.ThirdPartyRedemptions.Giftee.Url);
                        client.DefaultRequestHeaders.Authorization =
                            new AuthenticationHeaderValue("Bearer", appSettings.Value.ThirdPartyRedemptions.Giftee.Token);

                        var request = new GifteeRequestModel
                        {
                            campaign = appSettings.Value.ThirdPartyRedemptions.Giftee.CampaignName,
                            distributor = appSettings.Value.ThirdPartyRedemptions.Giftee.Distributor,
                            item_id = int.Parse(thirdPartyProduct.ExternalId),
                            request_code = DateTime.Now.Ticks.ToString()
                        };

                        var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                        var result = client.PostAsync(appSettings.Value.ThirdPartyRedemptions.Giftee.Url, content).Result;
                        string resultContent = result.Content.ReadAsStringAsync().Result;

                        dynamic testResult = JsonConvert.DeserializeObject(resultContent);

                        var gifteeResponseModel = new GifteeResponseModel();

                        if (testResult["egift_url"] != null)
                        {
                            var totalUrl = testResult["egift_url"].Count;
                            if (totalUrl > 0)
                            {
                                gifteeResponseModel.Url = testResult["egift_url"][0];
                                gifteeResponseModel.IsSuccessful = true;
                            }
                            gifteeResponseModel.RawResponse = testResult["egift_url"].ToString();
                            return gifteeResponseModel;
                        }
                        else
                        {
                            gifteeResponseModel.IsSuccessful = false;
                            gifteeResponseModel.Error = testResult["error_code"];
                            return gifteeResponseModel;
                        }
                    }
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
    }

    public class LogData
    {
        public CreateOrderCommandModel Request { get; set; }
        public string Email { get; set; }
        public int MasterMemberProfileId { get; set; }
        public int BeforeOrderPoints { get; set; }
        public int PendingOrderToBeDeductedPoints { get; set; }
        public int TotalUsedPoints { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
        public int AfterOrderPoints { get; set; }

    }

    public class GifteeResponseModel
    {
        public bool IsSuccessful { get; set; }
        public string Url { get; set; }

        public string RawResponse { get; set; }
        public string Error { get; set; }
    }

    public class GifteeRequestModel
    {
        public string campaign { get; set; }
        public string distributor { get; set; }

        public int item_id { get; set; }

        public string request_code { get; set; }
    }


    public class CreateOrderResponseViewModel
    {
        public Guid OrderId { get; set; }
        public int MasterMemberPoints { get; set; }

        public int OrderStatus { get; set; }
    }

    public partial class CreateOrderCommandModel
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
        [JsonProperty("ShippingCostModel")]
        public ShippingCostQuery ShippingCostModel { get; set; }
        public List<OrderShippingCostForPoductIdAndVariationIdModel> OrderShippingCosts { get; set; }
    }

    public partial class ProductList
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

    public partial class AdditionalDiscount
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


    public partial class Merchant
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public partial struct Value
    {
        public long? Integer;
        public string String;

        public static implicit operator Value(long Integer) => new Value { Integer = Integer };
        public static implicit operator Value(string String) => new Value { String = String };
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                ValueConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class DecodingChoiceConverter : JsonConverter
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

    internal class ValueConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Value) || t == typeof(Value?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    var integerValue = serializer.Deserialize<long>(reader);
                    return new Value { Integer = integerValue };
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new Value { String = stringValue };
            }
            throw new Exception("Cannot unmarshal type Value");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (Value)untypedValue;
            if (value.Integer != null)
            {
                serializer.Serialize(writer, value.Integer.Value);
                return;
            }
            if (value.String != null)
            {
                serializer.Serialize(writer, value.String);
                return;
            }
            throw new Exception("Cannot marshal type Value");
        }

        public static readonly ValueConverter Singleton = new ValueConverter();
    }

    internal class ParseStringConverter : JsonConverter
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

    public class CreateOrderCommandModel222
    {
        public Guid Id { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string Email { get; set; }
        public decimal TotalPrice { get; set; }
        public int TotalPoints { get; set; }
        public int TotalItems { get; set; }
        public byte OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }
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

        public List<CreateOrderItemModel> OrderItems { get; set; }
        public List<TempOrderItemModel> TempOrderItems { get; set; }
    }

    public class TempOrderItemModel
    {
        public int ProductId { get; set; }
        public int MerchantId { get; set; }
        public int Quantity { get; set; }

        public int Points { get; set; }
        public decimal Price { get; set; }
    }

    public class CreateOrderItemModel
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public int MerchantId { get; set; }
        public string MerchantDisplayName { get; set; }
        public decimal Commision { get; set; }
        public decimal Price { get; set; }
        public int Points { get; set; }
        public string ProductDetail { get; set; }
        public int ExpirationTypeId { get; set; }
        public string ProductTitle { get; set; }
        public string ProductImageFolderUrl { get; set; }
        public byte Status { get; set; }

        public Voupon.Database.Postgres.RewardsEntities.Orders Order { get; set; }
    }

    public class ExternalProductRequest
    {
        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public byte ExternalTypeId { get; set; }
    }
}
