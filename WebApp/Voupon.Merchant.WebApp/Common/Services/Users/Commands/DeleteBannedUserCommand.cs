using MediatR;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Commands.Members
{
    public class UnBannedUserCommand : IRequest<ApiResponseViewModel>
    {
        public string BannedId { get; set; }
    }

    public class UnBannedUserCommandHandler : IRequestHandler<UnBannedUserCommand, ApiResponseViewModel>
    {
        VodusV2Context _context;
        public UnBannedUserCommandHandler(VodusV2Context context)
        {
            _context = context;
        }

        public async Task<ApiResponseViewModel> Handle(UnBannedUserCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            try
            {
                var dbModel = _context.BannedUsers.Where(m => m.Id == new Guid(request.BannedId)).FirstOrDefault();
                if (dbModel != null)
                {
                    _context.BannedUsers.Remove(dbModel);

                    var affected = await _context.SaveChangesAsync();
                    if (affected > 0)
                    {
                        response.Data = dbModel;
                        response.Successful = true;
                        response.Message = "Banned User UnBanned Successfully";
                    }
                    else
                    {
                        response.Successful = false;
                        response.Message = "Failed To UnBanned User";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}