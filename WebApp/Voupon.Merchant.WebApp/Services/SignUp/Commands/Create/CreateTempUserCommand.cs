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
    public class CreateTempUserCommand : CreateTempUserCommandRequestViewModel, IRequest<ApiResponseViewModel>
    {


        public class CreateTempUserCommandHandler : IRequestHandler<CreateTempUserCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            private readonly ISMSS360 smss360;

            public CreateTempUserCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings, ISMSS360 smss360)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
                this.smss360 = smss360;
            }

            public async Task<ApiResponseViewModel> Handle(CreateTempUserCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                //  Verify Google Recaptcha
                var reCaptchaResult = ReCaptchaV2.Validate(request.GoogleRecaptchaResponse, appSettings.Value.GoogleReCaptcha.Secretkey);
                if (!reCaptchaResult)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Hmm... Something is not right. Please try again.";
                    return apiResponseViewModel;
                }

                var userEntity = await rewardsDBContext.Users.Where(x => x.NormalizedEmail == request.Email).FirstOrDefaultAsync();
                if (userEntity != null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Email is already registered. Try logging in instead.";
                    return apiResponseViewModel;
                }
                else
                {
                    Random r = new Random();
                    int randNum = r.Next(1000000);
                    string tac = randNum.ToString("D6");

                    var tempUser = new TempUsers
                    {
                        Id = Guid.NewGuid(),
                        BusinessName = request.BusinessName,
                        CreatedAt = DateTime.Now,
                        Email = request.Email,
                        MobileNumber = request.MobileNumber,
                        TAC = tac,
                        TACRequestedAt = DateTime.Now,
                        CountryId = request.CountryId
                    };

                    await rewardsDBContext.TempUsers.AddAsync(tempUser);
                    var result = await rewardsDBContext.SaveChangesAsync();
                    if (result > 0)
                    {
                        
                        var message = string.Format(SMSS360.Templates.Template1, tac);
                        var smsResult = await smss360.SendMessage(request.MobileNumber, message, Guid.NewGuid().ToString());
                        if (smsResult != "0" && smsResult != "1606")
                        {
                            //  sms sending failed
                            apiResponseViewModel.Message = "Hmmm. TAC fails:" + smsResult;
                            apiResponseViewModel.Successful = false;
                            return apiResponseViewModel;
                        }
                        
                        apiResponseViewModel.Data = new CreateTempUserCommandReponseViewModel
                        {
                            Id = tempUser.Id.ToString()
                        };
                        apiResponseViewModel.Successful = true;
                    }
                    else
                    {
                        apiResponseViewModel.Message = "Something wasnt right. Please try again later";
                        apiResponseViewModel.Successful = false;
                    }
                }
                return apiResponseViewModel;
            }
        }
    }

    public class CreateTempUserCommandRequestViewModel
    {
        [Required]
        public int CountryId { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string BusinessName { get; set; }

        [JsonPropertyName("g-recaptcha-response")]
        public string GoogleRecaptchaResponse { get; set; }
    }

    public class CreateTempUserCommandReponseViewModel
    {
        public string Id { get; set; }
    }

}