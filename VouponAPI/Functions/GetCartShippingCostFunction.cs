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
    public class GetCartShippingCostFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;
        private int _masterMemberProfileId = 0;
        private AppConfig _appConfig;

        public GetCartShippingCostFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get cart shipping cost", tags: new[] { "Cart" }, Description = "Get cart shipping cost", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(CartShippingCostRequestModel), Description = "JSON request body ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CartShippingCostResponseModel), Summary = "Get product shipping cost")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]

        [FunctionName("GetCartShippingCostFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "cart/shipping-cost")] HttpRequest req, ILogger log)
        {
           
            var response = new CartShippingCostResponseModel
            {
                Data = new CartShippingCostData()
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
                var requestModel = HttpRequestHelper.DeserializeModel<CartShippingCostRequestModel>(req);

                var maxOrders = rewardsDBContext.AggregatorMaxQuantityFilters.AsNoTracking().Where(x => x.StatusId == 1).OrderByDescending(x => x.MaxQuantity).ToList();

                var items = await rewardsDBContext.ShippingCost.AsNoTracking().Include(x => x.ProductShipping).ThenInclude(x => x.Product).ThenInclude(x => x.Merchant).Where(x => x.ProvinceId == requestModel.ProvinceId && requestModel.CartProduct.Select(x=>x.Id).Contains(x.ProductShipping.ProductId)).ToListAsync();
                var groupedProducts = items.GroupBy(x => x.ProductShipping.Product.MerchantId);
                decimal totalShippingCost = 0;

                OrderShippingCostsModel orderShippingCostsModel = new OrderShippingCostsModel();
                List<OrderShippingCostForPoductIdAndVariationIdModel> orderShippingCosts = new List<OrderShippingCostForPoductIdAndVariationIdModel>();

                var cartProduct = rewardsDBContext.CartProducts.Include(x => x.Product).Where(x => x.MasterMemberProfileId == _masterMemberProfileId && requestModel.CartProduct.Select(x=>x.Id).Contains(x.Product.Id)).ToList();
                List<ShippingCostItemModel> shippingCostItem = new List<ShippingCostItemModel>();

                foreach (var product in cartProduct)
                {
                    var shippingObj = items.Where(x => x.ProductShipping.Product.Id == product.ProductId && x.ProductShipping.Product.MerchantId == product.MerchantId).FirstOrDefault();
                    ShippingCostItemModel item = new ShippingCostItemModel();
                    item.ProductId = product.ProductId;
                    item.MerchantId = product.MerchantId;
                    item.Quantity = product.OrderQuantity;
                    item.VariationId = product.VariationId;

                    if (shippingObj != null)
                    {

                        var shippingCost = shippingObj.Cost;
                        item.Cost = shippingCost;
                        if (product.Product.ShareShippingCostSameItem != 0)
                        {
                            item.Cost = product.OrderQuantity <= product.Product.ShareShippingCostSameItem ? shippingCost : shippingCost * Math.Ceiling((decimal)product.OrderQuantity / product.Product.ShareShippingCostSameItem);
                        }

                        if (shippingObj.ProductShipping.ShippingTypeId == 3)
                        {
                            if (item.VariationId != null)
                            {
                                //Old algorithm
                                var variationList = requestModel.CartProduct.Where(x => x.Id == product.ProductId).Select(x => x.VariationId).ToList();

                                var cartVariant = cartProduct.Where(x => x.MasterMemberProfileId == _masterMemberProfileId && x.ProductId == product.ProductId && variationList.Contains(x.VariationId.Value)).ToList();
                                var totalCostForAllQuantity = cartVariant.Sum(x => x.SubTotal);
                                if (totalCostForAllQuantity > 0 && shippingObj.ProductShipping.ConditionalShippingCost < totalCostForAllQuantity)
                                {
                                    item.Cost = 0;
                                }
                            }
                            else
                            {
                                if (product.Product.ShareShippingCostSameItem > 0)
                                {
                                    item.Cost = product.OrderQuantity <= product.Product.ShareShippingCostSameItem ? shippingCost : shippingCost * Math.Ceiling((decimal)product.OrderQuantity / product.Product.ShareShippingCostSameItem);
                                }
                            }

                        }
                        item.IsSharingShippingDifferentItem = product.Product.IsShareShippingDifferentItem;
                    }
                    else
                    {
                        item.Cost = 0;
                    }
                    OrderShippingCostForPoductIdAndVariationIdModel orderShippingCost = new OrderShippingCostForPoductIdAndVariationIdModel()
                    {
                        MerchantId = item.MerchantId,
                        ProductId = item.ProductId,
                        VariationId = (item.VariationId.HasValue ? item.VariationId.Value : 0),
                        //OrderShippingCost = Math.Max(maxShippingCost, 0)

                    };

                    orderShippingCosts.Add(orderShippingCost);

                    shippingCostItem.Add(item);
                }

                var groupedByMerchant = shippingCostItem.GroupBy(x => x.MerchantId);

                foreach (var itemsMerchant in groupedByMerchant)
                {
                    var countItem = itemsMerchant.Count();
                    decimal maxShippingCost = 0;


                    if (countItem > 1)
                    {
                        foreach (var item in itemsMerchant)
                        {


                            if (!item.IsSharingShippingDifferentItem)
                            {
                                maxShippingCost += item.Cost;
                            }
                            else
                            {
                                maxShippingCost = Math.Max(maxShippingCost, item.Cost);
                            }
                        }
                    }
                    else
                    {
                        maxShippingCost = itemsMerchant.FirstOrDefault().Cost;
                    }
                    var ships = orderShippingCosts.Where(x => x.MerchantId == itemsMerchant.FirstOrDefault().MerchantId).ToList();
                    foreach (var ship in ships) {
                        ship.OrderShippingCost = Math.Max(maxShippingCost, 0);
                    }


                    totalShippingCost += maxShippingCost;
                }



                //  Add external item shipping costs
                orderShippingCostsModel.OrderShippingCosts = orderShippingCosts;
                orderShippingCostsModel.TotalShippingCost = totalShippingCost;
                var allShippingCostModel = await GetShippingCostExternalProduct(requestModel.CartProduct, orderShippingCostsModel);
               


                response.Data.ItemShippingCosts = allShippingCostModel.OrderShippingCosts;
                response.Data.TotalShippingCost = allShippingCostModel.TotalShippingCost;
                response.Code = 0;
                return new OkObjectResult(response);

            }
            catch (Exception ex)
            {
                response.ErrorMessage = "Fail to get cart shipping cost details";
                return new BadRequestObjectResult(response);
            }
        }

        private async Task<OrderShippingCostsModel> GetShippingCostExternalProduct(List<CartProduct> productVariant,OrderShippingCostsModel orderShippingCostsModel) {
            var externalItems = productVariant.Where(x => !string.IsNullOrEmpty(x.ExternalItemId)).ToList();
            var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == _masterMemberProfileId).FirstOrDefaultAsync();

            var maxOrderFilter = rewardsDBContext.AggregatorMaxQuantityFilters.AsNoTracking().Where(x => x.StatusId == 1).OrderByDescending(x => x.MaxQuantity).ToList().Select(x => new MaxOrderFilter
            {
                Id = x.Id,
                MaxQuantity = x.MaxQuantity,
                Keyword = x.Keyword
            }).ToList();
            if (externalItems.Count != 0)
            {
                foreach (var item in externalItems)
                {
                    var request = new AggregatorRequest()
                    {
                        ExternalItemId = item.ExternalItemId,
                        ExternalShopId = item.ExternalShopId,
                        ExternalTypeId = item.ExternalTypeId,
                        City = master.City,
                        State = master.State
                    };
                    var externalShippingResult = await GetFromAggregator(request);
                    externalShippingResult.ProductTitle = item.ProductTitle;
                    if (externalShippingResult != null)
                    {
                        var aggregatorData = externalShippingResult;
                        if (orderShippingCostsModel.OrderShippingCosts.Where(x => x.ExternalShopId == aggregatorData.ExternalShopId).Any())
                        {
                            var ordersFromSameShop = orderShippingCostsModel.OrderShippingCosts.Where(x => x.ExternalShopId == aggregatorData.ExternalShopId && x.OrderShippingCost != 0).FirstOrDefault();
                            if (ordersFromSameShop != null)
                            {
                                if (ordersFromSameShop.OrderShippingCost >= aggregatorData.OrderShippingCost)
                                {
                                    aggregatorData.OrderShippingCost = 0;
                                }
                                else
                                {
                                    ordersFromSameShop.OrderShippingCost = 0;
                                }
                            }
                        }
                        if (item.ProductPrice > 100)
                        {
                            aggregatorData.OrderShippingCost = item.Quantity * aggregatorData.OrderShippingCost;
                        }
                        else
                        {
                            var maxOrderFilterForProduct = maxOrderFilter.Where(x => item.ProductTitle.ToLower().Contains(x.Keyword)).FirstOrDefault();
                            if (maxOrderFilterForProduct != null)
                            {
                                var numberOfShippingRequired = Math.Ceiling((decimal)item.Quantity / maxOrderFilterForProduct.MaxQuantity);
                                if (numberOfShippingRequired > 1)
                                {
                                    aggregatorData.OrderShippingCost = numberOfShippingRequired * aggregatorData.OrderShippingCost;
                                }
                            }
                            else
                            {
                                var numberOfShippingRequired = Math.Ceiling((decimal)item.Quantity / _appConfig.MaxOrderFilter);
                                if (numberOfShippingRequired > 1)
                                {
                                    aggregatorData.OrderShippingCost = numberOfShippingRequired * aggregatorData.OrderShippingCost;
                                }
                            }
                        }
                        

                        orderShippingCostsModel.OrderShippingCosts.Add(aggregatorData);

                        orderShippingCostsModel.TotalShippingCost += Math.Max(aggregatorData.OrderShippingCost, 0);
                    }
                }
                if (orderShippingCostsModel.OrderShippingCosts != null && orderShippingCostsModel.OrderShippingCosts.Count > 0)
                {
                    orderShippingCostsModel.TotalShippingCost = orderShippingCostsModel.OrderShippingCosts.Sum(x => x.OrderShippingCost);
                }

            }
            return orderShippingCostsModel;

        }



        private async Task<OrderShippingCostForPoductIdAndVariationIdModel> GetFromAggregator(AggregatorRequest request) {

            try
            {
                _appConfig = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                if (_appConfig == null || !_appConfig.IsAggregatorEnabled)
                {
                    return null;
                }

                var aggregatorUrl = await rewardsDBContext.AggregatorApiUrls.Where(x => x.StatusId == 1 && x.TypeId == 1).AsNoTracking().ToListAsync();
                var _aggregatorUrl = "";
                if (bool.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.USE_LOCAL_AGGREGATOR)) == true)
                {
                    _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault().Url;
                }
                else
                {
                    _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == false).OrderBy(x => x.LastUpdatedAt).First().Url;
                }


                request.ExternalTypeId = 1;
                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                var httpClient = new HttpClient();
                var result = await httpClient.PostAsync($"{_aggregatorUrl}/v1/shipping-cost", httpContent);
                var resultString = await result.Content.ReadAsStringAsync();
                var crawlerResult = JsonConvert.DeserializeObject<AggregatorResponse>(resultString);
                if (crawlerResult.Successful)
                {
                    var aggregatorData = JsonConvert.DeserializeObject<OrderShippingCostForPoductIdAndVariationIdModel>(crawlerResult.Data.ToString());
                    //aggregatorData.OrderShippingCost = (aggregatorData.OrderShippingCost >= appSettings.Value.App.AdditionalShippingDiscount ? aggregatorData.OrderShippingCost - appSettings.Value.App.AdditionalShippingDiscount : 0);
                    aggregatorData.OrderShippingCost = aggregatorData.OrderShippingCost;
                    return aggregatorData;
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                return null;

            }
            return null;
        }




        public class MaxOrderFilter
        {
            public int Id { get; set; }
            public string Keyword { get; set; }
            public short StatusId { get; set; }
            public short MaxQuantity { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        private class AggregatorRequest {

            public string Town { get; set; }
            public string State { get; set; }
            public string City { get; set; }
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; } = 1;
        }

        private class AggregatorResponse {
            public bool Successful { get; set; }
            public int Code { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }
        }

        protected class CartShippingCostRequestModel
        {
            public int ProvinceId { get; set; }
            public List<CartProduct> CartProduct { get; set; }
        }

        protected class CartProduct
        {
            public int Id { get; set; }
            public int VariationId { get; set; }
            public int Quantity { get; set; }
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalVariationText { get; set; }

            public string ProductTitle { get; set; }

            public decimal ProductPrice { get; set; }

        }

        private class CartShippingCostResponseModel : ApiResponseViewModel
        {
            public CartShippingCostData Data { get; set; }
        }

        private class CartShippingCostData
        {
            public List<OrderShippingCostForPoductIdAndVariationIdModel> ItemShippingCosts { get; set; }
            public decimal TotalShippingCost { get; set; }
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

            public int MerchantId;
        }

        public class OrderShippingCostsModel
        {
            public decimal TotalShippingCost { get; set; }

            public List<OrderShippingCostForPoductIdAndVariationIdModel> OrderShippingCosts { get; set; }
        }

        public class ShippingCostItemModel
        {
            public int ProductId;

            public decimal Cost;

            public int Quantity;

            public int MerchantId;

            public bool IsSharingShippingDifferentItem;

            public int? VariationId;
        }

    }
}
