using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Common.Services.Orders.Queries;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.Orders.Queries
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MemberPointsQuery : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
        public class MemberPointsQueryHandler : IRequestHandler<MemberPointsQuery, ApiResponseViewModel>
        {
            RewardsDBContext _rewardsDBContext;
            VodusV2Context _vodusV2Context;
            public MemberPointsQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
            {
                _rewardsDBContext = rewardsDBContext;
                _vodusV2Context = vodusV2Context;
            }

            public async Task<ApiResponseViewModel> Handle(MemberPointsQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel
                {
                    Successful = false,
                    Data = null
                };

                var model = new MemberPointsViewModel();
                var master = await _vodusV2Context.MasterMemberProfiles.Where(x => x.Id == request.MasterMemberProfileId).FirstOrDefaultAsync();
                if(master == null)
                {
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

                var memberProfile = await _vodusV2Context.MemberProfiles.Where(x => x.Id == master.MemberProfileId).FirstOrDefaultAsync();
                if(memberProfile == null)
                {
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }
                
                var orders = await _rewardsDBContext.Orders.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId).AsNoTracking().ToListAsync();


                if (orders != null)
                {
                    model.TotalUsedPoints = orders.Where(x => x.OrderStatus  == 2).Sum(x => x.TotalPoints);
                    model.TotalOnHoldPoints = orders.Where(x => x.OrderStatus == 1).Sum(x => x.TotalPoints);
                }

                //  Refunds

                //var refunds = await _rewardsDBContext.Refunds.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId).AsNoTracking().ToListAsync();

                //Caputure Internal items refunds total points
                var refunds = await _rewardsDBContext.OrderItems
                .Include(x => x.Order)
                .Where(x => x.Status == 5 && x.Order.MasterMemberProfileId == request.MasterMemberProfileId)
                .AsNoTracking()
                .ToListAsync();


     


                if (refunds != null && refunds.Any())
                {
                    model.TotalRefundedPoints += refunds.Where(x => x.Status == 5).Sum(x => x.Points);
                }

             


                //  External refunds
                var externalRefunds = await _rewardsDBContext.RefundsExternalOrderItems.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId).AsNoTracking().ToListAsync();
                if(externalRefunds != null && externalRefunds.Any())
                {
                    model.TotalRefundedPoints += externalRefunds.Where(x => x.Status == 2).Sum(x => x.PointsRefunded);
                }


                model.TotalDemographicPoints = memberProfile.DemographicPoints;
                model.TotalPsyPoints += await _vodusV2Context.MemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == memberProfile.Id).CountAsync();
                model.TotalPsyPoints += await _vodusV2Context.DeletedMemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == memberProfile.Id).CountAsync();


                model.TotalBonusPoints = await _vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == master.Id && x.IsActive == true).SumAsync(x => x.Points);

                model.TotalSurveyPoints += await _vodusV2Context.SurveyResponses.Where(x => x.MemberProfileId == memberProfile.Id).SumAsync(x => x.PointsCollected);
                model.TotalSurveyPoints += await _vodusV2Context.DeletedSurveyResponses.AsNoTracking().Where(x => x.MemberProfileId == memberProfile.Id).SumAsync(x => x.PointsCollected);

                model.TotalAvailablePoints = (model.TotalBonusPoints + model.TotalDemographicPoints + model.TotalPsyPoints + model.TotalSurveyPoints + model.TotalRefundedPoints) - (model.TotalUsedPoints + model.TotalUsedPointsLegacy);

                apiResponseViewModel.Data = model;
                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
        }

        public class MemberPointsViewModel
        {
            public int TotalPsyPoints { get; set; }
            public int TotalBonusPoints { get; set; }
            public int TotalUsedPointsLegacy { get; set; }
            public int TotalUsedPoints { get; set; }
            public int TotalOnHoldPoints { get; set; }
            public int TotalRefundedPoints { get; set; }
            public int TotalAvailablePoints { get; set; }
            public int TotalSurveyPoints { get; set; }
            public int TotalDemographicPoints { get; set; }
        }
    }
}
