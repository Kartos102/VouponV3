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
    public class HUTBlastEmailCommand : IRequest<ApiResponseViewModel>
    {
        public string Language { get; set; }
        private class CreateTempUserCommandHandler : IRequestHandler<HUTBlastEmailCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;

            public CreateTempUserCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(HUTBlastEmailCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                return apiResponseViewModel;
                /*
              var usersTest = await vodusV2Context.Users.Where(x => x.NormalizedEmail == null).ToListAsync();

              var total = usersTest.Count();

              foreach(var user in usersTest)
              {
                  user.NormalizedEmail = user.Email.Normalize();
                  user.NormalizedUserName = user.UserName.Normalize();
                  vodusV2Context.Users.Update(user);
              }

              await vodusV2Context.SaveChangesAsync();
*/

              var hut = await vodusV2Context.HUTSurveyParticipants.Where(x => x.ListTypeId == 2).ToListAsync();

              foreach(var item in hut)
              {
                  var user = await vodusV2Context.Users.Where(x => x.Email.ToLower() == item.Email.ToLower()).FirstOrDefaultAsync();
                  if(user != null)
                  {
                      var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.UserId == user.Id.ToString()).FirstOrDefaultAsync();
                      if (master == null)
                      {
                          continue;
                      }


                      item.UserId = master.Id;
                      vodusV2Context.HUTSurveyParticipants.Update(item);
                  }
              }
              await vodusV2Context.SaveChangesAsync();
              

                if (string.IsNullOrEmpty(request.Language))
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request [001]";
                    return apiResponseViewModel;
                }


                var sendGridClient2 = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                var msg2 = new SendGridMessage();
                msg2.SetTemplateId("62b99170-6606-460c-98dc-4780e51530db");
                msg2.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                msg2.SetSubject("Uji rasa kopi untuk mendapatkan kredit Touch N Go RM30");
                msg2.AddTo(new EmailAddress("vodus.media@hotmail.com"));
                msg2.AddSubstitution("-name-", $"Kelvin");
                msg2.AddSubstitution("-productTestUrl-", $"https://docs.google.com/forms/d/e/1FAIpQLScsN5JFUJXSZvI567cYMMPMJSm-S6_5pdRFum4m2SFojVWwVw/viewform?entry.1299733908=vodus.media@hotmail.com");
                var response2 = sendGridClient2.SendEmailAsync(msg2).Result;

                var sendGridClient3 = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                var msg3 = new SendGridMessage();
                msg3.SetTemplateId("ca19237c-d458-4c65-9577-2535e8ce10b4");
                msg3.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                msg3.SetSubject("Coffee Taste Test for RM30 Touch N Go Credit");
                msg3.AddTo(new EmailAddress("kelvin.goh@vodus.com"));
                msg3.AddSubstitution("-name-", $"Kelvin");
                msg3.AddSubstitution("-productTestUrl-", $"https://docs.google.com/forms/d/e/1FAIpQLSfmKTrbE8RV1aa6SNwuLKPdHvFm2y-8gRv2bX1oNYP7-N9gIA/viewform?entry.1971312561=kelvin.goh@vodus.com");
                var response3 = sendGridClient3.SendEmailAsync(msg3).Result;
                return null;
                var existingParticipants = await vodusV2Context.HUTSurveyParticipants.ToListAsync();

                var users = await vodusV2Context.Users.Include(x => x.MasterMemberProfiles).Where(x => x.MasterMemberProfiles.Any()).ToListAsync();

                var states = new List<int>();
                states.Add(201);
                states.Add(209);
                states.Add(210);
                states.Add(212);

                var emailedCount = 0;
                var checkedCount = 0;

                //  en
                if (request.Language == "ms")
                {
                    var selectedUsers = users.Where(x => !existingParticipants.Select(z => z.Email).Contains(x.Email) && (x.MasterMemberProfiles.First().PreferLanguage == "ms")).ToList();
                    foreach (var user in selectedUsers)
                    {
                        checkedCount++;
                        if (!user.MasterMemberProfiles.Any())
                        {
                            continue;
                        }

                        var memberProfile = vodusV2Context.MemberProfiles.FromSqlRaw(string.Format("Select top 1 * from memberprofiles WITH (NOLOCK) where mastermemberprofileid={0} and ismasterprofile=1", user.MasterMemberProfiles.First().Id)).FirstOrDefault();

                        if (memberProfile == null)
                        {
                            continue;
                        }

                        if (!memberProfile.DemographicStateId.HasValue)
                        {
                            continue;
                        }


                        if (!states.Contains(memberProfile.DemographicStateId.Value))
                        {
                            continue;
                        }

                        
                        var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                        var msg = new SendGridMessage();
                        msg.SetTemplateId("62b99170-6606-460c-98dc-4780e51530db");
                        msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                        msg.SetSubject("Uji rasa kopi untuk mendapatkan kredit Touch N Go RM30");
                        msg.AddTo(new EmailAddress(user.Email));
                        msg.AddSubstitution("-name-", $"{user.FirstName} {user.LastName}");
                        msg.AddSubstitution("-productTestUrl-", $"https://docs.google.com/forms/d/e/1FAIpQLScsN5JFUJXSZvI567cYMMPMJSm-S6_5pdRFum4m2SFojVWwVw/viewform?entry.1299733908={user.Email}");
                        var response = sendGridClient.SendEmailAsync(msg).Result;
                        
                        emailedCount++;
                    }
                    apiResponseViewModel.Message = $"{emailedCount} / {checkedCount} users emailed";
                }
                else
                {

                    var selectedUsers = users.Where(x => !existingParticipants.Select(z => z.Email).Contains(x.Email) && (x.MasterMemberProfiles.First().PreferLanguage != "ms")).ToList();
                    foreach (var user in selectedUsers)
                    {
                        checkedCount++;
                        if (!user.MasterMemberProfiles.Any())
                        {
                            continue;
                        }

                        var memberProfile = vodusV2Context.MemberProfiles.FromSqlRaw(string.Format("Select top 1 * from memberprofiles WITH (NOLOCK) where mastermemberprofileid={0} and ismasterprofile=1", user.MasterMemberProfiles.First().Id)).FirstOrDefault();

                        if (memberProfile == null)
                        {
                            continue;
                        }

                        if (!memberProfile.DemographicStateId.HasValue)
                        {
                            continue;
                        }

                        if (!states.Contains(memberProfile.DemographicStateId.Value))
                        {
                            continue;
                        }


                        var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                        var msg = new SendGridMessage();
                        msg.SetTemplateId("ca19237c-d458-4c65-9577-2535e8ce10b4");
                        msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                        msg.SetSubject("Coffee Taste Test for RM30 Touch N Go Credit");
                        msg.AddTo(new EmailAddress(user.Email));
                        msg.AddSubstitution("-name-", $"{user.FirstName} {user.LastName}");
                        msg.AddSubstitution("-productTestUrl-", $"https://docs.google.com/forms/d/e/1FAIpQLSfmKTrbE8RV1aa6SNwuLKPdHvFm2y-8gRv2bX1oNYP7-N9gIA/viewform?entry.1971312561={user.Email}");
                        var response = sendGridClient.SendEmailAsync(msg).Result;

                        emailedCount++;
                    }
                    apiResponseViewModel.Message = $"{emailedCount} / {checkedCount} users emailed";
                }

                return apiResponseViewModel;
            }
        }
    }


}
