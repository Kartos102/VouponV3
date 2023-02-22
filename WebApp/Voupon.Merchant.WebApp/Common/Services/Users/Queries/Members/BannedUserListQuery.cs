using MediatR;

using Microsoft.EntityFrameworkCore;

using System;
using System.Threading;
using System.Threading.Tasks;

using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Queries.Members
{
    public class BannedUserListQuery : IRequest<ApiResponseViewModel>
    {
    }

    public class BannedUserListQueryHandler : IRequestHandler<BannedUserListQuery, ApiResponseViewModel>
    {
        VodusV2Context _context;
        public BannedUserListQueryHandler(VodusV2Context context)
        {
            _context = context;
        }

        public async Task<ApiResponseViewModel> Handle(BannedUserListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var users = await _context.BannedUsers
                    .ToListAsync();
                if (users != null)
                {
                    response.Successful = true;
                    response.Message = "Get Banned Users Successfully";
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