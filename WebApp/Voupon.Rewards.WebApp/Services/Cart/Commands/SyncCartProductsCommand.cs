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
using Voupon.Rewards.WebApp.Services.Cart.Models;

namespace Voupon.Rewards.WebApp.Services.Cart.Commands
{
    public class SyncCartProductsCommand : CartProductModel, IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
    }
    public class SyncCartProductsCommandHandler : IRequestHandler<SyncCartProductsCommand, ApiResponseViewModel>
    {
        private readonly VodusV2Context vodusV2Context;
        private readonly RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        private readonly IAzureBlobStorage azureBlobStorage;

        public SyncCartProductsCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.appSettings = appSettings;
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(SyncCartProductsCommand request, CancellationToken cancellationToken)
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

                    if (cartProduct.AdditionalDiscount != null)
                    {

                        var additionalDiscountInfo = await rewardsDBContext.ProductDiscounts.Where(x => x.Id == request.AdditionalDiscount.Id).FirstOrDefaultAsync();
                        cartProduct.AdditionalDiscountId = request.Id;

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
                    }
                    rewardsDBContext.Update(cartProduct);
                }
                else
                {
                    cartProduct = new CartProducts();
                    var productInfo = await rewardsDBContext.Products.Where(x => x.Id == request.ProductId).FirstOrDefaultAsync();
                    //var dealExpirationInfo = rewardsDBContext.DealExpirations.Where(x => x.Id == request.DealExpiration.Id).FirstOrDefault();
                    var variationInfo = await rewardsDBContext.ProductVariation.Where(x => x.Id == request.VariationId).FirstOrDefaultAsync();
                    cartProduct.OrderQuantity = request.OrderQuantity;
                    cartProduct.MerchantId = request.Merchant.Id;
                    cartProduct.DealExpirationId = request.DealExpiration.Id;
                    cartProduct.ProductId = request.ProductId;

                    if (request.AdditionalDiscount != null)
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

                            if (additionalDiscountInfo.DiscountTypeId == 1)
                            {
                                cartProduct.TotalPrice = variationInfo.DiscountedPrice.Value - ((variationInfo.DiscountedPrice.Value * (additionalDiscountInfo.PriceValue / 100)) * cartProduct.OrderQuantity);
                            }
                            else
                            {
                                cartProduct.TotalPrice = ((variationInfo.DiscountedPrice.Value - (variationInfo.DiscountedPrice.Value)) * cartProduct.OrderQuantity);
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
                            cartProduct.SubTotal = variationInfo.DiscountedPrice.Value * cartProduct.OrderQuantity;
                            cartProduct.TotalPrice = cartProduct.SubTotal;
                        }
                    }
                    rewardsDBContext.CartProducts.Add(cartProduct);
                }

                rewardsDBContext.SaveChanges();

                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Fail to Add to Cart";
                return apiResponseViewModel;

            }
        }
    }
}
