using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Database.Postgres.VodusEntities;
using System.Threading;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Options;

namespace Voupon.Rewards.WebApp.Services.Profile.Queries
{
    public class SendAccountVerificationEmail : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }

        public string Email { get; set; }
        public string From { get; set; }
    }

    public class GetUserDetailsByEmailHandler : IRequestHandler<SendAccountVerificationEmail, ApiResponseViewModel>
    {
        VodusV2Context VodusV2Context;
        private IOptions<AppSettings> appSettings;

        public GetUserDetailsByEmailHandler(VodusV2Context vodusV2, IOptions<AppSettings> appSettings)
        {
            this.VodusV2Context = vodusV2;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(SendAccountVerificationEmail request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();

            var user = VodusV2Context.Users.Where(x => x.Email == request.Email).FirstOrDefault();
            if (user != null)
            {
                var userName = "";
                if (user.FirstName != null)
                {
                    userName += user.FirstName;
                }

                if (user.LastName != null)
                {
                    userName += " " + user.LastName;
                }

                if (string.IsNullOrEmpty(user.ActivationCode))
                {
                    user.ActivationCode = Guid.NewGuid().ToString();
                }

                var url = appSettings.Value.App.BaseUrl + "/ConfirmAccountVerification/" + request.Email + "/" + user.ActivationCode + (!string.IsNullOrEmpty(request.From) ? "?from=" + Uri.UnescapeDataString(request.From) : "");

                var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                var msg = new SendGridMessage();
                msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.VerifyEmail);
                msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                msg.SetSubject("Email verification");
                msg.AddTo(new EmailAddress(request.Email));
                msg.AddSubstitution("-userName-", userName);
                msg.AddSubstitution("-verificationUrl-", url);
                var response = sendGridClient.SendEmailAsync(msg).Result;

                //  Update user email
                user.NewPendingVerificationEmail = request.Email;
                VodusV2Context.Users.Update(user);
                VodusV2Context.SaveChanges();

                apiResponseViewModel.Message = "Email sent successfully";
                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
            else
            {
                apiResponseViewModel.Message = "Email is not correct ";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }
        }
    }



}
