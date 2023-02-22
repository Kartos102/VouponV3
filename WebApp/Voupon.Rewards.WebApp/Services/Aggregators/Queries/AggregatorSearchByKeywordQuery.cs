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

    public class AggregatorSearchByKeywordQuery : IRequest<ApiResponseViewModel>
    {
        public string SearchQuery { get; set; }
        public int MinPrice { get; set; }

        public int MaxPrice { get; set; }
        public List<int> LocationFilter { get; set; }

        public int PageNumber { get; set; }
    }
    public class AggregatorSearchByKeywordQueryHandler : IRequestHandler<AggregatorSearchByKeywordQuery, ApiResponseViewModel>
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private string _aggregatorUrl;
        private readonly IOptions<AppSettings> appSettings;
        private const int SLEEP_TO_SLOWDOWN_REQUEST_MILISECONDS = 500;

        public AggregatorSearchByKeywordQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            _rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(AggregatorSearchByKeywordQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                var appConfig = await _rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                if(appConfig == null || !appConfig.IsAggregatorEnabled)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Data = null;
                    return apiResponseViewModel;
                }

                var aggregatorUrl = await _rewardsDBContext.AggregatorApiUrls.Where(x=>x.StatusId == 1 && x.TypeId == 2).ToListAsync();
                if (appSettings.Value.App.UseLocalAggregator)
                {
                    var agg = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault();
                    if(agg == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Data = null;
                        return apiResponseViewModel;
                    }
                    _aggregatorUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault().Url;
                }
                else
                {
                    var agg = aggregatorUrl.Where(x => x.IsLocalAggregator == false).OrderBy(x => x.LastUpdatedAt).FirstOrDefault();

                    if(agg == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Data = null;
                        return apiResponseViewModel;
                    }

                    agg.LastUpdatedAt = DateTime.Now;
                    _rewardsDBContext.Update(agg);
                    await _rewardsDBContext.SaveChangesAsync();
                    _aggregatorUrl = agg.Url;
                }

                Thread.Sleep(appConfig.AggregatorSleepMiliseconds);
                StringContent httpContent = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                var httpClient = new HttpClient();
                var result = await httpClient.PostAsync($"{_aggregatorUrl}/aggregator/product-search", httpContent);
                var resultString = await result.Content.ReadAsStringAsync();

                if(string.IsNullOrEmpty(resultString))
                {
                    apiResponseViewModel.Data = null;
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

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
