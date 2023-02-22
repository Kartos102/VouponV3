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
using Voupon.Rewards.WebApp.Common.ShippingCost.Models;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Aggregators.Queries
{

    public class AggregatorShippingCostQuery : IRequest<ApiResponseViewModel>
    {
        public string Town { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public short ExternalTypeId { get; set; }
    }
    public class AggregatorShippingCostQueryHandler : IRequestHandler<AggregatorShippingCostQuery, ApiResponseViewModel>
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        private string _aggregatorUrl;

        public AggregatorShippingCostQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            _rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(AggregatorShippingCostQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                var appConfig = await _rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                if (appConfig == null || !appConfig.IsAggregatorEnabled)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Data = null;
                    return apiResponseViewModel;
                }

                var aggregatorUrl = await _rewardsDBContext.AggregatorApiUrls.Where(x=>x.StatusId == 1 && x.TypeId == 1).AsNoTracking().ToListAsync();
                if (appSettings.Value.App.UseLocalAggregator)
                {
                    _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault().Url;
                }
                else
                {
                    _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == false).OrderBy(x => x.LastUpdatedAt).Last().Url;
                }

                Thread.Sleep(appConfig.AggregatorSleepMiliseconds);

                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                var httpClient = new HttpClient();
                var result = await httpClient.PostAsync($"{_aggregatorUrl}/v1/shipping-cost", httpContent);
                var resultString = await result.Content.ReadAsStringAsync();
                var crawlerResult = JsonConvert.DeserializeObject<ApiResponseViewModel>(resultString);
                if (crawlerResult.Successful)
                {
                    var aggregatorData = JsonConvert.DeserializeObject<OrderShippingCostForPoductIdAndVariationIdModel>(crawlerResult.Data.ToString());
                    //aggregatorData.OrderShippingCost = (aggregatorData.OrderShippingCost >= appSettings.Value.App.AdditionalShippingDiscount ? aggregatorData.OrderShippingCost - appSettings.Value.App.AdditionalShippingDiscount : 0);
                    aggregatorData.OrderShippingCost = aggregatorData.OrderShippingCost;
                    apiResponseViewModel.Data = JsonConvert.SerializeObject(aggregatorData);
                    apiResponseViewModel.Successful = crawlerResult.Successful;
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
            }
            return apiResponseViewModel;
        }
    }
}
