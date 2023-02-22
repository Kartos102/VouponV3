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
    public class UpdateAdditionalDiscountCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public short Points { get; set; }

        public class UpdateAdditionalDiscountCommandHandler : IRequestHandler<UpdateAdditionalDiscountCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateAdditionalDiscountCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateAdditionalDiscountCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var additionalDiscounts = await rewardsDBContext.AdditionalDiscounts.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                    if (additionalDiscounts == null)
                    {
                        apiResponseViewModel.Message = "Fail to update additional discounts [0001]";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    additionalDiscounts.DiscountPercentage = request.DiscountPercentage;
                    additionalDiscounts.MaxPrice = request.MaxPrice;
                    additionalDiscounts.Points = request.Points;

                    rewardsDBContext.AdditionalDiscounts.Update(additionalDiscounts);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Message = "Successfully updated additional discounts";
                    apiResponseViewModel.Successful = true;

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to update additional discounts [0003]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
