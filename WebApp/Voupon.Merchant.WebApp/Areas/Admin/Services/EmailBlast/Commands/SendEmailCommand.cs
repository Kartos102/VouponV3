using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Areas.Admin.Services.EmailBlast.ViewModels;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.EmailBlast.Commands
{
    public class SendEmailCommand : IRequest<ApiResponseViewModel>
    {
       public SendEmailViewModel EmailViewModel { get; set; }

        public class SendEmailCommandHandler : IRequestHandler<SendEmailCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            private readonly VodusV2Context vodusV2Context;
            private readonly IOptions<AppSettings> appSettings;


            public SendEmailCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(SendEmailCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                    
                    if (request.EmailViewModel.EmailType == 1)
                    {
                        var toEmails = await vodusV2Context.Users.AsNoTracking().Where(x=> x.EmailConfirmed == true).ToListAsync();
                        for (int i = 0; i < toEmails.Count; i++)
                        {
                            var emailContent = "";
                            var msg = new SendGridMessage();

                            msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.MerchantsAndRespondentGeneralEmail);
                            msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
                            msg.SetSubject(request.EmailViewModel.EmailTitle);
                            emailContent = request.EmailViewModel.EmailContent.Replace("-firstname-", toEmails[i].FirstName);
                            msg.AddSubstitution("-content-", emailContent);
                            msg.AddTo(new EmailAddress(toEmails[i].Email));
                            var response = sendGridClient.SendEmailAsync(msg).Result;
                        }
                    }
                    else if (request.EmailViewModel.EmailType == 2)
                    {
                        var toEmails = await rewardsDBContext.Users.AsNoTracking().Include(x=> x.UserRoles).ThenInclude(x=>x.Merchant).ThenInclude(x=> x.PersonInCharges).ToListAsync();

                        for (int i = 0; i < toEmails.Count; i++)
                        {
                            var emailContent = "";
                            var msg = new SendGridMessage();

                            msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.MerchantsAndRespondentGeneralEmail);
                            msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
                            msg.SetSubject(request.EmailViewModel.EmailTitle);
                            msg.AddTo(new EmailAddress(toEmails[i].Email));
                            if (toEmails[i].UserRoles.FirstOrDefault().Merchant.PersonInCharges.FirstOrDefault().Name != null)
                                emailContent = request.EmailViewModel.EmailContent.Replace("-firstname-", toEmails[i].UserRoles.FirstOrDefault().Merchant.PersonInCharges.FirstOrDefault().Name);
                            else
                                emailContent = request.EmailViewModel.EmailContent.Replace("-firstname-", "");
                            msg.AddSubstitution("-content-", emailContent);

                            var response = sendGridClient.SendEmailAsync(msg).Result;
                        }
                    }
                    else
                    {
                        var Username = await vodusV2Context.Users.AsNoTracking().Where(x => x.NormalizedEmail == request.EmailViewModel.TestEmail.ToUpper()).Select(x => x.FirstName).FirstOrDefaultAsync();
                        if (Username == null)
                            Username = await rewardsDBContext.Users.AsNoTracking().Include(x => x.UserRoles).ThenInclude(x => x.Merchant).ThenInclude(x => x.PersonInCharges).Where(x => x.NormalizedEmail == request.EmailViewModel.TestEmail.ToUpper()).Select(x => x.UserRoles.FirstOrDefault().Merchant.PersonInCharges.FirstOrDefault().Name).FirstOrDefaultAsync();

                        if (Username == null)
                            Username = "";

                        var msg = new SendGridMessage();
                        msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.MerchantsAndRespondentGeneralEmail);
                        msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
                        msg.SetSubject(request.EmailViewModel.EmailTitle);
                        msg.AddSubstitution("-content-", request.EmailViewModel.EmailContent.Replace("-firstname-", Username));
                        msg.AddTo(new EmailAddress(request.EmailViewModel.TestEmail));
                        var response = sendGridClient.SendEmailAsync(msg).Result;
                    }

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Emails sent successfully";
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to send Emails [0001]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
