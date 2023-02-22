using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Queries.List
{
    public class ListKeywordFilterQuery : ListQueryRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public class ListKeywordFilterQueryHandler : IRequestHandler<ListKeywordFilterQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ListKeywordFilterQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ListKeywordFilterQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var items = await rewardsDBContext.AggregatorKeywordFilters.AsNoTracking().ToListAsync();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = new ListView<KeywordFilterViewModel>(items.Count(), request.PageSize, request.PageIndex)
                    {
                        Items = items.Select(x => new KeywordFilterViewModel
                        {
                            Id = x.Id,
                            Keyword = x.Keyword,
                            CreatedAt = x.CreatedAt,
                            StatusId = x.StatusId
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


        public class KeywordFilterViewModel
        {
            public int Id { get; set; }
            public string Keyword { get; set; }
            public short StatusId { get; set; }
            public short MaxQuantity { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }


}
