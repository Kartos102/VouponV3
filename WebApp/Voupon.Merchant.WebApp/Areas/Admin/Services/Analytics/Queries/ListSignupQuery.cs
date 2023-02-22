using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Analytics.Queries.List
{
    public class ListSignupQuery : ListQueryRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public class ListSignupQueryHandler : IRequestHandler<ListSignupQuery, ApiResponseViewModel>
        {
            VodusV2Context vodusV2Context;
            RewardsDBContext rewardsDBContext;
            public ListSignupQueryHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ListSignupQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var master = await vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.CreatedAt >= request.From && x.CreatedAt < request.To.AddDays(1)).AsNoTracking().ToListAsync();
                    var merchant = await rewardsDBContext.Merchants.Where(x => x.CreatedAt >= request.From && x.CreatedAt < request.To.AddDays(1)).AsNoTracking().ToListAsync();

                    var dates = Enumerable.Range(0, 1 + request.To.Subtract(request.From).Days).Select(offset => request.From.AddDays(offset)).ToArray();

                    var masterGroupByDate = master.GroupBy(x => x.CreatedAt.Date);
                    var merchantGroupByDate = merchant.GroupBy(x => x.CreatedAt.Date);


                    var list = new List<SignupViewModel>();

                    foreach (var item in dates)
                    {
                        var users = master.Where(x => x.CreatedAt.Date == item.Date);
                        var merchants = merchant.Where(x => x.CreatedAt.Date == item.Date);
                        var totalMerchantSignup = merchants.Count();
                        var merchantEmail = rewardsDBContext.UserClaims.Include(x => x.User).Where(x => x.ClaimType == "MerchantId" && merchants.Select(x => x.Id.ToString()).Contains(x.ClaimValue)).Select(x => x.User.Email).ToArray();

                        var totalUserSignup = users.Count();

                        list.Add(new SignupViewModel
                        {
                            Date = item,
                            DateString = item.ToString("dddd, dd MMMM"),
                            TotalUserSignup = totalUserSignup,
                            TotalMerchantSignup = totalMerchantSignup,
                            MerchantEmails = string.Join(",", merchantEmail),
                            UserEmails = string.Join(",", users.Select(x => x.User.Email))
                        });
                    }

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = new ListView<SignupViewModel>(list.Count(), request.PageSize, request.PageIndex)
                    {
                        Items = list
                    };
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;
                }
                return apiResponseViewModel;
            }
        }


        public class SignupViewModel
        {
            public DateTime Date { get; set; }
            public string DateString { get; set; }
            public int TotalUserSignup { get; set; }
            public int TotalMerchantSignup { get; set; }
            public string MerchantEmails { get; set; }
            public string UserEmails { get; set; }
        }
    }
}
