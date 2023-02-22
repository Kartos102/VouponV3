using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Aggregators.Queries
{
    public class AggregatorProductListingByShopQuery : IRequest<ApiResponseViewModel>
    {
        public string ExternalItemId { get; set; }
        public string ExternalShopId { get; set; }
        public byte ExternalTypeId { get; set; }

        public int PageNumber { get; set; }
    }
    public class AggregatorProductListingByShopQueryHandler : IRequestHandler<AggregatorProductListingByShopQuery, ApiResponseViewModel>
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private string _aggregatorUrl { get; set; }
        IConnectionMultiplexer _connectionMultiplexer;

        private readonly IOptions<AppSettings> _appSettings;
        public AggregatorProductListingByShopQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings, IConnectionMultiplexer connectionMultiplexer)
        {
            _rewardsDBContext = rewardsDBContext;
            _appSettings = appSettings;
            _connectionMultiplexer = connectionMultiplexer;
        }

        public async Task<ApiResponseViewModel> Handle(AggregatorProductListingByShopQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();

            var appConfig = await _rewardsDBContext.AppConfig.FirstOrDefaultAsync();
            if (appConfig == null || !appConfig.IsAggregatorEnabled)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Data = null;
                return apiResponseViewModel;
            }

            var aggregatorUrl = await _rewardsDBContext.AggregatorApiUrls.Where(x => x.StatusId == 1 && x.TypeId == 1).ToListAsync();
            if (_appSettings.Value.App.UseLocalAggregator)
            {
                _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault().Url;
            }
            else
            {
                var agg = aggregatorUrl.Where(x => x.IsLocalAggregator == false).OrderBy(x => x.LastUpdatedAt).Last();

                agg.LastUpdatedAt = DateTime.Now;
                _rewardsDBContext.Update(agg);
                await _rewardsDBContext.SaveChangesAsync();
                _aggregatorUrl = agg.Url;
            }

            try
            {
                Thread.Sleep(appConfig.AggregatorSleepMiliseconds);
                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                var httpClient = new HttpClient();
                var result = await httpClient.GetAsync($"{_aggregatorUrl}/v1/shop/listing?externalTypeId={request.ExternalTypeId}&ExternalShopId={request.ExternalShopId}&limt=10&offset=0");
                var resultString = await result.Content.ReadAsStringAsync();
                var crawlerResult = JsonConvert.DeserializeObject<ApiResponseViewModel>(resultString);
                if (crawlerResult.Successful)
                {
                    apiResponseViewModel.Data = crawlerResult.Data;
                    apiResponseViewModel.Successful = crawlerResult.Successful;
                }
                else
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = crawlerResult.Message;
                }
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = ex.ToString();
            }
            return apiResponseViewModel;
        }
    }
}
