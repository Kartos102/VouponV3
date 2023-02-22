using MediatR;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;

using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Commands.Members
{
    public class AddBannedUserCommand : IRequest<ApiResponseViewModel>
    {
        public string Email { get; set; }
        public string Reason { get; set; }
    }

    public class AddBannedUserCommandHandler : IRequestHandler<AddBannedUserCommand, ApiResponseViewModel>
    {
        private readonly UserManager<Voupon.Database.Postgres.RewardsEntities.Users> _userManager;
        VodusV2Context _context;
        public AddBannedUserCommandHandler(VodusV2Context context, UserManager<Voupon.Database.Postgres.RewardsEntities.Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ApiResponseViewModel> Handle(AddBannedUserCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            try
            {
                var iUser = await _context.Users.Where(m => m.Email == request.Email).FirstOrDefaultAsync();
                if (iUser == null)
                {
                    response.Successful = false;
                    response.Message = $"User With Email {request.Email} Not Found !";
                    return response;
                }

                var alreadyBanned = await _context.BannedUsers.AnyAsync(m => m.Email == request.Email);
                if (alreadyBanned)
                {
                    response.Successful = false;
                    response.Message = $"User With Email {request.Email} Already Banned.";
                    return response;
                }

                var newBannedUser = new BannedUsers
                {
                    Email = request.Email,
                    Reason = request.Reason
                };

                var masterMemberData = _context.MasterMemberProfiles.Where(m => m.UserId == iUser.Id).FirstOrDefault();
                var userData = _context.Users.Where(m => m.Email == request.Email).FirstOrDefault();


                //Will be calculated upon data retrival
                newBannedUser.VPointsGained = 0;
                
                newBannedUser.Name = userData?.FirstName + " " + userData?.LastName;
                newBannedUser.Address = masterMemberData?.AddressLine1 + " , " + masterMemberData?.AddressLine2 + " , " + masterMemberData.City + " , " + masterMemberData.State;
                newBannedUser.UserId = iUser.Id.ToString();

                _context.BannedUsers.Add(newBannedUser);

                var affected = await _context.SaveChangesAsync();
                if (affected > 0)
                {
                    response.Data = newBannedUser;
                    response.Successful = true;
                    response.Message = "Banned User Successfully";
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Failed To Ban User";
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
