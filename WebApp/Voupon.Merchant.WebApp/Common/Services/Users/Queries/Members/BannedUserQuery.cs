using MediatR;

using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Queries.Members
{
    public class BannedUserQuery : IRequest<ApiResponseViewModel>
    {
        public string BannedId { get; set; }
    }

    public class BannedUserQueryHandler : IRequestHandler<BannedUserQuery, ApiResponseViewModel>
    {
        VodusV2Context _context;
        public BannedUserQueryHandler(VodusV2Context context)
        {
            _context = context;
        }

        public async Task<ApiResponseViewModel> Handle(BannedUserQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var user = await _context.BannedUsers.Where(m => m.Id == new Guid(request.BannedId)).FirstOrDefaultAsync();
                if (user != null)
                {
                    response.Successful = true;
                    response.Message = "Get Banned User Successfully";
                    response.Data = user;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Banned User not found";
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