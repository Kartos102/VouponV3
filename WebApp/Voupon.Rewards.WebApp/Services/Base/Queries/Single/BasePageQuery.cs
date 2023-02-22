using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Encryption;
using Voupon.Database.Postgres.VodusEntities;

namespace Voupon.Rewards.WebApp.Services.Base.Queries.Single
{
    public class BasePageQuery : IRequest<BasePageQueryViewModel>
    {
        public long MemberProfileId { get; set; }

        public string UserName { get; set; }
    }

    public class BankAccountQueryHandler : IRequestHandler<BasePageQuery, BasePageQueryViewModel>
    {
        VodusV2Context vodusV2Context;
        IOptions<AppSettings> appSettings;
        public BankAccountQueryHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
        {
            this.vodusV2Context = vodusV2Context;
            this.appSettings = appSettings;
        }

        public async Task<BasePageQueryViewModel> Handle(BasePageQuery request, CancellationToken cancellationToken)
        {
            var allLanguage = await vodusV2Context.Languages.AsNoTracking().ToListAsync();

            var itemTypes = new List<SelectListItem>();
            var preferredLanguage = "";
            itemTypes.Add(new SelectListItem { Text = "-- Select --", Value = "0" });
            foreach (var item in allLanguage)
            {
                if (preferredLanguage == item.LanguageCode)
                {
                    itemTypes.Add(new SelectListItem { Text = item.LanguageDisplayName, Value = item.LanguageCode, Selected = true });
                }
                else
                {
                    itemTypes.Add(new SelectListItem { Text = item.LanguageDisplayName, Value = item.LanguageCode });
                }
            }

            var variable = await vodusV2Context.Variables.FirstOrDefaultAsync();

            var viewModel = new BasePageQueryViewModel
            {
                AvailablePoints = 0,
                AccountEmail = "",
                AccountName = "",
                PreferredLanguage = "",
                ApiUrl = appSettings.Value.App.APIUrl,
                BaseUrl = appSettings.Value.App.BaseUrl,
                CDNUrl = appSettings.Value.App.CDNUrl,
                ServerlessUrl = appSettings.Value.App.ServerlessUrl,
                IsFingerprintingEnabled = variable.IsFingerprintingEnabled,
                AllLanguages = itemTypes
            };

            if (!string.IsNullOrEmpty(request.UserName))
            {
                var user = await vodusV2Context.Users.Include(x => x.MasterMemberProfiles).AsNoTracking().Where(x => x.UserName == request.UserName).FirstOrDefaultAsync();
                if (user != null)
                {
                    var master = user.MasterMemberProfiles.FirstOrDefault();
                    if (master != null)
                    {
                        viewModel.AccountEmail = user.Email;
                        viewModel.AccountName = (user.FirstName == null ? "" : (user.FirstName + " ")) + (user.LastName == null ? "" : user.LastName);
                        viewModel.AvailablePoints = master.AvailablePoints;
                        viewModel.PreferredLanguage = master.PreferLanguage == null ? "0" : master.PreferLanguage;

                        viewModel.ChatToken = new ChatToken
                        {
                            Email = user.Email,
                            CreatedAt = DateTime.Now

                        }.ToChatTokenValue();
                    }

                }
            }
            else
            {
                if (request.MemberProfileId > 0)
                {
                    var member = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == request.MemberProfileId).FirstOrDefaultAsync();
                    if (member != null)
                    {
                        if (member.SyncMemberProfileId.HasValue)
                        {
                            member = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == (member.SyncMemberProfileId.Value)).FirstOrDefaultAsync();
                        }

                        var master = await vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.Id == member.MasterMemberProfileId).FirstOrDefaultAsync();
                        if (master == null)
                        {
                            var psyPoints = vodusV2Context.MemberPsychographics.Where(x => x.MemberProfileId == request.MemberProfileId).Count();
                            viewModel.AvailablePoints = member.AvailablePoints + member.DemographicPoints + psyPoints;
                        }
                        else
                        {
                            viewModel.AvailablePoints = master.AvailablePoints;
                            viewModel.AccountEmail = master.User.Email;
                            viewModel.AccountName = (master.User.FirstName == null ? "" : (master.User.FirstName + " ")) + (master.User.LastName == null ? "" : master.User.LastName);
                            viewModel.PreferredLanguage = master.PreferLanguage == null ? "0" : master.PreferLanguage;

                            viewModel.ChatToken = new ChatToken
                            {
                                Email = master.User.Email,
                                CreatedAt = DateTime.Now

                            }.ToChatTokenValue();
                        }

                    }
                }

            }
            return viewModel;
        }
    }

    public class BasePageQueryViewModel
    {
        public int AvailablePoints { get; set; }
        public string AccountEmail { get; set; }
        public string AccountName { get; set; }
        public string PreferredLanguage { get; set; }

        public string BaseUrl { get; set; }
        public string CDNUrl { get; set; }

        public string ServerlessUrl { get; set; }
        public string ApiUrl { get; set; }
        public string ChatAPIUrl { get; set; }
        public bool IsFingerprintingEnabled { get; set; }
        public string ChatToken { get; set; }
        public List<SelectListItem> AllLanguages { get; set; }
    }
}