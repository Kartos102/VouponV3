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
    public class CreateCommand : ViewModels.PromoCode.PromoCodeViewModel, IRequest<ApiResponseViewModel>
    {

        public class CreateCommandHandler : IRequestHandler<CreateCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public CreateCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(CreateCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var promoCode = await rewardsDBContext.PromoCodes.Where(x => x.PromoCode == request.PromoCode).FirstOrDefaultAsync();
                    if (promoCode != null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Code already exist";
                        return apiResponseViewModel;
                    }

                    var newPromoCode = new PromoCodes
                    {
                        Id = Guid.NewGuid(),
                        PromoCode = request.PromoCode,
                        CreatedAt = DateTime.Now,
                        Description = request.Description,
                        DiscountType = request.DiscountType,
                        DiscountValue = request.DiscountValue,
                        ExpireOn = request.ExpireOn,
                        IsFirstTimeUserOnly = request.IsFirstTimeUserOnly,
                        IsNewSignupUserOnly = request.IsNewSignupUserOnly,
                        IsSelectedUserOnly = request.IsSelectedUserOnly,
                        MaxDiscountValue = request.MaxDiscountValue,
                        MinSpend = request.MinSpend,
                        Status = 1,
                        TotalAllowedPerUser = request.TotalAllowedPerUser,
                        TotalRedeemed = request.TotalRedeemed,
                        TotalRedemptionAllowed = request.TotalRedemptionAllowed,

                            //1781
                        IsShipCostDeduct = request.IsShipCostDeduct

                };

                    await rewardsDBContext.PromoCodes.AddAsync(newPromoCode);
                    await rewardsDBContext.SaveChangesAsync();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = $"Successfully created promo code: {newPromoCode.PromoCode}";
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to create [999]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
