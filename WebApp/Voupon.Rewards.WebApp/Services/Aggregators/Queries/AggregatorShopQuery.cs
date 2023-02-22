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
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Aggregators.Queries
{

    public class AggregatorShopQuery : IRequest<ApiResponseViewModel>
    {
        public string ExternalShopId { get; set; }
        public byte ExternalTypeId { get; set; }
        public int PageNumber { get; set; }
    }
    public class AggregatorShopDetailQueryHandler : IRequestHandler<AggregatorShopQuery, ApiResponseViewModel>
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        private string _aggregatorUrl;
        private const int SLEEP_TO_SLOWDOWN_REQUEST_MILISECONDS = 700;

        public AggregatorShopDetailQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            _rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(AggregatorShopQuery request, CancellationToken cancellationToken)
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
                var result = await httpClient.PostAsync($"{_aggregatorUrl}/v1/shop", httpContent);
                var resultString = await result.Content.ReadAsStringAsync();
                var crawlerResult = JsonConvert.DeserializeObject<ApiResponseViewModel>(resultString);
                if (crawlerResult.Successful)
                {
                    apiResponseViewModel.Data = crawlerResult.Data;
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
