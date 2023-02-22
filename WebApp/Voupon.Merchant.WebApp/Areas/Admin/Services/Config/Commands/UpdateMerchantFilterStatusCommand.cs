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
    public class UpdateMerchantFilterStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public byte StatusId { get; set; }

        public class UpdateMerchantFilterStatusCommandHandler : IRequestHandler<UpdateMerchantFilterStatusCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateMerchantFilterStatusCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateMerchantFilterStatusCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var aggregatorExcludeMerchants = await rewardsDBContext.AggregatorExcludeMerchants.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                    if (aggregatorExcludeMerchants == null)
                    {
                        apiResponseViewModel.Message = "Fail to update status [0001]";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    aggregatorExcludeMerchants.StatusId = request.StatusId;

                    rewardsDBContext.AggregatorExcludeMerchants.Update(aggregatorExcludeMerchants);
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
