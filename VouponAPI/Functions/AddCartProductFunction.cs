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

namespace Voupon.API.Functions
{
    public class AddCartProductFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private int _masterMemberProfileId;


        public AddCartProductFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
        }

        [OpenApiOperation(operationId: "Add cart products", tags: new[] { "Cart" }, Description = "Add cart products", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AddCartProductResponseModel), Summary = "Get cart products")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to add to cart")]
        [OpenApiRequestBody("application/json", typeof(AddCartRequestModel), Description = "JSON request body ")]

        [FunctionName("AddCartProductFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cart/add")] HttpRequest req, ILogger log)
        {
            var response = new AddCartProductResponseModel()
            {
                Data = new AddCartProductData()
            };
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            _masterMemberProfileId = auth.MasterMemberProfileId;

            try
            {
                var request = HttpRequestHelper.DeserializeModel<AddCartRequestModel>(req);
                string message = "";
                if (request.ProductId == 0)
                {
                    message = await AddToCartExternal(request);
                }
                else 
                {
                    message = await AddToCart(request);
                }
                

                if (!string.IsNullOrEmpty(message))
                {
                    response.Code = -1;
                    response.ErrorMessage = message;
                    return new BadRequestObjectResult(response);

                }

                response.Code = 0;
                response.Data.Message = "Successfully added to cart";
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {

                response.Code = -1;
                response.ErrorMessage = "Fail to Add to Cart";
                return new BadRequestObjectResult(response);
            }

        }



        private async Task<string> AddToCart(AddCartRequestModel request) 
        {
            CartProducts cartProduct = new CartProducts();
            if (!request.IsVariationProduct)
            {
                cartProduct = await rewardsDBContext.CartProducts.Include(x => x.AdditionalDiscount).Include(x => x.DealExpiration).Include(x => x.Variation).Include(x => x.Merchant).Include(x => x.Product).Where(x => x.MasterMemberProfileId == _masterMemberProfileId && x.ProductId == request.ProductId).FirstOrDefaultAsync();
            }
            else
            {
                cartProduct = await rewardsDBContext.CartProducts.Include(x => x.AdditionalDiscount).Include(x => x.DealExpiration).Include(x => x.Variation).Include(x => x.Merchant).Include(x => x.Product).Where(x => x.MasterMemberProfileId == _masterMemberProfileId && x.ProductId == request.ProductId && x.VariationId == request.VariationId).FirstOrDefaultAsync();
            }
            string message = "";
            if (cartProduct != null)
            {
                message = await UpdateCart(cartProduct, request);
            }
            else
            {
                message = await CreateCart(request);
            }
            rewardsDBContext.SaveChanges();


            return message;
        }
        private async Task<string> AddToCartExternal(AddCartRequestModel request) 
        {
 
                var cartProductExternal = await rewardsDBContext.CartProductExternal.
                Where(x => x.MasterMemberProfileId == _masterMemberProfileId && x.ExternalItemId == request.ExternalItemId && x.ExternalShopId == request.ExternalShopId && x.ExternalTypeId == request.ExternalTypeId && x.VariationText == request.VariationText).
                FirstOrDefaultAsync();

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
                    cartProductExternal.DealExpirationId = (request.DealExpirationId != 0 ? request.DealExpirationId : 2);
                    cartProductExternal.MasterMemberProfileId = _masterMemberProfileId;
                    cartProductExternal.VariationText = request.VariationText;
                    cartProductExternal.ProductCartPreviewSmallImageURL = request.ProductCartPreviewSmallImage;
                    cartProductExternal.IsVariationProduct = request.IsVariationProduct;
                    cartProductExternal.CartProductType = request.CartProductType;
                    cartProductExternal.ExternalUrl = $"https://shopee.com.my/abc-i.{request.ExternalShopId}.{request.ExternalItemId}";
                    cartProductExternal.ExternalItemId = request.ExternalItemId;
                    cartProductExternal.ExternalShopId = request.ExternalShopId;
                    cartProductExternal.ExternalTypeId = (byte) request.ExternalTypeId;
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
                return null;
        }
        private async Task<string> CreateCart(AddCartRequestModel request) 
        {
            CartProducts cartProduct = new CartProducts();
            var productInfo = await rewardsDBContext.Products.Where(x => x.Id == request.ProductId).FirstOrDefaultAsync();
            var variationInfo = await rewardsDBContext.ProductVariation.Where(x => x.Id == request.VariationId).FirstOrDefaultAsync();
            cartProduct.OrderQuantity = request.OrderQuantity;
            cartProduct.MerchantId = request.Merchant.Id;
            cartProduct.DealExpirationId = request.DealExpirationId;
            cartProduct.ProductId = request.ProductId;
            cartProduct.VariationId = request.VariationId;
            cartProduct.MasterMemberProfileId = _masterMemberProfileId;
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

            try
            {
                await rewardsDBContext.CartProducts.AddAsync(cartProduct);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }

        private async Task<string> UpdateCart(CartProducts cartProduct,AddCartRequestModel request) 
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
            try
            {
                rewardsDBContext.Update(cartProduct);
            }
            catch (Exception ex)
            { 
                return ex.Message;
            }
            


            return null;
        }
        private class AddCartRequestModel {
            public int Id { get; set; }
            public string ProductCartPreviewSmallImage { get; set; }
            public decimal DiscountedPrice { get; set; }
            public decimal DiscountRate { get; set; }
            public decimal Price { get; set; }
            public decimal SubTotal { get; set; }
            public decimal TotalPrice { get; set; }
            public int AvailableQuantity { get; set; }
            public int OrderQuantity { get; set; }
            public int PointsRequired { get; set; }
            public bool IsVariationProduct { get; set; }
            public string Title { get; set; }
            public int TypeId { get; set; }
            public int CartProductType { get; set; }
            public string VariationText { get; set; }
            public int ProductId { get; set; }
            public int VariationId { get; set; }
            public MerchantDetail Merchant { get; set; }
            public AdditionalDiscount AdditionalDiscount { get; set; }
            public int DealExpirationId { get; set; }

            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public int ExternalTypeId { get; set; }

            public DateTime AddedAt { get; set; }

        }

        private class AddCartProductResponseModel : ApiResponseViewModel
        {
            public AddCartProductData Data { get; set; }
        }

        private class AddCartProductData
        {
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }
        private class MerchantDetail  
        {
            public int Id { get; set; }
            public string Name { get; set; }
           

            public string ExternalId { get; set; }
            public string ExternalShopName { get; set; }

            public string ExternalShopUrl { get; set; }
            public string ExternalItemUrl { get; set; }
        }

        public class DealExpiration
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime ExpiredDate { get; set; }
            public int Type { get; set; }
            public int? TotalValidDays { get; set; }
        }

        public class AdditionalDiscount
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int PointsRequired { get; set; }
            public int Type { get; set; }
            public decimal Value { get; set; }

            public decimal ExternalItemDiscountPercentage { get; set; }

            public decimal VPointsMultiplier { get; set; }
            public decimal VPointsMultiplierCap { get; set; }
        }
    }
}