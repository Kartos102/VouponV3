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

namespace Voupon.Rewards.WebApp.Services.Cart.Queries
{
    public class CartProductsListQuery : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
    }
    public class CartProductsListQueryHandler : IRequestHandler<CartProductsListQuery, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;


        public CartProductsListQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CartProductsListQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            List<CartProductModel> listCartProductModel = new List<CartProductModel>();
            try
            {
                var appConfig = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                int multiTireVpointThreshold = 3;
                if (appConfig != null)
                {
                    multiTireVpointThreshold = appConfig.MaxQuantityPerVPounts;
                }
                var cartProductsList = await rewardsDBContext.CartProducts.Include(x => x.AdditionalDiscount).Include(x => x.DealExpiration).Include(x => x.Variation).ThenInclude(y => y.VariationCombination).ThenInclude(z => z.OptionOne).Include(x => x.Merchant).Include(x => x.Product).Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId).ToListAsync();

                foreach (var cartProduct in cartProductsList)
                {
                    CartProductModel cartProductModel = new CartProductModel();
                    cartProductModel.OrderQuantity = cartProduct.OrderQuantity;
                    cartProductModel.AddedAt = cartProduct.CreatedAt;
                    cartProductModel.Id = cartProduct.Id;
                    cartProductModel.Title = cartProduct.Product.Title;
                    cartProductModel.ProductId = cartProduct.ProductId;
                    cartProductModel.TypeId = cartProduct.Product.DealTypeId.Value;
                    //cartProductModel.ProductCartPreviewSmallImage = cartProduct.ProductCartPreviewSmallImageUrl;
                    if (cartProduct.DealExpiration.TotalValidDays != null)
                    {
                        cartProductModel.DealExpiration = new DealExpiration()
                        {
                            ExpiredDate = cartProduct.DealExpiration.ExpiredDate.Value,
                            Name = cartProduct.DealExpiration.ExpirationType == null ? null : cartProduct.DealExpiration.ExpirationType.Name,
                            Id = cartProduct.DealExpiration.Id,
                            StartDate = cartProduct.DealExpiration.StartDate.Value,
                            TotalValidDays = cartProduct.DealExpiration.TotalValidDays.Value,
                            Type = cartProduct.DealExpiration.ExpirationTypeId.Value
                        };
                    }
                    else
                    {
                        cartProductModel.DealExpiration = new DealExpiration()
                        {
                            ExpiredDate = cartProduct.DealExpiration.ExpiredDate.Value,
                            Name = cartProduct.DealExpiration.ExpirationType == null ? null : cartProduct.DealExpiration.ExpirationType.Name,
                            Id = cartProduct.DealExpiration.Id,
                            StartDate = cartProduct.DealExpiration.StartDate.Value,
                            Type = cartProduct.DealExpiration.ExpirationTypeId.Value
                        };
                    }
                    if (cartProduct.Variation != null)
                    {
                        cartProductModel.Price = cartProduct.Variation.Price;
                        cartProductModel.DiscountedPrice = cartProduct.Variation.DiscountedPrice.Value;
                        cartProductModel.AvailableQuantity = cartProduct.Variation.AvailableQuantity;
                        cartProductModel.VariationId = cartProduct.Variation.Id;
                        cartProductModel.IsVariationProduct = true;
                        cartProductModel.VariationText = cartProduct.Variation.VariationCombination.OptionOne.Name;
                        if (cartProduct.Variation.VariationCombination.OptionTwoId != null)
                        {
                            var opton2 = await rewardsDBContext.VariationOptions.Where(x => x.Id == cartProduct.Variation.VariationCombination.OptionTwoId).FirstOrDefaultAsync();
                            cartProductModel.VariationText += ", " + opton2.Name;
                        }
                        cartProductModel.ProductCartPreviewSmallImage = cartProduct.Variation.VariationCombination.OptionOne.ImageUrl;
                    }
                    else if (cartProduct.Variation == null && cartProduct.IsVariationProduct && cartProduct.CartProductType == 4)
                    {
                        cartProductModel.Price = 0;
                        cartProductModel.DiscountedPrice = 0;
                        cartProductModel.AvailableQuantity = 0;
                        cartProductModel.CartProductType = 4;
                        cartProductModel.IsVariationProduct = cartProduct.IsVariationProduct;
                        cartProductModel.ProductCartPreviewSmallImage = "images/not-available-stamp.jpg";//"images/not-available-stamp.jpg"
                        if (cartProduct.IsVariationProduct)
                            cartProductModel.VariationText = cartProduct.VariationText;
                    }
                    else
                    {
                        cartProductModel.Price = cartProduct.Product.Price.Value;
                        cartProductModel.DiscountedPrice = cartProduct.Product.DiscountedPrice.Value;
                        cartProductModel.AvailableQuantity = cartProduct.Product.AvailableQuantity.Value;
                        cartProductModel.IsVariationProduct = false;
                    }
                    if (cartProduct.CartProductType != 4 && (cartProductModel.AvailableQuantity <= 0 || cartProduct.Product.IsActivated == false))
                    {
                        cartProductModel.CartProductType = 2;
                    }
                    else if (cartProduct.CartProductType == 3 && cartProduct.Product.IsActivated)
                    {
                        cartProductModel.CartProductType = 3;
                    }
                    else if (cartProduct.CartProductType != 4 && cartProduct.Product.IsActivated && cartProductModel.AvailableQuantity > 0)
                    {
                        cartProductModel.CartProductType = 1;
                    }
                    if (cartProduct.Product.IsDeleted == true)
                    {
                        cartProductModel.CartProductType = 4;
                    }
                    cartProductModel.Merchant = new CartMerchantDetails()
                    {
                        Id = cartProduct.Merchant.Id,
                        Name = cartProduct.Merchant.DisplayName.Replace("'", ""),
                        
                    };
                    cartProductModel.OrderQuantity = cartProduct.OrderQuantity;
                    if (cartProduct.AdditionalDiscount != null)
                    {
                        cartProductModel.AdditionalDiscount = new AdditionalDiscount()
                        {
                            Id = cartProduct.AdditionalDiscount.Id,
                            Name = cartProduct.AdditionalDiscount.Name,
                            PointsRequired = cartProduct.AdditionalDiscount.PointRequired,
                            Type = cartProduct.AdditionalDiscount.DiscountTypeId,
                        };
                        cartProductModel.PointsRequired = (int)(cartProduct.AdditionalDiscount.PointRequired * Math.Ceiling((decimal)(cartProduct.OrderQuantity / multiTireVpointThreshold)));
                        cartProductModel.SubTotal = cartProductModel.DiscountedPrice * cartProduct.OrderQuantity;

                        if (cartProduct.AdditionalDiscount.DiscountTypeId == 1)
                        {
                            cartProductModel.AdditionalDiscount.Value = cartProduct.AdditionalDiscount.PercentageValue;

                            if (appConfig.IsVPointsMultiplierEnabled)
                            {
                                if (!cartProductModel.Title.Contains("voucher"))
                                {
                                    if (cartProduct.AdditionalDiscount.PercentageValue > 0)
                                    {
                                        var newMultiplier = appConfig.VPointsMultiplier * cartProduct.AdditionalDiscount.PercentageValue;
                                        if (newMultiplier > appConfig.VPointsMultiplierCap)
                                        {
                                            cartProduct.AdditionalDiscount.PercentageValue = appConfig.VPointsMultiplierCap;

                                        }
                                        else if (newMultiplier <= appConfig.VPointsMultiplierCap)
                                        {
                                            cartProduct.AdditionalDiscount.PercentageValue = newMultiplier;
                                        }
                                        cartProductModel.AdditionalDiscount.Value = cartProduct.AdditionalDiscount.PercentageValue;

                                        cartProductModel.TotalPrice = cartProductModel.DiscountedPrice - ((cartProductModel.DiscountedPrice * (cartProduct.AdditionalDiscount.PercentageValue / 100)) * cartProduct.OrderQuantity);
                                        var totalDiscountPrice = ((cartProductModel.DiscountedPrice * cartProduct.AdditionalDiscount.PercentageValue) / 100) + (cartProductModel.Price - cartProductModel.DiscountedPrice);
                                        cartProductModel.DiscountRate = cartProduct.AdditionalDiscount.PercentageValue;

                                    }
                                    else
                                    {
                                        cartProductModel.TotalPrice = cartProductModel.DiscountedPrice - ((cartProductModel.DiscountedPrice * (cartProduct.AdditionalDiscount.PercentageValue / 100)) * cartProduct.OrderQuantity);
                                        var totalDiscountPrice = ((cartProductModel.DiscountedPrice * cartProduct.AdditionalDiscount.PercentageValue) / 100) + (cartProductModel.Price - cartProductModel.DiscountedPrice);
                                        cartProductModel.DiscountRate = (int)(totalDiscountPrice * 100 / cartProductModel.Price);
                                    }

                                }
                            }
                            else
                            {
                                cartProductModel.TotalPrice = cartProductModel.DiscountedPrice - ((cartProductModel.DiscountedPrice * (cartProduct.AdditionalDiscount.PercentageValue / 100)) * cartProduct.OrderQuantity);
                                var totalDiscountPrice = ((cartProductModel.DiscountedPrice * cartProduct.AdditionalDiscount.PercentageValue) / 100) + (cartProductModel.Price - cartProductModel.DiscountedPrice);
                                cartProductModel.DiscountRate = (int)(totalDiscountPrice * 100 / cartProductModel.Price);

                            }
                        }
                        else
                        {
                            cartProductModel.AdditionalDiscount.Value = cartProduct.AdditionalDiscount.PriceValue;
                            cartProductModel.TotalPrice = ((cartProductModel.DiscountedPrice - (cartProduct.AdditionalDiscount.PriceValue)) * cartProduct.OrderQuantity);
                            var totalDiscountPrice = (cartProduct.AdditionalDiscount.PriceValue) + (cartProductModel.Price - cartProductModel.DiscountedPrice);
                            cartProductModel.DiscountRate = (int)(totalDiscountPrice * 100 / cartProductModel.Price);
                        }
                    }
                    else
                    {
                        cartProductModel.SubTotal = cartProductModel.DiscountedPrice * cartProduct.OrderQuantity;
                        cartProductModel.TotalPrice = cartProductModel.SubTotal;
                    }

                    listCartProductModel.Add(cartProductModel);
                }


                var cartProductExternalList = await rewardsDBContext.CartProductExternal.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId).ToListAsync();

                foreach (var cartProduct in cartProductExternalList)
                {
                    CartProductModel cartProductModel = new CartProductModel();
                    cartProductModel.OrderQuantity = cartProduct.OrderQuantity;
                    cartProductModel.AddedAt = cartProduct.CreatedAt;
                    cartProductModel.Id = 0;
                    cartProductModel.Title = cartProduct.ProductTitle;
                    cartProductModel.ProductId = 0;
                    cartProductModel.TypeId = 2;
                    cartProductModel.ProductCartPreviewSmallImage = cartProduct.ProductCartPreviewSmallImageURL;
                    cartProductModel.VariationText = cartProduct.VariationText;
                    cartProductModel.ExternalItemId = cartProduct.ExternalItemId;
                    cartProductModel.ExternalShopId = cartProduct.ExternalShopId;
                    cartProductModel.ExternalTypeId = (byte)cartProduct.ExternalTypeId;
                    cartProductModel.Price = cartProduct.ProductPrice;
                    cartProductModel.DiscountedPrice = cartProduct.ProductDiscountedPrice;
                    cartProductModel.SubTotal = cartProduct.SubTotal;
                    cartProductModel.TotalPrice = cartProduct.TotalPrice;
                    cartProductModel.SubTotal = cartProduct.SubTotal;
                    cartProductModel.OrderQuantity = cartProduct.OrderQuantity;
                    cartProductModel.CartProductType = cartProduct.CartProductType;
                    cartProductModel.AvailableQuantity = 9999;
                    cartProductModel.ExternalId = cartProduct.Id.ToString();
                   


                    var merchant = await rewardsDBContext.Merchants.Where(x => x.ExternalId == cartProduct.ExternalShopId && x.ExternalTypeId == cartProduct.ExternalTypeId).FirstOrDefaultAsync();
                    var product = await rewardsDBContext.Products.Where(x => x.ExternalId == cartProduct.ExternalItemId && x.ExternalTypeId == cartProduct.ExternalTypeId).FirstOrDefaultAsync();
                    if(merchant != null)
                    {
                        cartProductModel.ExternalShopUrl = merchant.ExternalUrl;
                        cartProductModel.ExternalShopName = merchant.DisplayName;
                        if(product != null)
                        {
                            cartProductModel.ExternalItemUrl = product.ExternalUrl;
                            cartProductModel.Id = product.Id;
                        }
                    }
                   

                    if (cartProduct.AdditionalDiscountPointRequired.HasValue && cartProduct.AdditionalDiscountPriceValue.Value > 0)
                    {
                        cartProductModel.AdditionalDiscount = new AdditionalDiscount
                        {
                            PointsRequired = cartProduct.AdditionalDiscountPointRequired.Value,
                            Value = cartProduct.AdditionalDiscountPriceValue.Value,
                            Type = 1,
                            ExternalItemDiscountPercentage = (cartProduct.AdditionalDiscountPriceValue.HasValue ? cartProduct.AdditionalDiscountPriceValue.Value : 0),
                            VPointsMultiplier = (cartProduct.VPointsMultiplier != null ? cartProduct.VPointsMultiplier.Value : 0),
                            VPointsMultiplierCap = (cartProduct.VPointsMultiplierCap.HasValue ? cartProduct.VPointsMultiplierCap.Value : 0)
                        };
                    }

                    cartProductModel.Merchant = new CartMerchantDetails
                    {
                        Name = merchant.DisplayName,
                        ExternalId = cartProduct.ExternalShopId,
                        TypeId = cartProduct.ExternalTypeId
                        
                    };

                    listCartProductModel.Add(cartProductModel);
                }

                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Get Cart Products successfully";
                apiResponseViewModel.Data = listCartProductModel.OrderByDescending(x => x.AddedAt).ToList();
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Fail to Get Cart Products";
                return apiResponseViewModel;

            }
        }
    }
}
