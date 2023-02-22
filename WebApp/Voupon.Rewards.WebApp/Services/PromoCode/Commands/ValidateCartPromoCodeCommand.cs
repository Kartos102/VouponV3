using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Services.Cart.Commands;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.PromoCode.Commands
{
    public class ValidateCartPromoCodeCommand : IRequest<ApiResponseViewModel>
    {
        public decimal SubTotal { get; set; }
        public string PromoCode { get; set; }
        public int MasterMemberProfileId { get; set; }
        public string UserName { get; set; }

        [JsonProperty("productList")]
        public List<ProductList> ProductList { get; set; }

        public class ValidateCartPromoCodeCommandHandler : IRequestHandler<ValidateCartPromoCodeCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            private readonly VodusV2Context vodusV2Context;
            private readonly IOptions<AppSettings> appSettings;


            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public ValidateCartPromoCodeCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(ValidateCartPromoCodeCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                  
                    if (string.IsNullOrEmpty(request.PromoCode))
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Promo code is missing";
                        return apiResponseViewModel;
                    }

                    request.PromoCode = request.PromoCode.ToUpper();
                    var promo = await rewardsDBContext.PromoCodes.Where(x => x.PromoCode == request.PromoCode && x.Status == 1).FirstOrDefaultAsync();
                    if (promo == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Incorrect or invalid promo code";
                        return apiResponseViewModel;
                    }

                    //  Check if user qualified if its selected users only
                    if (promo.IsSelectedUserOnly)
                    {
                        var promoCodeSelectedUsers = await rewardsDBContext.PromoCodeSelectedUsers.Where(x => x.PromoCodeId == promo.Id && x.Email == request.UserName).FirstOrDefaultAsync();
                        if (promoCodeSelectedUsers == null)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Incorrect or invalid promo code";
                            return apiResponseViewModel;
                        }
                    }

                    var orders = await rewardsDBContext.Orders.AsTracking().Where(x => x.Email == request.UserName && x.OrderStatus == 2).ToListAsync();
                    if (orders.Where(x => x.PromoCodeId == promo.Id).Count() >= promo.TotalAllowedPerUser)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "You have reach the promo code allowed for this code or you are not qualified to use this promo code";
                        return apiResponseViewModel;
                    }

                    if (request.PromoCode.ToUpper() == "EVERYWEDNESDAY2XDISCOUNT")
                    {
                        if (DateTime.Now.DayOfWeek != DayOfWeek.Wednesday)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = $"The promo code \"{request.PromoCode}\" is only available on Wednesday";
                            return apiResponseViewModel;
                        }

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
                                apiResponseViewModel.Message = $"Promo code \"{request.PromoCode}\" cannot be use with cash voucher items";
                                break;
                            }
                        }
                    }
                    if (hasError)
                    {
                        return apiResponseViewModel;
                    }

                    if (request.SubTotal <= promo.MinSpend)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = $"The mininum spending required for this promo code \"{request.PromoCode}\" is RM{promo.MinSpend}";
                        return apiResponseViewModel;
                    }

                    decimal discountWithPromoCode = 0;

                    if (request.PromoCode.ToUpper() == "2XPROMO" || request.PromoCode.ToUpper() == "EVERYWEDNESDAY2XDISCOUNT")
                    {
                        foreach(var product in request.ProductList)
                        {
                            if (product.AdditionalDiscount != null)
                            {
                                decimal newMultiplier = 0;
                                decimal discountPc = 0;
                                if (product.Id == 0) {
                                    newMultiplier = promo.DiscountValue * product.AdditionalDiscount.ExternalItemDiscountPercentage;
                                    discountPc = product.AdditionalDiscount.ExternalItemDiscountPercentage;
                                }
                                else {
                                    newMultiplier = (decimal)(promo.DiscountValue * Convert.ToInt32( product.AdditionalDiscount.Value));
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
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = $"The promo code \"{request.PromoCode}\" is no longer available";
                            return apiResponseViewModel;
                        }

                        discountWithPromoCode = (request.SubTotal * promo.DiscountValue) / 100;
                        if (discountWithPromoCode > promo.MaxDiscountValue)
                        {
                            discountWithPromoCode = promo.MaxDiscountValue;
                        }


                    }
                   

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = $"\"{request.PromoCode}\" has been applied and you've got RM{discountWithPromoCode.ToString("0.00")} discount";
                    apiResponseViewModel.Data = discountWithPromoCode.ToString("0.00");
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "We are busy at the moment. Please try again later";
                    return apiResponseViewModel;
                }
            }

        }
    }

}
