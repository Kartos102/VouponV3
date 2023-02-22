using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Commands
{
    public class UpdateUserCommand : IRequest<ApiResponseViewModel>
    {
        public Guid UserId { get; set; }
        public int? MerchantId { get; set; }
        public Guid RoleId { get; set; }
    }

    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;

        public UpdateUserCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var guid = request.UserId;
                var user = await rewardsDBContext.Users.Include(x => x.UserClaims).Include(x => x.UserRoles)
                    .ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Id == guid);
                if (user != null)
                {
                    if (request.RoleId != null)
                    {
                        if (request.RoleId.ToString().ToUpper() == "C7EAC662-99C8-4612-8273-90913087198D")
                        {
                            request.MerchantId = null;
                        }

                        if (user.UserRoles.Count > 0)
                        {
                            user.UserRoles.Clear();
                            var role_guid = request.RoleId;
                            UserRoles newRole = new UserRoles();
                            var role = await rewardsDBContext.Roles.FirstAsync(x => x.Id == request.RoleId);
                            newRole.MerchantId = request.MerchantId;
                            newRole.Role = role;
                            newRole.RoleId = role.Id;
                            newRole.UserId = guid;
                            user.UserRoles.Add(newRole);

                            //var userRole = user.UserRoles.First();
                            //Guid role_guid = new Guid(request.RoleId);
                            //var role = await rewardsDBContext.Roles.FirstAsync(x => x.Id == role_guid);
                            //userRole.Role = role;
                            //userRole.RoleId = roleGuid;
                            //userRole.MerchantId = request.MerchantId;
                        }
                        else
                        {
                            UserRoles newRole = new UserRoles()
                                { MerchantId = request.MerchantId, RoleId = request.RoleId, UserId = guid };
                            user.UserRoles.Add(newRole);
                        }

                        if (user.UserClaims.FirstOrDefault(x => x.ClaimType == "MerchantId") != null)
                        {
                            var userClaims = user.UserClaims.First(x => x.ClaimType == "MerchantId");
                            userClaims.ClaimValue = request.MerchantId.ToString();
                        }
                        else
                        {
                            UserClaims newClaims = new UserClaims()
                                { UserId = guid, ClaimType = "MerchantId", ClaimValue = request.MerchantId.ToString() };
                            user.UserClaims.Add(newClaims);
                        }
                    }

                    rewardsDBContext.SaveChanges();
                    response.Successful = true;
                    response.Message = "Update User Successfully";
                }
                else
                {
                    response.Message = "User not found";
                }

                return response;
            }
            catch (Exception ex)
            {
                return response;
            }
        }
    }
}