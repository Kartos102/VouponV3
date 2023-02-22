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
    public class RoleListQuery : IRequest<ApiResponseViewModel>
    {      
    }

    public class RoleListQueryHandler : IRequestHandler<RoleListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public RoleListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(RoleListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var users = await rewardsDBContext.Roles.ToListAsync();
                if (users != null)
                {                 
                    response.Successful = true;
                    response.Message = "Get Roles Successfully";
                    response.Data = users;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Roles not found";
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
