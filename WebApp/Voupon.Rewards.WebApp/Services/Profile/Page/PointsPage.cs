using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Profile.Page
{
    public class PointsPage : IRequest<ApiResponseViewModel>
    {
        public string Username { get; set; }
        private class PointsPageHandler : IRequestHandler<PointsPage, ApiResponseViewModel>
        {
            VodusV2Context vodusV2Context;
            RewardsDBContext rewardsDBContext;
            public PointsPageHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(PointsPage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                var masterData = await vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.User.UserName == request.Username).FirstOrDefaultAsync();

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
                    PointsOnHold = 0
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

                //Correction for bonus points
                
                var bonus = await vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == masterData.Id && x.IsActive == true).ToListAsync();
                if (bonus != null && bonus.Any())
                {
                    viewModel.BonusPoints = bonus.Sum(x => x.Points);
                }

                var usedPoints = 0;
                //var v2Orders = await vodusV2Context.Orders.Where(x => x.MasterMemberId == masterData.Id).ToListAsync();
                var orders = await rewardsDBContext.Orders.Where(x => x.MasterMemberProfileId == masterData.Id).ToListAsync();



                //if (v2Orders != null && v2Orders.Any())
                //{
                //    usedPoints += v2Orders.Sum(x => x.TotalPoints);
                //}

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
        public int BonusPoints { get; set; }
        public int ResponseCollectedPoints { get; set; }
        public int UsedPoints { get; set; }

        public int PointsOnHold { get; set; }
    }
}
