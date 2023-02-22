using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Ads.Queries
{
    public class GoogleMerchantKeywords
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; }
        public int TotalListing { get; set; }
        public string SortBy { get; set; }
        public string Language { get; set; }
    }
    public class GoogleMerchantKeywordQuery : IRequest<List<GoogleMerchantKeywords>>
    {

        public class GoogleMerchantKeywordQueryHandler : IRequestHandler<GoogleMerchantKeywordQuery, List<GoogleMerchantKeywords>>
        {

            RewardsDBContext rewardsDBContext;
            IConnectionMultiplexer connectionMultiplexer;
            private readonly IAzureBlobStorage azureBlobStorage;
            private readonly IOptions<AppSettings> appSettings;
            public GoogleMerchantKeywordQueryHandler(RewardsDBContext rewardsDBContext, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.connectionMultiplexer = connectionMultiplexer;
                this.azureBlobStorage = azureBlobStorage;
                this.appSettings = appSettings;
            }


            public async Task<List<GoogleMerchantKeywords>> Handle(GoogleMerchantKeywordQuery request, CancellationToken cancellationToken)
            {
                var result = new List<GoogleMerchantKeywords>();
                try
                {
                    var googleMerchantKeywords = await rewardsDBContext.GoogleMerchantKeywords.ToListAsync();
                    if (googleMerchantKeywords == null)
                    {
                        return result;
                    }

                    return googleMerchantKeywords.Select(x => new GoogleMerchantKeywords
                    {
                        Id = x.Id,
                        Keyword = x.Keyword,
                        SortBy = x.SortBy,
                        TotalListing = x.TotalListing,
                        Language = x.Language
                    }).ToList();
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
    }

}
