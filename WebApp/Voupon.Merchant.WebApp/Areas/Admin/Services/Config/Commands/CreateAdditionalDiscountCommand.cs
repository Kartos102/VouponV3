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
    public class CreateAdditionalDiscountCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public short Points { get; set; }
        public Guid UpdatedBy { get; set; }

        public class CreateAdditionalDiscountCommandHandler : IRequestHandler<CreateAdditionalDiscountCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public CreateAdditionalDiscountCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(CreateAdditionalDiscountCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var additionalDiscounts = new AdditionalDiscounts
                    {
                        MaxPrice = request.MaxPrice,
                        DiscountPercentage = request.DiscountPercentage,
                        Points = request.Points,
                        StatusId = 1,
                    };

                    await rewardsDBContext.AdditionalDiscounts.AddAsync(additionalDiscounts);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully created additional discounts";

                }
                catch (Exception ex)
                {
                    //  log
                    apiResponseViewModel.Message = "Fail to create additional discounts";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
