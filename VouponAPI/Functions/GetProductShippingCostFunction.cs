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
using Azure.Core;
using static Voupon.API.Functions.GetCartShippingCostFunction;

namespace Voupon.API.Functions
{
    public class GetProductShippingCostFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetProductShippingCostFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get product shipping cost", tags: new[] { "Products" }, Description = "Get product shipping cost", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(ShippingCostRequestModel), Description = "JSON request body ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ProductShippingCostResponseModel), Summary = "Get product shipping cost")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]

        [FunctionName("GetProductShippingCostFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "product/shipping-cost")] HttpRequest req, ILogger log)
        {
            var apiResponseViewModel = new ProductShippingCostResponseModel
            {
                Data = new ProductShippingCostData()
            };

            try
            {
                var requestModel = HttpRequestHelper.DeserializeModel<ShippingCostRequestModel>(req);

                /*
                var reqProductId = req.Query["id"];
                var reqProvinceId = req.Query["provinceId"];
                var reqExternalItemId = req.Query["i"];
                var reqExternalShopId = req.Query["s"];
                var reqExternalTypeId = req.Query["t"];
                var reqState = req.Query["state"];
                var reqTitle = req.Query["title"];
                var reqQuantity = req.Query["quantity"];


                int productId = 0;
                bool success = int.TryParse(requestModel.ProductId, out productId);
                */

                var productShippingCostModel = new ProductShippingCost();
                var provinces = await rewardsDBContext.Provinces.AsNoTracking().ToListAsync();
                if (requestModel.ProductId > 0)
                {
                    var item = await rewardsDBContext.ShippingCost.Include(x => x.ProductShipping).Where(x => x.ProductShipping.ProductId == requestModel.ProductId && x.ProvinceId == requestModel.ProvinceId).FirstOrDefaultAsync();

                    if (item != null)
                    {
                        productShippingCostModel.Id = item.ProductShipping.Id;
                        productShippingCostModel.ProductId = item.ProductShipping.ProductId;
                        productShippingCostModel.ShippingTypeId = item.ProductShipping.ShippingTypeId;
                        productShippingCostModel.ConditionalShippingCost = item.ProductShipping.ConditionalShippingCost;
                        productShippingCostModel.ShippingCosts = new List<ShippingCostModel>();

                        var shippingCostModel = new ShippingCostModel();
                        shippingCostModel.Id = item.Id;
                        shippingCostModel.ProductShippingId = item.ProductShippingId;
                        shippingCostModel.ProvinceId = item.ProvinceId;
                        //shippingCostModel.Cost = (item.Cost >= appSettings.Value.App.AdditionalShippingDiscount ? item.Cost - appSettings.Value.App.AdditionalShippingDiscount : 0);
                        shippingCostModel.Cost = item.Cost;
                        productShippingCostModel.ShippingCosts.Add(shippingCostModel);
                    }
                    //Default shipping when there is no shipping cost
                    else
                    {
                        productShippingCostModel.ShippingTypeId = 1;
                    }

                    apiResponseViewModel.Data.ProductShippingCost = productShippingCostModel;
                    return new OkObjectResult(apiResponseViewModel);
                }

                if (requestModel.ProvinceId != 0)
                {
                    var province = provinces.Where(x => x.Id == requestModel.ProvinceId).FirstOrDefault();
                    if (province != null)
                    {
                        requestModel.State = province.Name;
                    }
                }
                else
                {
                    requestModel.State = "Kuala Lumpur";
                }
                var aggregatorUrl = await vodusV2Context.AggregatorApiUrls.AsNoTracking().ToListAsync();
                var _aggregatorUrl = "";
                if (bool.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.USE_LOCAL_AGGREGATOR)) == true)
                {
                    _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true && x.Status == 1).FirstOrDefault().Url;
                }
                else
                {
                    _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == false && x.Status == 1).OrderBy(x => x.LastUpdatedAt).First().Url;
                }
                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(requestModel), System.Text.Encoding.UTF8, "application/json");
                var httpClient = new HttpClient();
                var result = await httpClient.PostAsync($"{_aggregatorUrl}/v1/shipping-cost", httpContent);
                var resultString = await result.Content.ReadAsStringAsync();
                var crawlerResult = JsonConvert.DeserializeObject<LegacyApiResponseViewModel>(resultString);
                if (crawlerResult.Successful)
                {
                    var aggregatorData = JsonConvert.DeserializeObject<OrderShippingCostForPoductIdAndVariationIdModel>(crawlerResult.Data.ToString());

                    productShippingCostModel.ShippingCosts = new List<ShippingCostModel>();


                    var maxOrderFilter = rewardsDBContext.AggregatorMaxQuantityFilters.AsNoTracking().Where(x => x.StatusId == 1).OrderBy(x => x.MaxQuantity).ToList();
                    var appConfig = await rewardsDBContext.AppConfig.AsNoTracking().FirstOrDefaultAsync();

                    if (requestModel.ProductPrice > 100)
                    {
                        aggregatorData.OrderShippingCost = requestModel.Quantity * aggregatorData.OrderShippingCost;
                    }
                    else
                    {
                        var numberOfShippingRequired = Math.Ceiling((decimal)requestModel.Quantity / appConfig.MaxOrderFilter);
                        if (numberOfShippingRequired > 1)
                        {
                            aggregatorData.OrderShippingCost = numberOfShippingRequired * aggregatorData.OrderShippingCost;
                        }

                        foreach (var filter in maxOrderFilter)
                        {
                            var keyword = filter.Keyword.Trim();
                            var splittedKeywords = keyword.Split(" ");

                            var totalMatches = 0;
                            foreach (var key in splittedKeywords)
                            {
                                if (requestModel.ProductTitle.ToLower().Contains(key))
                                {
                                    totalMatches++;
                                }
                            }
                            if (totalMatches == splittedKeywords.Length)
                            {
                                numberOfShippingRequired = Math.Ceiling((decimal)requestModel.Quantity / filter.MaxQuantity);
                                if (numberOfShippingRequired > 1)
                                {
                                    aggregatorData.OrderShippingCost = numberOfShippingRequired * aggregatorData.OrderShippingCost;
                                }
                                break;
                            }
                        }
                    }
                    var shippingCostModel = new ShippingCostModel();
                    shippingCostModel.ProvinceId = requestModel.ProvinceId;
                    //shippingCostModel.Cost = (aggregatorData.OrderShippingCost >= appSettings.Value.App.AdditionalShippingDiscount ? aggregatorData.OrderShippingCost - appSettings.Value.App.AdditionalShippingDiscount : 0);
                    shippingCostModel.Cost = aggregatorData.OrderShippingCost;
                    productShippingCostModel.ShippingCosts.Add(shippingCostModel);

                }

                apiResponseViewModel.Data.ProductShippingCost = productShippingCostModel;
                return new OkObjectResult(apiResponseViewModel);

            }
            catch (Exception ex)
            {
                apiResponseViewModel.ErrorMessage = "Fail to get Product Shipping cost details";
                return new BadRequestObjectResult(apiResponseViewModel);
            }
        }

        protected class ShippingCostRequestModel
        {
            public int ProductId { get; set; }
            public int ProvinceId { get; set; }
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string State { get; set; }
            public string ProductTitle { get; set; }
            public int Quantity { get; set; }
            public decimal ProductPrice { get; set; }
        }

        private class ProductShippingCostResponseModel : ApiResponseViewModel
        {
            public ProductShippingCostData Data { get; set; }
        }

        private class ProductShippingCostData
        {
            public ProductShippingCost ProductShippingCost { get; set; }
        }

        protected class OrderShippingCostForPoductIdAndVariationId
        {
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalItemVariationText { get; set; }
            public int ProductId { get; set; }
            public int VariationId { get; set; }
            public decimal OrderShippingCost { get; set; }
            public string ProductTitle { get; set; }
        }

        protected class GetProductShippingDetails
        {
            public ProductShippingCost productShippingCost { get; set; }
            public List<ShippingTypes> ShippingTypes { get; set; }
        }

        protected class ProductShippingCost
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int ShippingTypeId { get; set; }
            public decimal ConditionalShippingCost { get; set; }
            public ICollection<ShippingCostModel> ShippingCosts { get; set; }

        }

        protected class ShippingTypes
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActivated { get; set; }
            public DateTime CreatedAt { get; set; }
            public Guid CreatedByUserId { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public Guid? LastUpdatedByUserId { get; set; }
        }

        protected class ShippingCostModel
        {
            public int Id { get; set; }
            public int ProductShippingId { get; set; }
            public int ProvinceId { get; set; }
            public string? ProvinceName { get; set; }
            public decimal Cost { get; set; }

        }
    }
}
