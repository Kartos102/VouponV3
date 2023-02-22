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
    public class UpdateMaxOrderStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public byte StatusId { get; set; }

        public class UpdateMaxOrderStatusCommandHandler : IRequestHandler<UpdateMaxOrderStatusCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateMaxOrderStatusCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateMaxOrderStatusCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var aggregatorMaxQuantityFilters = await rewardsDBContext.AggregatorMaxQuantityFilters.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                    if (aggregatorMaxQuantityFilters == null)
                    {
                        apiResponseViewModel.Message = "Fail to update status [0001]";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    aggregatorMaxQuantityFilters.StatusId = request.StatusId;

                    rewardsDBContext.AggregatorMaxQuantityFilters.Update(aggregatorMaxQuantityFilters);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Message = "Successfully updated status";
                    apiResponseViewModel.Successful = true;

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to update status [0003]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
