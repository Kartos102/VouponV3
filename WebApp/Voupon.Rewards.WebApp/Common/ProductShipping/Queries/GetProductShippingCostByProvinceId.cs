using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Common.Services.ProductShipping.Models;
using Voupon.Rewards.WebApp.Common.ShippingCost.Models;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Services.ProductShipping.Queries
{
    public class GetProductShippingCostByProvinceId : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public int ProvinceId { get; set; }
        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public byte ExternalTypeId { get; set; }
        public string State { get; set; }
        public string ProductTitle { get; set; }
        public int Quantity { get; set; }

        public int ShareShippingCostSameItem { get; set; }

        public decimal ProductPrice { get; set; }

    }
    public class GetProductShippingCostByProvinceIdHandler : IRequestHandler<GetProductShippingCostByProvinceId, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        VodusV2Context vodusV2Context;
        private readonly IOptions<AppSettings> appSettings;
        private string _aggregatorUrl;

        public GetProductShippingCostByProvinceIdHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(GetProductShippingCostByProvinceId request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                ProductShippingCostModel productShippingCostModel = new ProductShippingCostModel();
                var provinces = await rewardsDBContext.Provinces.AsNoTracking().ToListAsync();
                if (request.ProductId > 0)
                {
                    var item = await rewardsDBContext.ShippingCost.Include(x => x.ProductShipping).Where(x => x.ProductShipping.ProductId == request.ProductId && x.ProvinceId == request.ProvinceId).FirstOrDefaultAsync();

                    if (item != null)
                    {
                        productShippingCostModel.Id = item.ProductShipping.Id;
                        productShippingCostModel.ProductId = item.ProductShipping.ProductId;
                        productShippingCostModel.ShippingTypeId = item.ProductShipping.ShippingTypeId;
                        productShippingCostModel.ConditionalShippingCost = item.ProductShipping.ConditionalShippingCost;
                        productShippingCostModel.ShippingCosts = new List<Models.ShippingCostModel>();

                        var shippingCostModel = new Models.ShippingCostModel();
                        shippingCostModel.Id = item.Id;
                        shippingCostModel.ProductShippingId = item.ProductShippingId;
                        shippingCostModel.ProvinceId = item.ProvinceId;
                        //shippingCostModel.Cost = (item.Cost >= appSettings.Value.App.AdditionalShippingDiscount ? item.Cost - appSettings.Value.App.AdditionalShippingDiscount : 0);
                        shippingCostModel.Cost = item.Cost;
                        if (request.ShareShippingCostSameItem > 0) 
                        {
                            shippingCostModel.Cost = request.Quantity <= request.ShareShippingCostSameItem ? item.Cost : item.Cost * Math.Ceiling((decimal)request.Quantity / request.ShareShippingCostSameItem); ;

                        }
                        productShippingCostModel.ShippingCosts.Add(shippingCostModel);
                    }
                    //Default shipping when there is no shipping cost
                    else
                    {
                        productShippingCostModel.ShippingTypeId = 1;
                    }

                    response.Successful = true;
                    response.Message = "Get Product Shipping cost details Successfully";
                    response.Data = productShippingCostModel;
                    return response;
                }

                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    if (request.ProvinceId != 0)
                    {
                        var province = provinces.Where(x => x.Id == request.ProvinceId).FirstOrDefault();
                        if (province != null)
                        {
                            request.State = province.Name;
                        }
                    }
                    else
                    { 
                        request.State = "Kuala Lumpur";
                    }
                    var aggregatorUrl = await rewardsDBContext.AggregatorApiUrls.Where(x=>x.StatusId == 1 && x.TypeId == 1).AsNoTracking().ToListAsync();
                    if (appSettings.Value.App.UseLocalAggregator)
                    {
                        _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault().Url;
                    }
                    else
                    {
                        _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == false).OrderBy(x => x.LastUpdatedAt).Last().Url;
                    }
                    StringContent httpContent = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                    var httpClient = new HttpClient();
                    var result = await httpClient.PostAsync($"{_aggregatorUrl}/v1/shipping-cost", httpContent);
                    var resultString = await result.Content.ReadAsStringAsync();
                    var crawlerResult = JsonConvert.DeserializeObject<ApiResponseViewModel>(resultString);
                    if (crawlerResult.Successful)
                    {
                        var aggregatorData = JsonConvert.DeserializeObject<OrderShippingCostForPoductIdAndVariationIdModel>(crawlerResult.Data.ToString());

                        productShippingCostModel.ShippingCosts = new List<Models.ShippingCostModel>();


                        var maxOrderFilter = rewardsDBContext.AggregatorMaxQuantityFilters.AsNoTracking().Where(x => x.StatusId == 1).OrderBy(x => x.MaxQuantity).ToList();
                        var appConfig = await rewardsDBContext.AppConfig.AsNoTracking().FirstOrDefaultAsync();

                        if(request.ProductPrice > 100)
                        {
                            aggregatorData.OrderShippingCost = request.Quantity * aggregatorData.OrderShippingCost;
                        }
                        else
                        {
                            var numberOfShippingRequired = Math.Ceiling((decimal)request.Quantity / appConfig.MaxOrderFilter);
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
                                    if (request.ProductTitle.ToLower().Contains(key))
                                    {
                                        totalMatches++;
                                    }
                                }
                                if (totalMatches == splittedKeywords.Length)
                                {
                                    numberOfShippingRequired = Math.Ceiling((decimal)request.Quantity / filter.MaxQuantity);
                                    if (numberOfShippingRequired > 1)
                                    {
                                        aggregatorData.OrderShippingCost = numberOfShippingRequired * aggregatorData.OrderShippingCost;
                                    }
                                    break;
                                }
                            }
                        }

                        

                        var shippingCostModel = new Models.ShippingCostModel();
                        shippingCostModel.ProvinceId = request.ProvinceId;
                        //shippingCostModel.Cost = (aggregatorData.OrderShippingCost >= appSettings.Value.App.AdditionalShippingDiscount ? aggregatorData.OrderShippingCost - appSettings.Value.App.AdditionalShippingDiscount : 0);
                        shippingCostModel.Cost = aggregatorData.OrderShippingCost;
                        productShippingCostModel.ShippingCosts.Add(shippingCostModel);

                    }
                }
                catch (Exception ex)
                {
                    var error = ex.ToString();
                }

                response.Successful = true;
                response.Message = "Get Product Shipping cost details Successfully";
                response.Data = productShippingCostModel;

            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = "Fail to get Product Shipping cost details";
            }

            return response;
        }
    }
    public class GetProductShippingDetailsModel
    {
        public ProductShippingCostModel productShippingCost { get; set; }
        public List<ShippingTypes> ShippingTypes { get; set; }
    }
}
