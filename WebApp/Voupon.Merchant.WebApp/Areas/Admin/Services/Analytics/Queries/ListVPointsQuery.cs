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
    public class ListVPointsQuery : ListQueryRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public class ListVPointsQueryHandler : IRequestHandler<ListVPointsQuery, ApiResponseViewModel>
        {
            VodusV2Context vodusV2Context;
            RewardsDBContext rewardsDBContext;
            public ListVPointsQueryHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ListVPointsQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var pointsEarn = await vodusV2Context.SurveyResponses.Where(x => x.CreatedAt >= request.From && x.CreatedAt < request.To.AddDays(1)).AsNoTracking().ToListAsync();
                    var pointsUsed = await rewardsDBContext.Orders.Where(x => x.CreatedAt >= request.From && x.CreatedAt < request.To.AddDays(1) && x.OrderStatus == 2 && x.TotalPoints > 0).AsNoTracking().ToListAsync();

                    var demoPoints = await vodusV2Context.MemberProfileExtensions.Where(x => x.CreatedAt >= request.From && x.CreatedAt < request.To.AddDays(1)).AsNoTracking().ToListAsync();
                    var psyPoints = await vodusV2Context.MemberPsychographics.Where(x => x.CreatedAt >= request.From && x.CreatedAt < request.To.AddDays(1)).AsNoTracking().ToListAsync();

                    var dates = Enumerable.Range(0, 1 + request.To.Subtract(request.From).Days).Select(offset => request.From.AddDays(offset)).ToArray();

                    var pointsEarnGroupByDate = pointsEarn.GroupBy(x => x.CreatedAt.Date);
                    var pointsUsedGroupByDate = pointsUsed.GroupBy(x => x.CreatedAt.Date);


                    var list = new List<PointsViewModel>();

                    foreach (var item in dates)
                    {

                        var totalPointEarn = pointsEarn.Where(x => x.CreatedAt.Date == item.Date).Sum(x => x.PointsCollected);
                        var totalPointEarnDemo = demoPoints.Where(x => x.CreatedAt.Date == item.Date).Count();
                        var totalPointEarnPsy = psyPoints.Where(x => x.CreatedAt.Date == item.Date).Count();

                        var totalPointUsed = pointsUsed.Where(x => x.CreatedAt.Date == item.Date).Sum(x => x.TotalPoints);
                        list.Add(new PointsViewModel
                        {
                            Date = item,
                            DateString = item.ToString("dddd, dd MMMM"),
                            TotalPointsEarn = totalPointEarn + totalPointEarnDemo + totalPointEarnPsy,
                            TotalPointsUsed = totalPointUsed
                        });
                    }

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = new ListView<PointsViewModel>(list.Count(), request.PageSize, request.PageIndex)
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


        public class PointsViewModel
        {
            public DateTime Date { get; set; }
            public string DateString { get; set; }
            public int TotalPointsEarn { get; set; }
            public int TotalPointsUsed { get; set; }
        }
    }
}
