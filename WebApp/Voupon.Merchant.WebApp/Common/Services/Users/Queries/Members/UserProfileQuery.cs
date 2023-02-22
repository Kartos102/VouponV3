using MediatR;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.Users;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Queries.Members
{
    public class UserProfileQuery : IRequest<ApiResponseViewModel>
    {
        public string UserId { get; set; }
    }

    public class UserProfileQueryHandler : IRequestHandler<UserProfileQuery, ApiResponseViewModel>
    {
        VodusV2Context _vodusContext;
        public UserProfileQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusContext)
        {
            _vodusContext = vodusContext;
        }

        public async Task<ApiResponseViewModel> Handle(UserProfileQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var user = await _vodusContext.Users
                    .Where(m => m.Id == request.UserId)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    var mastermemberProfile = await _vodusContext.MasterMemberProfiles.Where(m => m.UserId == user.Id.ToString()).FirstOrDefaultAsync();
                    if (mastermemberProfile == null)
                    {
                        //return response;
                    }
                    //var memberProfile = await _vodusContext.MasterMemberProfiles.Where(m => m.Email == user.Email).FirstOrDefaultAsync();
                    var token = await _vodusContext.UserTokens.Where(m => m.UserId == user.Id.ToString()).FirstOrDefaultAsync();

                    //var MemberProfileExt = _vodusContext.MemberProfileExtensions.Where(m => m.MemberProfileId == memberProfile.Id);
                    //var demoGraphicList = _vodusContext.MemberProfileExtensions.Where(m => m.MemberProfileId == memberProfile.Id);
                    var demoGraphicList = await _vodusContext.DemographicValues.ToListAsync();
                    var gender = demoGraphicList.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Gender).Select(m => m.DisplayValue).FirstOrDefault();
                    var edu = demoGraphicList.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Education).Select(m => m.DisplayValue).FirstOrDefault();
                    var eth = demoGraphicList.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Ethnicity).Select(m => m.DisplayValue).FirstOrDefault();
                    var age = demoGraphicList.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.Age).Select(m => m.DisplayValue).FirstOrDefault();
                    var state = demoGraphicList.Where(x => x.IsActive == true && x.DemographicTypeId == (int)DemographicTypeEnum.State).Select(m => m.DisplayValue).FirstOrDefault();
                    UserProfileViewModel r = new UserProfileViewModel()
                    {
                        Address = mastermemberProfile?.AddressLine1,
                        MobilePhone = mastermemberProfile?.MobileNumber,
                        Email = user.Email,
                        Name = user.NormalizedUserName,
                        UserToken = user.SecurityStamp,
                        UserId = user.Id.ToString(),
                        JoinDate = user.CreatedAt,
                        DateOfBirth = mastermemberProfile?.DateOfBirth,

                        Gender = gender,
                        Age = age,
                        Education = edu,
                        Ethinicity = eth,
                        State = state,
                    };


                    response.Successful = true;
                    response.Message = "Get Users Successfully";
                    response.Data = r;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Users not found";
                }
            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
