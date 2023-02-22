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

namespace Voupon.Merchant.WebApp.Services.Identity.Queries
{
    public class UserMerchantIdQuery : IRequest<ApiResponseViewModel>
    {
        public string Email { get; set; }
        private class UserMerchantIdQueryHandler : IRequestHandler<UserMerchantIdQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;

            private readonly UserManager<Users> userManager;
            private readonly IOptions<AppSettings> appSettings;
            private readonly SignInManager<Users> signInManager;
            private readonly IHttpContextAccessor httpContext;
            private IPrincipal principal;


            public UserMerchantIdQueryHandler(IPrincipal currentPrincipal, RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings, UserManager<Users> userManager, SignInManager<Users> signInManager, IHttpContextAccessor httpContext)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
                this.httpContext = httpContext;
                this.principal = currentPrincipal;
            }

            public async Task<ApiResponseViewModel> Handle(UserMerchantIdQuery request, CancellationToken cancellationToken)
            {
                int merchantId = 0;
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
                        var userClaim = user.UserClaims.FirstOrDefault(x => x.ClaimType == "MerchantId");
                        if (userClaim == null)
                        {

                        }
                        else
                        {
                            merchantId = Int32.Parse(userClaim.ClaimValue);
                        }
                        apiResponseViewModel.Data = merchantId;
                        apiResponseViewModel.Successful = true;
                        return apiResponseViewModel;
                    }
                    else
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid email";
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
