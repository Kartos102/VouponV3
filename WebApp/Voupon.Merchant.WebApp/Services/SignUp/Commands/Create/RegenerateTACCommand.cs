using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Voupon.Common.Google;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Voupon.Common.SMS.SMSS360;

namespace Voupon.Merchant.WebApp.Services.SignUp.Commands.Create
{
    public class RegenerateTACCommand : CreateTempUserCommandRequestViewModel, IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }
    }

    public class RegenerateTACCommandHandler : IRequestHandler<RegenerateTACCommand, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        private readonly ISMSS360 smss360;

        public RegenerateTACCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings, ISMSS360 smss360)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
            this.smss360 = smss360;
        }

        public async Task<ApiResponseViewModel> Handle(RegenerateTACCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();

            var tempUserEntity = await rewardsDBContext.TempUsers.Where(x => x.Id == request.Id).FirstOrDefaultAsync();

            if (tempUserEntity == null)
            {
                apiResponseViewModel.Message = "Invalid request";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }

            if (tempUserEntity.UserId.HasValue)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Account is already registered. Try logging in instead.";
                return apiResponseViewModel;
            }
            else
            {
                if (tempUserEntity.TACVerifiedAt.HasValue)
                {
                    if (tempUserEntity.UserId.HasValue)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Account already exists. Please login instead";
                        return apiResponseViewModel;
                    }
                    else
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Account already verified. Please setup password instead";
                        return apiResponseViewModel;
                    }
                }
            }

            var interval = (DateTime.Now - tempUserEntity.TACRequestedAt).TotalSeconds;

            if (interval < 300 ? true : false)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Request interval is not met. Please wait for " + Convert.ToInt32(300 - interval) + " more seconds";
                return apiResponseViewModel;
            }

            Random r = new Random();
            int randNum = r.Next(1000000);
            string tac = randNum.ToString("D6");

            tempUserEntity.TAC = tac;
            tempUserEntity.TACRequestedAt = DateTime.Now;

            rewardsDBContext.TempUsers.Update(tempUserEntity);


            var result = await rewardsDBContext.SaveChangesAsync();
            if (result > 0)
            {
                
                var message = string.Format(SMSS360.Templates.Template1, tac);
                var smsResult = await smss360.SendMessage(request.MobileNumber, message, Guid.NewGuid().ToString());
                if (smsResult != "0" && smsResult != "1606")
                {
                    //  sms sending failed
                    apiResponseViewModel.Message = "Something wasnt right. Please try again later: " + smsResult;
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }
                
                apiResponseViewModel.Successful = true;
            }
            else
            {
                apiResponseViewModel.Message = "Something wasnt right. Please try again later";
                apiResponseViewModel.Successful = false;
            }
            return apiResponseViewModel;
        }
    }
}