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
    public class UserListWithMerchantIdQuery : IRequest<ApiResponseViewModel>
    {      
        public int MerchantId { get; set; }
    }

    public class UserListWithMerchantIdQueryHandler : IRequestHandler<UserListWithMerchantIdQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UserListWithMerchantIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UserListWithMerchantIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var users = await rewardsDBContext.Users.Include(x => x.UserClaims).Include(x => x.UserRoles).ThenInclude(x=>x.Role).Where(x=>x.UserClaims.FirstOrDefault(z => z.ClaimType == "MerchantId") != null && x.UserClaims.FirstOrDefault(z => z.ClaimType == "MerchantId").ClaimValue==request.MerchantId.ToString()).ToListAsync();
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
