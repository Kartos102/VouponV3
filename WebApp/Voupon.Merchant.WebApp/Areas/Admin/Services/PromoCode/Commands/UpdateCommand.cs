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
    public class UpdateCommand : ViewModels.PromoCode.PromoCodeViewModel, IRequest<ApiResponseViewModel>
    {

        public class UpdateCommandHandler : IRequestHandler<UpdateCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var promoCode = await rewardsDBContext.PromoCodes.Where(x => x.Id.ToString() == request.GeneratedId).FirstOrDefaultAsync();
                    if (promoCode == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request";
                        return apiResponseViewModel;
                    }

                    promoCode.PromoCode = request.PromoCode;
                    promoCode.LastUpdatedAt = DateTime.Now;
                    promoCode.Description = request.Description;
                    //promoCode.DiscountType = request.DiscountType;
                    promoCode.DiscountValue = request.DiscountValue;
                    promoCode.ExpireOn = request.ExpireOn;
                    promoCode.IsFirstTimeUserOnly = request.IsFirstTimeUserOnly;
                    promoCode.IsNewSignupUserOnly = request.IsNewSignupUserOnly;
                    promoCode.IsSelectedUserOnly = request.IsSelectedUserOnly;
                    promoCode.MaxDiscountValue = request.MaxDiscountValue;
                    promoCode.MinSpend = request.MinSpend;
                    promoCode.Status = request.Status;
                    promoCode.TotalAllowedPerUser = request.TotalAllowedPerUser;
                    promoCode.TotalRedeemed = request.TotalRedeemed;
                    promoCode.TotalRedemptionAllowed = request.TotalRedemptionAllowed;

                    //1781
                    promoCode.IsShipCostDeduct = request.IsShipCostDeduct;

                    rewardsDBContext.PromoCodes.Update(promoCode);
                    await rewardsDBContext.SaveChangesAsync();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = $"Successfully updated promo code: {promoCode.PromoCode}";
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
