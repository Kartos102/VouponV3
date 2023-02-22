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
using Voupon.Rewards.WebApp.Services.Logger;
using Newtonsoft.Json;

namespace Voupon.Rewards.WebApp.Services.Cart.Commands
{
    public class AddToCartCommand : CartProductModel, IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
    }
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;
        private IOptions<AppSettings> appSettings;

        public AddToCartCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var cartProduct = new CartProducts();
            var apiResponseViewModel = new ApiResponseViewModel();

            try
            {
                if (!request.IsVariationProduct)
                {
                    cartProduct = await rewardsDBContext.CartProducts.Include(x => x.AdditionalDiscount).Include(x => x.DealExpiration).Include(x => x.Variation).Include(x => x.Merchant).Include(x => x.Product).Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.ProductId == request.ProductId).FirstOrDefaultAsync();
                }
                else
                {
                    cartProduct = await rewardsDBContext.CartProducts.Include(x => x.AdditionalDiscount).Include(x => x.DealExpiration).Include(x => x.Variation).Include(x => x.Merchant).Include(x => x.Product).Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.ProductId == request.ProductId && x.VariationId == request.VariationId).FirstOrDefaultAsync();
                }

                if (cartProduct != null)
                {
                    cartProduct.OrderQuantity = request.OrderQuantity;

                    if (request.AdditionalDiscount != null && request.AdditionalDiscount.Id != 0)
                    {
                        var additionalDiscountInfo = await rewardsDBContext.ProductDiscounts.Where(x => x.Id == request.AdditionalDiscount.Id).FirstOrDefaultAsync();
                        cartProduct.AdditionalDiscountId = request.AdditionalDiscount.Id;

                        if (!request.IsVariationProduct)
                        {
                            cartProduct.SubTotal = cartProduct.Product.DiscountedPrice.Value * cartProduct.OrderQuantity;

                            if (additionalDiscountInfo.DiscountTypeId == 1)
                            {
                                cartProduct.TotalPrice = cartProduct.Product.DiscountedPrice.Value - ((cartProduct.Product.DiscountedPrice.Value * (additionalDiscountInfo.PriceValue / 100)) * cartProduct.OrderQuantity);
                            }
                            else
                            {
                                cartProduct.TotalPrice = ((cartProduct.Product.DiscountedPrice.Value - (cartProduct.Product.DiscountedPrice.Value)) * cartProduct.OrderQuantity);
                            }
                        }
                        else
                        {
                            cartProduct.VariationId = request.VariationId;
                            cartProduct.SubTotal = cartProduct.Variation.DiscountedPrice.Value * cartProduct.OrderQuantity;

                            if (additionalDiscountInfo.DiscountTypeId == 1)
                            {
                                cartProduct.TotalPrice = cartProduct.Variation.DiscountedPrice.Value - ((cartProduct.Variation.DiscountedPrice.Value * (additionalDiscountInfo.PriceValue / 100)) * cartProduct.OrderQuantity);
                            }
                            else
                            {
                                cartProduct.TotalPrice = ((cartProduct.Variation.DiscountedPrice.Value - (cartProduct.Variation.DiscountedPrice.Value)) * cartProduct.OrderQuantity);
                            }
                        }
                    }
                    else
                    {
                        if (!request.IsVariationProduct)
                        {
                            cartProduct.SubTotal = cartProduct.Product.DiscountedPrice.Value * cartProduct.OrderQuantity;
                            cartProduct.TotalPrice = cartProduct.SubTotal;
                        }
                        else
                        {
                            cartProduct.SubTotal = cartProduct.Variation.DiscountedPrice.Value * cartProduct.OrderQuantity;
                            cartProduct.TotalPrice = cartProduct.SubTotal;
                        }
                        cartProduct.AdditionalDiscount = null;
                    }
                    cartProduct.UpdatedAt = DateTime.Now;
                    rewardsDBContext.Update(cartProduct);
                }
                else
                {
                    cartProduct = new CartProducts();
                    var productInfo = await rewardsDBContext.Products.Where(x => x.Id == request.ProductId).FirstOrDefaultAsync();
                    var variationInfo = await rewardsDBContext.ProductVariation.Where(x => x.Id == request.VariationId).FirstOrDefaultAsync();
                    cartProduct.OrderQuantity = request.OrderQuantity;
                    cartProduct.MerchantId = request.Merchant.Id;
                    cartProduct.DealExpirationId = request.DealExpiration.Id;
                    cartProduct.ProductId = request.ProductId;
                    cartProduct.VariationId = request.VariationId;
                    cartProduct.MasterMemberProfileId = request.MasterMemberProfileId;
                    cartProduct.VariationText = request.VariationText;

                    cartProduct.IsVariationProduct = request.IsVariationProduct;
                    cartProduct.CartProductType = request.CartProductType;

                    if (string.IsNullOrEmpty(request.ProductCartPreviewSmallImage))
                    {
                        if (!request.IsVariationProduct)
                        {
                            var existingProduct = await rewardsDBContext.Products.Where(x => x.Id == request.ProductId).FirstOrDefaultAsync();

                            if (string.IsNullOrEmpty(existingProduct.ImageFolderUrl))
                            {
                                cartProduct.ProductCartPreviewSmallImageURL = existingProduct.ImageFolderUrl.Replace("http://", "https://").Replace(":80", "");
                            }
                        }
                        else
                        {
                            //  Use placeholder since there is no image found
                            cartProduct.ProductCartPreviewSmallImageURL = "https://vodus.com/Content/images/Vodus-V3-Logo-Small.svg";
                        }
                    }
                    else
                    {
                        cartProduct.ProductCartPreviewSmallImageURL = request.ProductCartPreviewSmallImage;
                    }

                    if (request.AdditionalDiscount != null && request.AdditionalDiscount.Id != 0)
                    {
                        var additionalDiscountInfo = await rewardsDBContext.ProductDiscounts.Where(x => x.Id == request.AdditionalDiscount.Id).FirstOrDefaultAsync();
                        cartProduct.AdditionalDiscountId = request.AdditionalDiscount.Id;
                        if (!request.IsVariationProduct)
                        {
                            cartProduct.SubTotal = productInfo.DiscountedPrice.Value * cartProduct.OrderQuantity;

                            if (additionalDiscountInfo.DiscountTypeId == 1)
                            {
                                cartProduct.TotalPrice = productInfo.DiscountedPrice.Value - ((productInfo.DiscountedPrice.Value * (additionalDiscountInfo.PriceValue / 100)) * cartProduct.OrderQuantity);
                            }
                            else
                            {
                                cartProduct.TotalPrice = ((productInfo.DiscountedPrice.Value - (productInfo.DiscountedPrice.Value)) * cartProduct.OrderQuantity);
                            }
                        }
                        else
                        {
                            cartProduct.VariationId = request.VariationId;
                            cartProduct.SubTotal = variationInfo.DiscountedPrice.Value * cartProduct.OrderQuantity;
                            if (additionalDiscountInfo != null)
                            {
                                if (additionalDiscountInfo.DiscountTypeId == 1)
                                {
                                    cartProduct.TotalPrice = variationInfo.DiscountedPrice.Value - ((variationInfo.DiscountedPrice.Value * (additionalDiscountInfo.PriceValue / 100)) * cartProduct.OrderQuantity);
                                }
                                else
                                {
                                    cartProduct.TotalPrice = (variationInfo.DiscountedPrice.Value * cartProduct.OrderQuantity);
                                }
                            }
                            else
                            {
                                cartProduct.TotalPrice = (variationInfo.DiscountedPrice.Value * cartProduct.OrderQuantity);
                            }
                        }
                    }
                    else
                    {
                        if (!request.IsVariationProduct)
                        {
                            cartProduct.SubTotal = productInfo.DiscountedPrice.Value * cartProduct.OrderQuantity;
                            cartProduct.TotalPrice = cartProduct.SubTotal;
                        }
                        else
                        {
                            if (variationInfo == null)
                            {
                                cartProduct.SubTotal = productInfo.DiscountedPrice.Value * cartProduct.OrderQuantity;
                                cartProduct.TotalPrice = cartProduct.SubTotal;
                            }
                            else
                            {
                                cartProduct.SubTotal = variationInfo.DiscountedPrice.Value * cartProduct.OrderQuantity;
                                cartProduct.TotalPrice = cartProduct.SubTotal;
                            }

                        }
                    }
                    cartProduct.CreatedAt = DateTime.Now;
                    if (cartProduct.AdditionalDiscountId == 0)
                        cartProduct.AdditionalDiscountId = null;
                    if (cartProduct.VariationId == 0)
                        cartProduct.VariationId = null;

                    if(cartProduct.DealExpiration != null)
                    {
                        if(cartProduct.DealExpiration.Id == 0)
                        {
                            cartProduct.DealExpiration = null;
                        }
                    }
                    await rewardsDBContext.CartProducts.AddAsync(cartProduct);
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
                    ActionName = "AddToCartCommand",
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
