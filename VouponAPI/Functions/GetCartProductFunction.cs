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
using static Voupon.API.Functions.GetCartProductFunction;

namespace Voupon.API.Functions
{
    public class GetCartProductFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly VodusV2Context _vodusV2Context;
        private int _masterMemberProfileId;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetCartProductFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this._rewardsDBContext = rewardsDBContext;
            this._vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get cart products", tags: new[] { "Cart" }, Description = "Get cart products", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CartProductsResponseModel), Summary = "Get cart products")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]

        [FunctionName("GetCartProductFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cart/products")] HttpRequest req, ILogger log)
        {

            var response = new CartProductsResponseModel
            {
                Data = new CartProductData()
                {
                    TotalItemCount = 0
                }
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

                var cartProductsList = await GetCartProducts();
                var cartExternalProductList = await GetCartExternalProducts();

                List<MerchantProduct> cartProducts = new List<MerchantProduct>();

                cartProducts.AddRange(cartProductsList);
                cartProducts.AddRange(cartExternalProductList);
                if ( cartProducts == null && !cartProductsList.Any())
                {
                    return new OkObjectResult(response);
                }


                cartProducts = cartProducts.OrderByDescending(x => x.Latest).ToList();
                response.Code = 0;
                response.Data.TotalItemCount = cartProducts.Sum(x => x.TotalItemMerchant);
                response.Data.Shops = cartProducts;
                return new OkObjectResult(response);

            }
            catch (Exception ex)
            {
                response.ErrorMessage = "Fail to get cart products";
                return new BadRequestObjectResult(response);
            }
        }

        private async Task<List<MerchantProduct>> GetCartProducts()
        {
            //var cartProductList = new List<CartProducts>();
            var appConfig = await _rewardsDBContext.AppConfig.FirstOrDefaultAsync();
            int multiTireVpointThreshold = 3;
            if (appConfig != null)
            {
                multiTireVpointThreshold = appConfig.MaxQuantityPerVPounts;
            }
            var cartProductsList = await _rewardsDBContext.CartProducts.
                Include(x => x.AdditionalDiscount).
                Include(x => x.DealExpiration).
                Include(x => x.Variation).
                ThenInclude(y => y.VariationCombination).
                ThenInclude(z => z.OptionOne).
                Include(x => x.Merchant).
                Include(x => x.Product).
                Where(x => x.MasterMemberProfileId == _masterMemberProfileId).ToListAsync();

            var cartMerchat = cartProductsList.GroupBy(x => x.MerchantId);

            List<MerchantProduct> cartProducts = new List<MerchantProduct>();
            foreach (var merchant in cartMerchat)
            {
                MerchantProduct cp = new MerchantProduct();
                cp.MerchantId = merchant.Key;
                cp.Name = merchant.FirstOrDefault().Merchant.DisplayName;
                //cp.TypeId = merchant.FirstOrDefault().Merchant.BusinessTypeId.Value;
                cp.Products = new List<Product>();
                foreach (var product in merchant) 
                {
                    Product prod = new Product();
                    prod.OrderQuantity = product.OrderQuantity;
                    prod.AddedAt = product.CreatedAt;
                    prod.Id = product.Id;
                    prod.Title = product.Product.Title;
                    prod.ProductId = product.ProductId;
                    prod.TypeId = product.Product.DealTypeId.Value;

                    if (product.DealExpiration != null) 
                    {
                        prod.DealExpiration = new DealExpiration()
                        {
                            ExpiredDate = product.DealExpiration.ExpiredDate.Value,
                            Name = product.DealExpiration.ExpirationType == null ? null : product.DealExpiration.ExpirationType.Name,
                            Id = product.DealExpiration.Id,
                            StartDate = product.DealExpiration.StartDate.Value,
                            TotalValidDays = product.DealExpiration.TotalValidDays.HasValue ? product.DealExpiration.TotalValidDays.Value : 0,
                            Type = product.DealExpiration.ExpirationTypeId.Value
                        };
                    }

                    if (product.Variation != null)
                    {
                        prod.Price = product.Variation.Price;
                        prod.DiscountedPrice = product.Variation.DiscountedPrice.Value;
                        prod.AvailableQuantity = product.Variation.AvailableQuantity;
                        prod.VariationId = product.Variation.Id;
                        prod.IsVariationProduct = true;
                        prod.VariationText = product.Variation.VariationCombination.OptionOne.Name;
                        if (product.Variation.VariationCombination.OptionTwoId != null)
                        {
                            var opton2 = await _rewardsDBContext.VariationOptions.Where(x => x.Id == product.Variation.VariationCombination.OptionTwoId).FirstOrDefaultAsync();
                            prod.VariationText += ", " + opton2.Name;
                        }
                        prod.ProductCartPreviewSmallImage = await GetFromBlob(product.Id);
                    }
                    else if (product.Variation == null && product.IsVariationProduct && product.CartProductType == 4)
                    {
                        prod.Price = 0;
                        prod.DiscountedPrice = 0;
                        prod.AvailableQuantity = 0;
                        //prod.CartProductType = 4;
                        prod.IsVariationProduct = product.IsVariationProduct;
                        prod.ProductCartPreviewSmallImage = "images/not-available-stamp.jpg";//"images/not-available-stamp.jpg"
                        if (product.IsVariationProduct)
                            product.VariationText = product.VariationText;
                    }
                    else
                    {
                        prod.Price = product.Product.Price.Value;
                        prod.DiscountedPrice = product.Product.DiscountedPrice.Value;
                        prod.AvailableQuantity = product.Product.AvailableQuantity.Value;
                        prod.IsVariationProduct = false;
                    }

                    if (product.AdditionalDiscount != null)
                    {
                        prod.AdditionalDiscount = new AdditionalDiscount()
                        {
                            Id = product.AdditionalDiscount.Id,
                            Name = product.AdditionalDiscount.Name,
                            PointsRequired = product.AdditionalDiscount.PointRequired,
                            Type = product.AdditionalDiscount.DiscountTypeId,
                        };
                        prod.PointsRequired = (int)(product.AdditionalDiscount.PointRequired * Math.Ceiling((decimal)(product.OrderQuantity / multiTireVpointThreshold)));
                        prod.SubTotal = prod.DiscountedPrice * product.OrderQuantity;

                        var totalDiscountPrice = ((prod.DiscountedPrice * product.AdditionalDiscount.PercentageValue) / 100) + (prod.Price - prod.DiscountedPrice);
                        prod.TotalPrice = prod.DiscountedPrice - ((prod.DiscountedPrice * (product.AdditionalDiscount.PercentageValue / 100)) * product.OrderQuantity);
                        if (product.AdditionalDiscount.DiscountTypeId == 1)
                        {
                            prod.AdditionalDiscount.Value = product.AdditionalDiscount.PercentageValue;

                            if (appConfig.IsVPointsMultiplierEnabled)
                            {

                                if (!prod.Title.Contains("voucher"))
                                {
                                    
                                    if (product.AdditionalDiscount.PercentageValue > 0)
                                    {
                                        var newMultiplier = appConfig.VPointsMultiplier * product.AdditionalDiscount.PercentageValue;
                                        if (newMultiplier > appConfig.VPointsMultiplierCap)
                                        {
                                            product.AdditionalDiscount.PercentageValue = appConfig.VPointsMultiplierCap;

                                        }
                                        else if (newMultiplier <= appConfig.VPointsMultiplierCap)
                                        {
                                            product.AdditionalDiscount.PercentageValue = newMultiplier;
                                        }
                                        prod.AdditionalDiscount.Value = product.AdditionalDiscount.PercentageValue;
                                        prod.DiscountRate = product.AdditionalDiscount.PercentageValue;
                                    }
                                    else 
                                    {
                                        prod.DiscountRate = (int)(totalDiscountPrice * 100 / prod.Price);
                                    }
                                }
                            }
                            else
                            {
                                prod.TotalPrice = prod.DiscountedPrice - ((prod.DiscountedPrice * (product.AdditionalDiscount.PercentageValue / 100)) * product.OrderQuantity);
                                prod.DiscountRate = (int)(totalDiscountPrice * 100 / prod.Price);

                            }
                        }
                        else
                        {
                            prod.DiscountRate = (int)(totalDiscountPrice * 100 / prod.Price);
                        }
                    }
                    else
                    {
                        prod.SubTotal = prod.DiscountedPrice * product.OrderQuantity;
                        prod.TotalPrice = prod.SubTotal;
                    }
                    
                    cp.Products.Add(prod);
                }

                cp.Products = cp.Products.OrderByDescending(x => x.AddedAt).ToList();
                cp.TotalItemMerchant = cp.Products.Count;
                cp.Latest = cp.Products.FirstOrDefault().AddedAt;
                cartProducts.Add(cp);

            }


            return cartProducts;

        }


        private async Task<List<MerchantProduct>> GetCartExternalProducts() 
        {
            var cartProductExternalList = await _rewardsDBContext.CartProductExternal.Where(x => x.MasterMemberProfileId == _masterMemberProfileId).ToListAsync();
            var cartMerchat = cartProductExternalList.GroupBy(x => x.ExternalShopId);


            var _aggregatorUrl = "";
            var aggregatorUrl = await _vodusV2Context.AggregatorApiUrls.AsNoTracking().ToListAsync();
            if (bool.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.USE_LOCAL_AGGREGATOR)) == true)
            {
                _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true && x.Status == 1).FirstOrDefault().Url;
            }
            else
            {
                _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == false && x.Status == 1).OrderBy(x => x.LastUpdatedAt).First().Url;
            }

            List<MerchantProduct> externalList = new List<MerchantProduct>();
            foreach (var merchant in cartMerchat)
            {
                MerchantProduct cp = new MerchantProduct();
                cp.Name = merchant.FirstOrDefault().ExternalShopName.Replace("'", "");
                
                cp.TypeId = merchant.FirstOrDefault().ExternalTypeId;
                cp.ExternalShopId = merchant.FirstOrDefault().ExternalShopId;
                cp.Products = new List<Product>();

               
                foreach (var product in merchant)
                {
                    
                    Product prod = new Product();
                    prod.ExternalId = product.Id;
                    prod.OrderQuantity = product.OrderQuantity;
                    prod.AddedAt = product.CreatedAt;
                    prod.Id = 0;
                    prod.Title = product.ProductTitle;
                    prod.ProductId = 0;
                    prod.TypeId = 2;
                    prod.ProductCartPreviewSmallImage = product.ProductCartPreviewSmallImageURL;
                    prod.VariationText = product.VariationText;
                    prod.ExternalItemId = product.ExternalItemId;
                    prod.Price = product.ProductPrice;
                    prod.DiscountedPrice = product.ProductDiscountedPrice;
                    prod.SubTotal = product.SubTotal;
                    prod.TotalPrice = product.TotalPrice;
                    prod.SubTotal = product.SubTotal;
                    prod.OrderQuantity = product.OrderQuantity;
                    if (product.AdditionalDiscountPointRequired.HasValue && product.AdditionalDiscountPriceValue.Value > 0)
                    {
                        prod.AdditionalDiscount = new AdditionalDiscount
                        {
                            PointsRequired = product.AdditionalDiscountPointRequired.Value,
                            Name = product.AdditionalDiscountName,
                            Value = product.AdditionalDiscountPriceValue.Value,
                            Type = 1,
                            ExternalItemDiscountPercentage = (product.AdditionalDiscountPriceValue.HasValue ? product.AdditionalDiscountPriceValue.Value : 0),
                            VPointsMultiplier = (product.VPointsMultiplier != null ? product.VPointsMultiplier.Value : 0),
                            VPointsMultiplierCap = (product.VPointsMultiplierCap.HasValue ? product.VPointsMultiplierCap.Value : 0)
                        };
                    }


                    StringContent httpContent = new StringContent(JsonConvert.SerializeObject(new AggRequest()
                    {
                        ExternalItemId = product.ExternalItemId,
                        ExternalShopId = product.ExternalShopId,
                        ExternalTypeId = product.ExternalTypeId 

                    }), System.Text.Encoding.UTF8, "application/json"); ;


                    var httpClient = new HttpClient();
                    var result = await httpClient.PostAsync($"{_aggregatorUrl}/v1/product", httpContent);
                    var resultString = await result.Content.ReadAsStringAsync();
                    var crawlerResult = JsonConvert.DeserializeObject<LegacyApiResponseViewModel>(resultString);
                    if (!crawlerResult.Successful)
                    {
                        continue;
                    }

                    var productData = JsonConvert.DeserializeObject<DetailPageViewModel>(crawlerResult.Data.ToString());


                    //var productData = (DetailPageViewModel)JsonConvert.DeserializeObject<DetailPageViewModel>(productResult.Data.ToString());

                    if (!string.IsNullOrEmpty(product.VariationText))
                    {
                        var variationText = product.VariationText.Split(",");
                        //var a = variationText.Length;
                        if (variationText.Length == 1)
                        {
                            var variationItem = productData.VariationModel.VariationList.SelectMany(x => x.VariationOptions).Where(x => x.Name.ToLower() == variationText[0].ToLower()).FirstOrDefault();
                            if (variationItem == null)
                            {
                                prod.AvailableQuantity = productData.VariationModel.ProductVariationDetailsList.Where(x => x.Order == variationItem.Order.ToString()).FirstOrDefault().AvailableQuantity;
                            }
                        }
                        else
                        {
                            var variationItem = productData.VariationModel.VariationList.SelectMany(x => x.VariationOptions).Where(x => x.Name.ToLower() == variationText[0].ToLower().Trim()).FirstOrDefault();
                            var variationItem2 = productData.VariationModel.VariationList.SelectMany(x => x.VariationOptions).Where(x => x.Name.ToLower() == variationText[1].ToLower().Trim()).FirstOrDefault();


                            if (variationItem != null && variationItem2 != null)
                            {
                                var order = variationItem.Order.ToString() + "," + variationItem2.Order.ToString();
                                prod.AvailableQuantity = productData.VariationModel.ProductVariationDetailsList.Where(x => x.Order == order).FirstOrDefault().AvailableQuantity;

                            }

                        }
                    }
                    else
                    {
                        prod.AvailableQuantity = (productData.MaxPurchaseLimit > 0 ? productData.MaxPurchaseLimit : (productData.AvailableQuantity.HasValue ? productData.AvailableQuantity.Value : 0));
                    }

                    prod.ExternalItemUrl = productData.ExternalItemUrl;
                    cp.ExternalShopUrl = productData.ExternalShopUrl;
                    cp.Products.Add(prod);
                }

                cp.Products =  cp.Products.OrderByDescending(x => x.AddedAt).ToList();
                cp.TotalItemMerchant = cp.Products.Count;
                cp.Latest = cp.Products.FirstOrDefault().AddedAt;
                externalList.Add(cp);
            }
            return externalList;
        }

        public class CartProductsResponseModel : ApiResponseViewModel
        {
            public CartProductData Data { get; set; }
        }

        public class CartProductData
        {
            public int TotalItemCount { get; set; }
            public List<MerchantProduct> Shops { get; set; }
        }

        public class MerchantProduct
        {
            public int MerchantId { get; set; }
            public string Name { get; set; }

            public int TypeId { get; set; }

            public string ExternalShopId { get; set; }

            public string ExternalShopUrl { get; set; }

            public List<Product> Products { get; set; }

            public int TotalItemMerchant { get; set; }

            public DateTime Latest { get; set; }

        }

        public class Product 
        {
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
            //public int CartProductType { get; set; }
            public string VariationText { get; set; }
            public int ProductId { get; set; }
            public int VariationId { get; set; }

            public string ExternalItemId { get; set; }
            public string ExternalItemUrl { get; set; }
            public Guid ExternalId { get; set; }
            public DateTime AddedAt { get; set; }

            public AdditionalDiscount AdditionalDiscount { get; set; }
            public DealExpiration DealExpiration { get; set; }
        }

        public class CartProducts
        {
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
            public CartMerchantDetails Merchant { get; set; }
            public AdditionalDiscount AdditionalDiscount { get; set; }
            public DealExpiration DealExpiration { get; set; }

            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }

            public string ExternalId { get; set; }
            public string ExternalShopName { get; set; }

            public string ExternalShopUrl { get; set; }
            public string ExternalItemUrl { get; set; }
            public DateTime AddedAt { get; set; }
        }
        public class CartMerchantDetails
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public string ExternalId { get; set; }

            public int TypeId { get; set; }
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
        public class DealExpiration
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime ExpiredDate { get; set; }
            public int Type { get; set; }
            public int? TotalValidDays { get; set; }
        }

        public async Task<string> GetFromBlob(int id) 
        {
            var AzureBlob = new AzureBlob();
            var imageResult = await AzureBlob.BlobSmallImagesListQuery(new AzureBlob.BlobSmallImagesListModel
            {
                AzureBlobStorage = azureBlobStorage,
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

        public class DetailPageViewModel
        {
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalShopUrl { get; set; }
            public string ExternalItemUrl { get; set; }
            public int Id { get; set; }
            public int MerchantId { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Description { get; set; }
            public string AdditionInfo { get; set; }
            public string FinePrintInfo { get; set; }
            public string RedemptionInfo { get; set; }
            public string ImageFolderUrl { get; set; }
            public int? ProductCategoryId { get; set; }
            public int? ProductSubCategoryId { get; set; }
            public int? DealTypeId { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? Price { get; set; }
            public decimal? ActualPriceForVpoints { get; set; }
            public decimal? DiscountedPrice { get; set; }
            public int? DiscountRate { get; set; }
            public int? PointsRequired { get; set; }
            public int? AvailableQuantity { get; set; }
            public int? DealExpirationId { get; set; }
            public int? LuckyDrawId { get; set; }
            public int StatusTypeId { get; set; }
            public bool IsActivated { get; set; }
            public bool IsProductVariationEnabled { get; set; }
            public string PendingChanges { get; set; }
            public DateTime CreatedAt { get; set; }
            public Guid CreatedByUserId { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public Guid? LastUpdatedByUser { get; set; }
            public int? TotalBought { get; set; }
            public string Remarks { get; set; }
            public bool IsPublished { get; set; }
            public decimal Rating { get; set; }
            public List<string> ProductImageList { get; set; }
            public string ProductCartPreviewSmallImage { get; set; }

            public bool IsOriginalGuaranteeProduct { get; set; }

            public int MaxPurchaseLimit { get; set; }

            public DetailPageMerchant Merchant { get; set; }

            public DetailPageProductCategory ProductCategory { get; set; }
            public DetailPageProductSubCategory ProductSubCategory { get; set; }

            public List<DetailProductOutlets> ProductOutlets { get; set; }
            public List<DetailPageProductDiscounts> ProductDiscounts { get; set; }

            public DetailPageExpirationType DetailPageExpirationType { get; set; }

            public List<ProductReview> ProductReviewList { get; set; }

            public VariationModel VariationModel { get; set; }

            public List<string> VariationImageList { get; set; }

            public bool IsVPointsMultiplierEnabled { get; set; }
            public decimal VPointsMultiplier { get; set; }
            public decimal VPointsMultiplierCap { get; set; }

            public int ShareShippingCostSameItem { get; set; }

        }
        public class Video
        {
            public string Id { get; set; }

            public Uri Cover { get; set; }

            public Uri Url { get; set; }

            public long? Duration { get; set; }

            public object UploadTime { get; set; }
        }

        public class VariationModel
        {
            public int ProductId { get; set; }
            public List<VariationList> VariationList { get; set; }
            public List<ProductVariationDetailsList> ProductVariationDetailsList { get; set; }
        }

        public class VariationList
        {
            public string Name { get; set; }
            public int ProductId { get; set; }
            public bool IsFirstVariation { get; set; }
            public List<VariationOptions> VariationOptions { get; set; }

        }

        public class VariationOptions
        {
            public string Name { get; set; }
            public int Order { get; set; }
        }

        public class ProductVariationDetailsList
        {
            public string ExternalVariationId { get; set; }
            public int Id { get; set; }
            public string Sku { get; set; }
            public int AvailableQuantity { get; set; }
            public string Order { get; set; }
            public decimal Price { get; set; }
            public decimal PriceBeforeDiscount { get; set; }
            public decimal DiscountedPrice { get; set; }
            public bool IsDiscountedPriceEnabled { get; set; }
        }

        public class ProductReview
        {
            //public Guid Id { get; set; }
            public int ProductId { get; set; }
            //public int MerchantId { get; set; }
            //public Guid OrderItemId { get; set; }
            public decimal Rating { get; set; }
            public string Comment { get; set; }
            public string MemberName { get; set; }
            public string ProductTitle { get; set; }
            // public int MasterMemberProfileId { get; set; }
            public DateTime CreatedAt { get; set; }

            public string VariationText { get; set; }

            public List<string> ImageList { get; set; }
            public List<Video> VideoList { get; set; }
        }
        public class SingleProductImageInfo
        {
            public string ImageUrl { get; set; }
            public DateTimeOffset? createdAt { get; set; }
        }

        public class DetailPageExpirationType
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Type { get; set; }
            public int TotalValidDays { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime ExpiredDate { get; set; }
        }

        public class DetailPageMerchant
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string DisplayName { get; set; }
            public string LogoUrl { get; set; }
            public bool OnVacation { get; set; }
        }

        public class DetailPageProductCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DetailPageProductSubCategory
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DetailProductOutlets
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class DetailPageProductDiscounts
        {
            public int Id { get; set; }
            public int DiscountTypeId { get; set; }
            public string Name { get; set; }
            public decimal PercentageValue { get; set; }
            public decimal PriceValue { get; set; }
            public int PointRequired { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public bool IsActivated { get; set; }
            public bool IsMultiplierEnabled { get; set; }

        }
    }
}
