using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Merchants.Commands
{
    public class UpdateMerchantTestAccountStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public bool Status { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }

        public class UpdateMerchantTestAccountStatusCommandHandler : IRequestHandler<UpdateMerchantTestAccountStatusCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateMerchantTestAccountStatusCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateMerchantTestAccountStatusCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    // var result = await UserManager.UpdateSecurityStampAsync(user.Id);
                    var merchant = await rewardsDBContext.Merchants.Where(x => x.Id == request.MerchantId).FirstOrDefaultAsync();
                    if (merchant == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [001]";
                        return apiResponseViewModel;
                    }
                    merchant.IsTestAccount = request.Status;
                    merchant.LastUpdatedAt = DateTime.Now;
                    merchant.LastUpdatedByUserId = request.LastUpdatedByUserId;

                    rewardsDBContext.Merchants.Update(merchant);
                    await rewardsDBContext.SaveChangesAsync();
                    apiResponseViewModel.Message = "Successfully updated test account status for " + merchant.DisplayName;
                    apiResponseViewModel.Successful = true;
                }
                catch (Exception ex)
                {
                    var errorLogs = new ErrorLogs
                    {
                        ActionName = "UpdateMerchantTestAccountStatusCommand",
                        TypeId = 2,
                        CreatedAt = DateTime.Now,
                        ActionRequest = JsonConvert.SerializeObject(request),
                        Errors = ex.ToString()
                    };

                    rewardsDBContext.ErrorLogs.Add(errorLogs);
                    await rewardsDBContext.SaveChangesAsync();


                    apiResponseViewModel.Message = "Fail to update merchant test account status [0003]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
