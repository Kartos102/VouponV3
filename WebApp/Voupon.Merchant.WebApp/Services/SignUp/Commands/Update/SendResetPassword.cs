using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
//using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.SignUp.Commands.Update
{
    public class SendResetPasswordCommand : SendResetPasswordCommandRequestViewModel, IRequest<ApiResponseViewModel>
    {
        private class SendResetPasswordCommandHandler : IRequestHandler<SendResetPasswordCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            private readonly UserManager<Users> userManager;
            private readonly SignInManager<Users> signInManager;
            private readonly IOptions<AppSettings> appSettings;
            public SendResetPasswordCommandHandler(RewardsDBContext rewardsDBContext, UserManager<Users> userManager, SignInManager<Users> signInManager, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.userManager = userManager;
                this.signInManager = signInManager;
                this.appSettings = appSettings;

            }

            public async Task<ApiResponseViewModel> Handle(SendResetPasswordCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {
                    rewardsDBContext.Database.BeginTransaction();

                    var userEntity = await rewardsDBContext.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync();

                    if (userEntity == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid email";
                        return apiResponseViewModel;
                    }
                    var entity = new PasswordResets();
                    entity.Email = request.Email;
                    entity.CreatedAt = DateTime.Now;
                    entity.ExpireAt = DateTime.Now.AddHours(2);
                    entity.ResetCode = Guid.NewGuid().ToString();

                    var passwordResetEntity = await rewardsDBContext.PasswordResets.AddAsync(entity);
                    if (passwordResetEntity == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Fial to Create reset Password";
                        return apiResponseViewModel;
                    }

                    var resetUrl = appSettings.Value.App.BaseUrl + "/resetpassword?email=" + request.Email + "&code=" + entity.ResetCode;
                    //  Sent error email using sendgrid
                    string apiKey = appSettings.Value.Mailer.Sendgrid.APIKey;


                    var sendGridClient = new SendGridClient(apiKey);
                    var msg = new SendGridMessage();
                    msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.ResetPasswordMerchant);
                    msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
                    msg.SetSubject("Reset password request");
                    msg.AddTo(new EmailAddress(userEntity.Email));
                    msg.AddSubstitution("-userName-", "" );
                    msg.AddSubstitution("-resetPasswordUrl-", resetUrl);
                    var response = sendGridClient.SendEmailAsync(msg).Result;


                    await rewardsDBContext.SaveChangesAsync();
                    rewardsDBContext.Database.CommitTransaction();
                    apiResponseViewModel.Successful = true;

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to complete reset password. Please contact support team.";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }
    }

    public class SendResetPasswordCommandRequestViewModel
    {
        [Required]
        public string Email { get; set; }
        //[Required]
        //public Guid Id { get; set; }

    }

    public class SendResetPasswordCommandResponseViewModel
    {
        public Guid Id { get; set; }
    }

}
