using MediatR;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Queries
{
    public class PointsQuery : IRequest<ApiResponseViewModel>
    {
        public string UserId { get; set; }
        private class PointsQueryHandler : IRequestHandler<PointsQuery, ApiResponseViewModel>
        {
            VodusV2Context vodusV2Context;
            RewardsDBContext rewardsDBContext;
            public PointsQueryHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(PointsQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                var masterData = await vodusV2Context
                    .MasterMemberProfiles
                    .Include(x => x.User)
                    .Where(x => x.UserId == request.UserId)
                    .FirstOrDefaultAsync();
                
                if (masterData == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request [001]";
                    return apiResponseViewModel;
                }

                var viewModel = new ProfilePointsViewModel
                {
                    AvailablePoints = masterData.AvailablePoints,
                    BonusPoints = 0,
                    DemographicPoints = 0,
                    ResponseCollectedPoints = 0,
                    UsedPoints = 0,
                    PointsOnHold = 0,
                    TotalPsyPoints = 0,
                    TotalSurveyPoints = 0
                };

                if (masterData.MemberProfileId.HasValue)
                {
                    var member = await vodusV2Context.MemberProfiles.Where(x => x.Id == masterData.MemberProfileId).FirstOrDefaultAsync();
                    if (member != null)
                    {
                        viewModel.DemographicPoints = member.DemographicPoints;

                        var memberResponses = await vodusV2Context.SurveyResponses.Where(x => x.MemberProfileId == member.Id).ToListAsync();

                        if (memberResponses != null && memberResponses.Any())
                        {
                            viewModel.ResponseCollectedPoints = memberResponses.Sum(x => x.PointsCollected);
                        }
                    }
                }
                else
                {   //   This is a fallback for member's master that's is not updated with memberprofileid yet. [slow querying]
                    var member = await vodusV2Context.MemberProfiles.Where(x => x.MasterMemberProfileId == masterData.Id).FirstOrDefaultAsync();
                    if (member != null)
                    {
                        viewModel.DemographicPoints = member.DemographicPoints;

                        var memberResponses = await vodusV2Context.SurveyResponses.Where(x => x.MemberProfileId == member.Id).ToListAsync();

                        if (memberResponses != null && memberResponses.Any())
                        {
                            viewModel.ResponseCollectedPoints = memberResponses.Sum(x => x.PointsCollected);
                        }
                    }
                }

                var bonus = await vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == masterData.Id).ToListAsync();
                if (bonus != null && bonus.Any())
                {
                    viewModel.BonusPoints = bonus.Sum(x => x.Points);
                }

                var usedPoints = 0;
                var v2Orders = await vodusV2Context.Orders.Where(x => x.MasterMemberId == masterData.Id).ToListAsync();
                var orders = await rewardsDBContext.Orders.Where(x => x.MasterMemberProfileId == masterData.Id).ToListAsync();

                if (v2Orders != null && v2Orders.Any())
                {
                    usedPoints += v2Orders.Sum(x => x.TotalPoints);
                }

                if (orders != null && orders.Any())
                {
                    var orderCompleted = orders.Where(x => x.OrderStatus == 2);
                    var orderPendingPayment = orders.Where(x => x.OrderStatus == 1);
                    if (orderCompleted != null && orderCompleted.Any())
                    {
                        usedPoints += orderCompleted.Sum(x => x.TotalPoints);
                    }

                    if (orderPendingPayment != null && orderPendingPayment.Any())
                    {
                        viewModel.PointsOnHold += orderPendingPayment.Sum(x => x.TotalPoints);
                    }
                }

                viewModel.UsedPoints = usedPoints;


                //Based on MemberPointsQuery.cs 
                viewModel.TotalPsyPoints += await vodusV2Context.MemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == masterData.Id).CountAsync();
                viewModel.TotalPsyPoints += await vodusV2Context.DeletedMemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == masterData.Id).CountAsync();

                viewModel.TotalSurveyPoints += await vodusV2Context.SurveyResponses.Where(x => x.MemberProfileId == masterData.Id).SumAsync(x => x.PointsCollected);
                viewModel.TotalSurveyPoints += await vodusV2Context.DeletedSurveyResponses.AsNoTracking().Where(x => x.MemberProfileId == masterData.Id).SumAsync(x => x.PointsCollected);


                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Data = viewModel;
                return apiResponseViewModel;
            }
        }
    }

    public class ProfilePointsViewModel
    {
        public int AvailablePoints { get; set; }
        public int DemographicPoints { get; set; }
        public int TotalSurveyPoints { get; set; }
        public int BonusPoints { get; set; }
        public int ResponseCollectedPoints { get; set; }
        public int UsedPoints { get; set; }
        public int TotalPsyPoints { get; set; }
        public int PointsOnHold { get; set; }
    }
}
