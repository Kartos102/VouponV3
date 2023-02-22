using MediatR;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Commands.Members
{
    public class UpdateBannedUserCommand : IRequest<ApiResponseViewModel>
    {
        public string BannedId { get; set; }
        public string Reason { get; set; }
    }

    public class UpdateBannedUserCommandHandler : IRequestHandler<UpdateBannedUserCommand, ApiResponseViewModel>
    {
        VodusV2Context _context;
        public UpdateBannedUserCommandHandler(VodusV2Context context)
        {
            _context = context;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateBannedUserCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            try
            {
                var dbModel = _context.BannedUsers.Where(m => m.Id == new Guid(request.BannedId)).FirstOrDefault();
                if (dbModel != null)
                {
                    dbModel.Reason = request.Reason;
                    _context.BannedUsers.Update(dbModel);

                    var affected = await _context.SaveChangesAsync();
                    Console.WriteLine();
                    if (affected > 0)
                    {
                        response.Data = dbModel;
                        response.Successful = true;
                        response.Message = "Banned User Updated Successfully";
                    }
                    else
                    {
                        response.Successful = false;
                        response.Message = "Failed To Updated Banned User";
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