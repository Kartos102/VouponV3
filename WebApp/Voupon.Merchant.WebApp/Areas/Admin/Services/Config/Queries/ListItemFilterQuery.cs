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
    public class ListItemFilterQuery : ListQueryRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public class ListItemFilterQueryHandler : IRequestHandler<ListItemFilterQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ListItemFilterQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ListItemFilterQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var items = await rewardsDBContext.AggregatorExcludeProducts.AsNoTracking().ToListAsync();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = new ListView<ItemFilterViewModel>(items.Count(), request.PageSize, request.PageIndex)
                    {
                        Items = items.Select(x => new ItemFilterViewModel
                        {
                            Id = x.Id,
                            MerchantId = x.MerchantId,
                            ExternalTypeId = x.ExternalTypeId,
                            ProductId = x.ProductId,
                            ProductUrl = x.ProductUrl,
                            StatusId = x.StatusId,
                            CreatedAt = x.CreatedAt
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

        public class ItemFilterViewModel
        {
            public int Id { get; set; }
            public string MerchantId { get; set; }
            public string ProductUrl { get; set; }
            public string ProductId { get; set; }
            public short StatusId { get; set; }
            public short ExternalTypeId { get; set; }
            public DateTime CreatedAt { get; set; }
        }
    }


}
