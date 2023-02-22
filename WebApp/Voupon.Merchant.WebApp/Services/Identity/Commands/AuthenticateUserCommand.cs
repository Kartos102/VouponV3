using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Microsoft.EntityFrameworkCore;
using Voupon.Merchant.WebApp.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;

namespace Voupon.Merchant.WebApp.Services.Identity.Commands
{
    public class AuthenticateUserCommand : IRequest<ApiResponseViewModel>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        private class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;

            private readonly UserManager<Users> userManager;
            private readonly IOptions<AppSettings> appSettings;
            private readonly SignInManager<Users> signInManager;
            private readonly IHttpContextAccessor httpContext;
            private IPrincipal principal;


            public AuthenticateUserCommandHandler(IPrincipal currentPrincipal,RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings, UserManager<Users> userManager, SignInManager<Users> signInManager, IHttpContextAccessor httpContext)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
                this.httpContext = httpContext;
                this.principal = currentPrincipal;
            }

            public async Task<ApiResponseViewModel> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    if (string.IsNullOrEmpty(request.Email))
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid username/password";
                    }
                    var securityToken = string.Empty;

                    var user = await userManager.FindByEmailAsync(request.Email);
                    if (user != null)
                    {
                        // ClaimsPrincipal cp = httpContext.HttpContext.User;
                        // var identity = cp.Identity as ClaimsIdentity;
                        // identity.AddClaim(new Claim("MerchantId", "1"));
                        
                        var result = await signInManager.PasswordSignInAsync(user.NormalizedUserName,
                              request.Password, true, lockoutOnFailure: false);
                        
                        if (result.Succeeded)
                        {
                            apiResponseViewModel.Data = await userManager.GetRolesAsync(user);
                            apiResponseViewModel.Successful = true;
                        }
                        else
                        {
                            apiResponseViewModel.Successful = false;

                            apiResponseViewModel.Message = "Invalid email/password";
                        }
                        return apiResponseViewModel;
                    }
                    else
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid username/password";
                    }
                }
                catch (Exception ex)
                 {
                    throw new Exception(ex.ToString());
                }
                return apiResponseViewModel;
            }

        }
    }

}
