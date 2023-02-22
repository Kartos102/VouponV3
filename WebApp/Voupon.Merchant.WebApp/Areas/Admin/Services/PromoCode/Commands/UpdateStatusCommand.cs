using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.PromoCode.Commands
{
    public class UpdateStatusCommand : IRequest<ApiResponseViewModel>
    {
        public string Id { get; set; }
        public bool Status { get; set; }

        public class UpdateStatusCommandHandler : IRequestHandler<UpdateStatusCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateStatusCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var promoCode = await rewardsDBContext.PromoCodes.Where(x => x.Id == new Guid(request.Id)).FirstOrDefaultAsync();
                    if (promoCode == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [001]";
                        return apiResponseViewModel;
                    }

                    if (request.Status)
                    {
                        promoCode.Status = 1;
                    }
                    else
                    {
                        promoCode.Status = 2;
                    }

                    rewardsDBContext.PromoCodes.Update(promoCode);
                    await rewardsDBContext.SaveChangesAsync();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = $"Successfully updated promo code status for {promoCode.PromoCode}";
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to update status [999]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
