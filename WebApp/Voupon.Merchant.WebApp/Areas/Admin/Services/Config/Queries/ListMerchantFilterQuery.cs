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
    public class ListMerchantFilterQuery : ListQueryRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public class ListMerchantFilterQueryHandler : IRequestHandler<ListMerchantFilterQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ListMerchantFilterQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ListMerchantFilterQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var items = await rewardsDBContext.AggregatorExcludeMerchants.AsNoTracking().ToListAsync();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = new ListView<MerchantFilterViewModel>(items.Count(), request.PageSize, request.PageIndex)
                    {
                        Items = items.Select(x => new MerchantFilterViewModel
                        {
                            Id = x.Id,
                            MerchantUsername = x.MerchantUsername,
                            MerchantId = x.MerchantId,
                            ExternalTypeId = x.ExternalTypeId,
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


        public class MerchantFilterViewModel
        {
            public int Id { get; set; }
            public string MerchantUsername { get; set; }
            public string MerchantId { get; set; }
            public short StatusId { get; set; }
            public short ExternalTypeId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }
}
