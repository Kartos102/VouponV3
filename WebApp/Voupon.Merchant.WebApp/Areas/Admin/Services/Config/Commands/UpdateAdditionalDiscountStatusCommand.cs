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
    public class UpdateAdditionalDiscountStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public byte StatusId { get; set; }

        public class UpdateAdditionalDiscountStatusCommandHandler : IRequestHandler<UpdateAdditionalDiscountStatusCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateAdditionalDiscountStatusCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateAdditionalDiscountStatusCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var additionalDiscounts = await rewardsDBContext.AdditionalDiscounts.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                    if (additionalDiscounts == null)
                    {
                        apiResponseViewModel.Message = "Fail to update status [0001]";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    additionalDiscounts.StatusId = request.StatusId;

                    rewardsDBContext.AdditionalDiscounts.Update(additionalDiscounts);
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
