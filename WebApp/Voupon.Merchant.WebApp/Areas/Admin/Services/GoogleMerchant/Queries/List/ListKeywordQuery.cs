using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.GoogleMerchant.Queries.List
{
    public class ListKeywordQuery : ListQueryRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public class ListKeywordQueryHandler : IRequestHandler<ListKeywordQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ListKeywordQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ListKeywordQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var keywords = await rewardsDBContext.GoogleMerchantKeywords.ToListAsync();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = new ListView<KeywordViewModel>(keywords.Count(), request.PageSize, request.PageIndex)
                    {
                        Items = keywords.Select(x => new KeywordViewModel
                        {
                            Id = x.Id,
                            Keyword = x.Keyword,
                            SortBy = x.SortBy,
                            TotalListing = x.TotalListing,
                            Language = x.Language
                        }).ToList()
                    };
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;
                }
                return apiResponseViewModel;
            }
        }
    }

    public class KeywordViewModel
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; }
        public int TotalListing { get; set; }
        public string SortBy { get; set; }
        public string Language { get; set; }
    }
}
