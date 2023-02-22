using MediatR;

using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.Users;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Queries.Members
{
    public class UserProfileListQuery : IRequest<ApiResponseViewModel>
    {
    }

    public class UserProfileListQueryHandler : IRequestHandler<UserProfileListQuery, ApiResponseViewModel>
    {
        VodusV2Context _vodusContext;
        public UserProfileListQueryHandler(VodusV2Context vodusContext)
        {
            _vodusContext = vodusContext;
        }

        public async Task<ApiResponseViewModel> Handle(UserProfileListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                List<UserProfileListViewModel> dataList = new List<UserProfileListViewModel>();
              
                var users = await _vodusContext.MasterMemberProfiles.Include(x=>x.User)?
                    //.AsNoTracking()

                    .Where(m => m.User.UserRoles.FirstOrDefault().Role.Name == "Member")
                    //.Include(m => m.MasterMemberProfiles)
                    //.ThenInclude(x => x.Role)
                    .OrderByDescending(m => m.CreatedAt)
                    .Take(50)
                    .ToListAsync();
                if (users != null)
                {
                    //var memberData = await _vodusContext.MemberProfiles.Select(m => new { email = m.Email, address = m.AddressLine1 }).ToListAsync();

                    foreach (var user in users)
                    {
                        //var address = memberData.Where(m => m.email == user.Email).Select(m => m.address).FirstOrDefault();
                        dataList.Add(new UserProfileListViewModel()
                        {
                            Id = new Guid(user.UserId),
                            Email = user.User.Email,
                            Address = user.AddressLine1,
                            CreatedAt = user.CreatedAt,
                            MobilePhone = user.User.PhoneNumber,
                            Dob = user.DateOfBirth,
                          
                            Name = user.User.NormalizedUserName,
                            VPoints = null
                        });
                    }
                    response.Successful = true;
                    response.Message = "Get Users Successfully";
                    response.Data = dataList;
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
