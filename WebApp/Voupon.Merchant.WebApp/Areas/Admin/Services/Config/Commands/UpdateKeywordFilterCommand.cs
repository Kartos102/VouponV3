using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands
{
    public class UpdateKeywordFilterCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string Keyword { get; set; }

        public class UpdateKeywordFilterCommandHandler : IRequestHandler<UpdateKeywordFilterCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateKeywordFilterCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateKeywordFilterCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var aggregatorKeywordFilters = await rewardsDBContext.AggregatorKeywordFilters.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                    if (aggregatorKeywordFilters == null)
                    {
                        apiResponseViewModel.Message = "Fail to update max order quantity [0001]";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    aggregatorKeywordFilters.Keyword = request.Keyword;

                    rewardsDBContext.AggregatorKeywordFilters.Update(aggregatorKeywordFilters);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Message = "Successfully updated keyword filter";
                    apiResponseViewModel.Successful = true;

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to update keyword filter [0003]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
