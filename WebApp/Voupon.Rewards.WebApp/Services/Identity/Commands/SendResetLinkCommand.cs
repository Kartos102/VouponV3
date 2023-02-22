using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Identity.Commands
{



    public class SendResetLinkCommand : IRequest<ApiResponseViewModel>
    {
        public string Email { get; set; }

        public class SendResetLinkCommandHandler : IRequestHandler<SendResetLinkCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly UserManager<Users> userManager;
            private readonly IOptions<AppSettings> appSettings;

            private readonly SignInManager<Users> signInManager;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public SendResetLinkCommandHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, UserManager<Users> userManager, SignInManager<Users> signInManager)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;

                this.userManager = userManager;
                this.signInManager = signInManager;
            }

            public async Task<ApiResponseViewModel> Handle(SendResetLinkCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {

                    var user = await userManager.FindByEmailAsync(request.Email.Trim().ToLower());
                    if (user == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid Email";
                        return apiResponseViewModel;
                    }

                    var entity = new PasswordResets();
                    entity.Email = request.Email;
                    entity.CreatedAt = DateTime.Now;
                    entity.ExpireAt = DateTime.Now.AddHours(2);
                    entity.ResetCode = Guid.NewGuid().ToString();
                    vodusV2Context.PasswordResets.Add(entity);
                    vodusV2Context.SaveChanges();
                    if (entity.Id == 0)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Fail to generate reset link";
                        return apiResponseViewModel;
                    }
                    else
                    {
                        var resetUrl = appSettings.Value.App.BaseUrl + "/home/resetpassword?email=" + entity.Email + "&code=" + entity.ResetCode;
                        string apiKey = appSettings.Value.Mailer.Sendgrid.APIKey;

                        var sendGridClient = new SendGridClient(apiKey);
                        var msg = new SendGridMessage();
                        msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.ResetPassword);
                        msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                        msg.SetSubject("Reset password request");
                        msg.AddTo(new EmailAddress(entity.Email));
                        msg.AddSubstitution("-userName-", user.FirstName + " " + user.LastName);
                        msg.AddSubstitution("-resetPasswordUrl-", resetUrl);
                        var response = sendGridClient.SendEmailAsync(msg).Result;
                        apiResponseViewModel.Message = "Successfully send reset link to email";
                        apiResponseViewModel.Successful = true;
                    }



                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Incorrect email or password";
                    return apiResponseViewModel;
                }
            }

        }
    }

}
