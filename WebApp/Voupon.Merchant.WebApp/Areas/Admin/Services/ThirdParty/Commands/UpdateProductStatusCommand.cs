using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.ThirdParty.Commands
{
    public class UpdateProductStatusCommand : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }

        public class UpdateProductStatusCommandHandler : IRequestHandler<UpdateProductStatusCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateProductStatusCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateProductStatusCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    // var result = await UserManager.UpdateSecurityStampAsync(user.Id);
                    var product = await rewardsDBContext.ThirdPartyProducts.Where(x=>x.Id == request.Id).FirstOrDefaultAsync();
                    if (product == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [001]";
                        return apiResponseViewModel;
                    }

                    if(request.IsActive)
                    {
                        product.Status = 1;
                    }
                    else
                    {
                        product.Status = 2;
                    }

                    rewardsDBContext.ThirdPartyProducts.Update(product);
                    await rewardsDBContext.SaveChangesAsync();
                    apiResponseViewModel.Successful = true;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to update config [0003]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
