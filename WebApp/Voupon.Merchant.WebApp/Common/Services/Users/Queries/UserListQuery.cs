using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Queries
{
    public class UserListQuery : IRequest<ApiResponseViewModel>
    {      
    }

    public class UserListQueryHandler : IRequestHandler<UserListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UserListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UserListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var users = await rewardsDBContext.Users.Include(x => x.UserClaims).Include(x => x.UserRoles).ThenInclude(x=>x.Role).ToListAsync();
                if (users != null)
                {                 
                    response.Successful = true;
                    response.Message = "Get Users Successfully";
                    response.Data = users;
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
