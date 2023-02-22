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

    public class AggregatorSearchByListKeywordsQuery : IRequest<ApiResponseViewModel>
    {
        public int[] SubcategoriesList { get; set; }
        public int MinPrice { get; set; }

        public int MaxPrice { get; set; }   
        public List<int> LocationFilter { get; set; }

        public int PageNumber { get; set; }
    }
    public class AggregatorSearchByListKeywordsQueryHandler : IRequestHandler<AggregatorSearchByListKeywordsQuery, ApiResponseViewModel>
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private string _aggregatorUrl;
        private readonly IOptions<AppSettings> appSettings;
        private const int SLEEP_TO_SLOWDOWN_REQUEST_MILISECONDS = 500;
        public AggregatorSearchByListKeywordsQueryHandler(IOptions<AppSettings> appSettings, RewardsDBContext rewardsDBContext)
        {
            _rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(AggregatorSearchByListKeywordsQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            var aggregatorResultList = new List<SearchProductViewModel>();
            string[] searchList = new string[request.SubcategoriesList.Length];
            try
            {
                var appConfig = await _rewardsDBContext.AppConfig.AsNoTracking().FirstOrDefaultAsync();

                if(appConfig == null || !appConfig.IsAggregatorEnabled)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Data = null;
                    return apiResponseViewModel;
                }

                var aggregatorUrl = await _rewardsDBContext.AggregatorApiUrls.Where(x => x.StatusId == 1 && x.TypeId == 2).ToListAsync();
                if (appSettings.Value.App.UseLocalAggregator)
                {
                    var agg = aggregatorUrl.Where(x => x.IsLocalAggregator == true).FirstOrDefault();
                    if (agg == null)
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

                    if (agg == null)
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

                searchList = await _rewardsDBContext.ProductSubCategories.AsNoTracking().Where(x => request.SubcategoriesList.Contains(x.Id)).Select(x => x.Name).ToArrayAsync();
                foreach (var searchText in searchList)
                {


                    /*
                    ApiCallObj apiCallObj = new ApiCallObj()
                    {
                        PageNumber = request.PageNumber,
                        MinPrice = request.MinPrice,
                        MaxPrice = request.MaxPrice,
                        SearchQuery = searchText,
                        LocationFilter = request.LocationFilter
                    };

                    
                    Thread.Sleep(appConfig.AggregatorSleepMiliseconds);

                    StringContent httpContent = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                    var httpClient = new HttpClient();
                    var result = await httpClient.PostAsync($"{_aggregatorUrl}/aggregator/product-search", httpContent);
                    var resultString = await result.Content.ReadAsStringAsync();

                    var crawlerResult = JsonConvert.DeserializeObject<ApiResponseViewModel>(resultString);
                    if (crawlerResult.Successful)
                    {
                        var aggregatorData = JsonConvert.DeserializeObject<List<SearchProductViewModel>>(crawlerResult.Data.ToString());
                        if (aggregatorData != null && aggregatorData.Any())
                        {
                            aggregatorResultList.AddRange(aggregatorData);
                        }
                    }
                    */
                }
                apiResponseViewModel.Data = aggregatorResultList;
                if (aggregatorResultList.Count > 0)
                    apiResponseViewModel.Successful = true;
                else
                    apiResponseViewModel.Successful = false;

            }
            catch (Exception ex)
            {
                var error = ex.ToString();
                //  Log
            }
            return apiResponseViewModel;
        }
        public class ApiCallObj
        {
            public string SearchQuery { get; set; }
            public int MinPrice { get; set; }

            public int MaxPrice { get; set; }
            public List<int> LocationFilter { get; set; }

            public int PageNumber { get; set; }
        }
    }
}
