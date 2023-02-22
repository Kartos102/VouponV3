using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Identity.Commands
{

    public class ResetPasswordViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        public string Code { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

    }

    public class ResetPasswordCommand : IRequest<ApiResponseViewModel>
    {
        public string Code { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;
            private readonly UserManager<Voupon.Database.Postgres.VodusEntities.Users> userManager;
            private readonly IOptions<AppSettings> appSettings;
            private readonly SignInManager<Voupon.Database.Postgres.VodusEntities.Users> signInManager;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public ResetPasswordCommandHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings, UserManager<Voupon.Database.Postgres.VodusEntities.Users> userManager, SignInManager<Voupon.Database.Postgres.VodusEntities.Users> signInManager)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {
                    var user = userManager.Users.FirstOrDefault(x => x.Email == request.Email.ToString());

                    if (request.Email.Trim().ToLower() != user.Email.Trim().ToLower())
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Incorrect Email";
                        return apiResponseViewModel;
                    }

                    //banned user list Backlog - 1756
                    var bannedUser = await this.vodusV2Context.BannedUsers.AnyAsync(m => m.Email == request.Email);
                    if (bannedUser)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Login failed. This account is suspended";
                        return apiResponseViewModel;
                    }

                    var passwordReset = await vodusV2Context.PasswordResets.Where(x => x.Email == request.Email).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();

                    if (passwordReset == null || passwordReset.ResetCode != request.Code || passwordReset.IsReset)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "The link is no longer valid. Please request a new reset link and try again";
                        return apiResponseViewModel;
                    }
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

                    if (!result.Succeeded)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = string.Join(" , ", result.Errors.Select(x => x.Description));
                        return apiResponseViewModel;
                    }
                    passwordReset.IsReset = true;
                    vodusV2Context.SaveChanges();
                    apiResponseViewModel.Message = "Successfully updated Password";
                    apiResponseViewModel.Successful = true;

                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    await new Logs
                    {
                        Description = ex.ToString(),
                        Email = request.Email,
                        JsonData = JsonConvert.SerializeObject(request),
                        ActionName = "ResetPasswordCommand",
                        TypeId = CreateErrorLogCommand.Type.Service,
                        SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                        RewardsDBContext = rewardsDBContext
                    }.Error();

                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Incorrect email or password";
                    return apiResponseViewModel;
                }
            }

        }
    }

}
