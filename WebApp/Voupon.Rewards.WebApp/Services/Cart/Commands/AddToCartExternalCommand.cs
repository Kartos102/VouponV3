using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Voupon.Rewards.WebApp.Services.Cart.Models;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using Newtonsoft.Json;
using Voupon.Rewards.WebApp.Services.Logger;

namespace Voupon.Rewards.WebApp.Services.Cart.Commands
{
    public class AddToCartExternalCommand : CartProductModel, IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
    }
    public class AddToCartExternalCommandHandler : IRequestHandler<AddToCartExternalCommand, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;
        private IOptions<AppSettings> appSettings;

        public AddToCartExternalCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(AddToCartExternalCommand request, CancellationToken cancellationToken)
        {
            var cartProductExternal = new CartProductExternal();
            var apiResponseViewModel = new ApiResponseViewModel();

            try
            {
                cartProductExternal = await rewardsDBContext.CartProductExternal.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.ExternalItemId == request.ExternalItemId && x.ExternalShopId == request.ExternalShopId && x.ExternalTypeId == request.ExternalTypeId && x.VariationText == request.VariationText).FirstOrDefaultAsync();

                if (cartProductExternal != null)
                {
                    cartProductExternal.OrderQuantity = request.OrderQuantity;
                    cartProductExternal.SubTotal = cartProductExternal.ProductDiscountedPrice * request.OrderQuantity;
                    cartProductExternal.TotalPrice = cartProductExternal.SubTotal;

                    if (request.AdditionalDiscount != null && request.AdditionalDiscount.Value != 0)
                    {
                        cartProductExternal.AdditionalDiscountPointRequired = request.AdditionalDiscount.PointsRequired;
                        cartProductExternal.AdditionalDiscountPriceValue = request.AdditionalDiscount.Value;
                        cartProductExternal.AdditionalDiscountName = request.AdditionalDiscount.Name;
                        cartProductExternal.SubTotal = request.DiscountedPrice * request.OrderQuantity;
                        cartProductExternal.AdditionalDiscountPointRequired = request.AdditionalDiscount.PointsRequired;
                        cartProductExternal.TotalPrice = request.DiscountedPrice - ((request.DiscountedPrice * (request.AdditionalDiscount.Value / 100)) * cartProductExternal.OrderQuantity);

                        if (request.AdditionalDiscount.VPointsMultiplier > 0)
                        {
                            cartProductExternal.VPointsMultiplier = request.AdditionalDiscount.VPointsMultiplier;
                            cartProductExternal.VPointsMultiplierCap = request.AdditionalDiscount.VPointsMultiplierCap;
                        }

                    }
                    else
                    {
                        cartProductExternal.AdditionalDiscountPointRequired = null;
                        cartProductExternal.AdditionalDiscountPriceValue = null;
                        cartProductExternal.AdditionalDiscountName = null;

                        cartProductExternal.SubTotal = request.DiscountedPrice * request.OrderQuantity;
                        cartProductExternal.TotalPrice = cartProductExternal.SubTotal;
                    }

                    rewardsDBContext.Update(cartProductExternal);
                }
                else
                {
                    cartProductExternal = new CartProductExternal
                    {
                        Id = Guid.NewGuid()
                    };

                    cartProductExternal.OrderQuantity = request.OrderQuantity;
                    cartProductExternal.DealExpirationId = (request.DealExpiration != null ? request.DealExpiration.Id : 2);
                    cartProductExternal.MasterMemberProfileId = request.MasterMemberProfileId;
                    cartProductExternal.VariationText = request.VariationText;
                    cartProductExternal.ProductCartPreviewSmallImageURL = request.ProductCartPreviewSmallImage;
                    cartProductExternal.IsVariationProduct = request.IsVariationProduct;
                    cartProductExternal.CartProductType = request.CartProductType;
                    cartProductExternal.ExternalUrl = $"https://shopee.com.my/abc-i.{request.ExternalShopId}.{request.ExternalItemId}";
                    cartProductExternal.ExternalItemId = request.ExternalItemId;
                    cartProductExternal.ExternalShopId = request.ExternalShopId;
                    cartProductExternal.ExternalTypeId = request.ExternalTypeId;
                    cartProductExternal.ProductCartPreviewSmallImageURL = request.ProductCartPreviewSmallImage;
                    cartProductExternal.ProductTitle = request.Title;
                    cartProductExternal.CreatedAt = DateTime.Now;
                    cartProductExternal.ExternalShopName = request.Merchant.Name;
                    cartProductExternal.ProductPrice = request.Price;
                    cartProductExternal.ProductDiscountedPrice = request.DiscountedPrice;
                    //cartProductExternal.JsonData = "SOMEDATA";

                    if (request.AdditionalDiscount != null && request.AdditionalDiscount.Value != 0)
                    {
                        cartProductExternal.AdditionalDiscountPointRequired = request.AdditionalDiscount.PointsRequired;
                        cartProductExternal.AdditionalDiscountPriceValue = request.AdditionalDiscount.Value;
                        cartProductExternal.AdditionalDiscountName = request.AdditionalDiscount.Name;
                        cartProductExternal.SubTotal = request.DiscountedPrice * request.OrderQuantity;
                        cartProductExternal.AdditionalDiscountPointRequired = request.AdditionalDiscount.PointsRequired;
                        cartProductExternal.TotalPrice = request.DiscountedPrice - ((request.DiscountedPrice * (request.AdditionalDiscount.Value / 100)) * cartProductExternal.OrderQuantity);

                        if (request.AdditionalDiscount.VPointsMultiplier > 0)
                        {
                            cartProductExternal.VPointsMultiplier = request.AdditionalDiscount.VPointsMultiplier;
                            cartProductExternal.VPointsMultiplierCap = request.AdditionalDiscount.VPointsMultiplierCap;
                        }
                    }
                    cartProductExternal.SubTotal = request.DiscountedPrice * request.OrderQuantity;
                    cartProductExternal.TotalPrice = cartProductExternal.SubTotal;

                    await rewardsDBContext.CartProductExternal.AddAsync(cartProductExternal);
                }

                rewardsDBContext.SaveChanges();

                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                await new Logs
                {
                    Description = ex.ToString(),
                    MasterProfileId = request.MasterMemberProfileId,
                    JsonData = JsonConvert.SerializeObject(request),
                    ActionName = "AddToCartExternalCommand",
                    TypeId = CreateErrorLogCommand.Type.Service,
                    SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                    RewardsDBContext = rewardsDBContext
                }.Error();

                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Fail to Add to Cart";
                return apiResponseViewModel;

            }
        }
    }
}
