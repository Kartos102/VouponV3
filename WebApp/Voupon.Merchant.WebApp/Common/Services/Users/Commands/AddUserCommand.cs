using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Users.Commands
{
    public class AddUserCommand : IRequest<ApiResponseViewModel>
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class AddUserCommandHandler : IRequestHandler<AddUserCommand, ApiResponseViewModel>
    {
        private readonly UserManager<Voupon.Database.Postgres.RewardsEntities.Users> userManager;
        RewardsDBContext rewardsDBContext;
        public AddUserCommandHandler(RewardsDBContext rewardsDBContext, UserManager<Voupon.Database.Postgres.RewardsEntities.Users> userManager)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.userManager = userManager;
        }

        public async Task<ApiResponseViewModel> Handle(AddUserCommand request, CancellationToken cancellationToken)
        {

            ApiResponseViewModel response = new ApiResponseViewModel();
         
            if (await userManager.FindByEmailAsync(request.Email) != null)
            {
                response.Successful = false;
                response.Message = "User Existing";
                return response;
            }
            var newUser = new Voupon.Database.Postgres.RewardsEntities.Users()
            {
                Id = Guid.NewGuid(),
                UserName = request.Email,
                Email = request.Email,
                NormalizedEmail = request.Email.Normalize(),
                NormalizedUserName = request.Email.Normalize(),
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                CreatedAt = DateTime.Now
            };
            try
            {
                var cre = await userManager.CreateAsync(newUser, request.Password);
                if (cre.Succeeded)
                {
                    response.Data = newUser;
                    response.Successful = true;
                    response.Message = "Create User Successfully";
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Failed To Create User";
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
