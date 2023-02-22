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
    public class UserQuery : IRequest<ApiResponseViewModel>
    {      
        public string UserId { get; set; }
    }

    public class UserQueryHandler : IRequestHandler<UserQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UserQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UserQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var guid = new Guid(request.UserId);
                var user = await rewardsDBContext.Users.Include(x => x.UserClaims).Include(x => x.UserRoles).ThenInclude(x=>x.Role).FirstOrDefaultAsync(x=>x.Id== guid);
                if (user != null)
                {                 
                    response.Successful = true;
                    response.Message = "Get User Successfully";
                    response.Data = user;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "User not found";
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
