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
    public class ListAdditionalDiscountQuery : ListQueryRequest<ApiResponseViewModel>
    {
        public class ListAdditionalDiscountQueryHandler : IRequestHandler<ListAdditionalDiscountQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ListAdditionalDiscountQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ListAdditionalDiscountQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var items = await rewardsDBContext.AdditionalDiscounts.AsNoTracking().OrderBy(x => x.MaxPrice).ToListAsync();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = new ListView<AddtionalDiscountViewModel>(items.Count(), request.PageSize, request.PageIndex)
                    {
                        Items = items.Select(x => new AddtionalDiscountViewModel
                        {
                            Id = x.Id,
                            StatusId = x.StatusId,
                            DiscountPercentage = x.DiscountPercentage,
                            Points = x.Points,
                            MaxPrice = x.MaxPrice,
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

        public class AddtionalDiscountViewModel
        {
            public int Id { get; set; }
            public decimal MaxPrice { get; set; }
            public decimal DiscountPercentage { get; set; }
            public short Points { get; set; }
            public short StatusId { get; set; }
        }
    }


}
