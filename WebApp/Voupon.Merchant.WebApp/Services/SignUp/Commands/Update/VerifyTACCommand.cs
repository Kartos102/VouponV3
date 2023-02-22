using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.SignUp.Commands.Update
{
    public class VerifyTACCommand : VerifyTACCommandRequestViewModel, IRequest<ApiResponseViewModel>
    {
        private class VerifyTACCommandHandler : IRequestHandler<VerifyTACCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public VerifyTACCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(VerifyTACCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                var tempUserEntity = await rewardsDBContext.TempUsers.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

                if (tempUserEntity == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request[1]";
                    return apiResponseViewModel;
                }

                if (tempUserEntity.UserId.HasValue)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "User account already exists";
                    return apiResponseViewModel;
                }

                if (tempUserEntity.TACVerifiedAt.HasValue)
                {
                    apiResponseViewModel.Code = 99;
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Account already verified before";
                    return apiResponseViewModel;
                }

                if ((DateTime.Now - tempUserEntity.TACRequestedAt).TotalMinutes > 5 ? true : false)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "TAC has expired. Please request a new TAC";
                    return apiResponseViewModel;
                }

                if (request.TAC == tempUserEntity.TAC)
                {
                    tempUserEntity.TACVerifiedAt = DateTime.Now;
                    rewardsDBContext.TempUsers.Update(tempUserEntity);
                    await rewardsDBContext.SaveChangesAsync();
                    apiResponseViewModel.Successful = true;
                }
                else
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Incorrect TAC. Please re-enter";
                }
                return apiResponseViewModel;
            }
        }
    }

    public class VerifyTACCommandRequestViewModel
    {
        [Required]
        public string TAC { get; set; }
        [Required]
        public Guid Id { get; set; }
    }

    public class VerifyTACCommandResponseViewModel
    {
        public string Id { get; set; }


    }

}
