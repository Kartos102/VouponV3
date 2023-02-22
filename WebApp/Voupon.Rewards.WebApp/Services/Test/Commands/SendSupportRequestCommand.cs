using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Test.Commands
{
    public class SendSupportRequestCommand : IRequest<ApiResponseViewModel>
    {
        public string Ip { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }

        private class SendSupportRequestCommandHandler : IRequestHandler<SendSupportRequestCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;

            public SendSupportRequestCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(SendSupportRequestCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var techTeamEmail = "kok.hong@vodus.my, osa@vodus.my, kelvin.goh@vodus.com";

                var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                var msg = new SendGridMessage();
                msg.SetTemplateId("d3071f81-7e2d-4e0f-9f1d-4e6ebcab4d96");
                msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                msg.SetSubject("Support request at Vodus.my (Voupon)");

                var techEmail = techTeamEmail.Split(",");
                for (var email = 0; email < techEmail.Length; email++)
                {
                    msg.AddTo(new EmailAddress(techEmail[email].Trim()));
                }
                //msg.AddTo(new EmailAddress("kok.hong@vodus.my"));

                var message = "Email: " + request.Email;
                message += "<br/><br/>IP: " + request.Ip;
                message += "<br/><br/>token: " + request.Token;

                msg.AddSubstitution("-EmailBody-", message);
                var response = sendGridClient.SendEmailAsync(msg).Result;

                return apiResponseViewModel;
            }
        }
    }


}
