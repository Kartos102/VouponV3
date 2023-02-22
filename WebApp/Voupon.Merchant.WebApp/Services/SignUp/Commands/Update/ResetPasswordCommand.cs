using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common;
using Voupon.Database.Postgres.RewardsEntities;
//using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.SignUp.Commands.Update
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
            private readonly RewardsDBContext rewardsDBContext;
            private readonly UserManager<Users> userManager;
            private readonly IOptions<AppSettings> appSettings;
            private readonly SignInManager<Users> signInManager;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public ResetPasswordCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings, UserManager<Users> userManager, SignInManager<Users> signInManager)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
            }

            public async Task<ApiResponseViewModel> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {

                    var user = await userManager.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync();

                    if (user == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request";
                        return apiResponseViewModel;
                    }
                    var passwordReset = await rewardsDBContext.PasswordResets.Where(x => x.Email == request.Email).OrderByDescending(x => x.CreatedAt).FirstOrDefaultAsync();
                    var resetPasswordRequestedTime = passwordReset.ExpireAt.Subtract(DateTime.Now).TotalMinutes;
                    if (resetPasswordRequestedTime > 120)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Reset password Code expired";
                        return apiResponseViewModel;
                    }
                    if (passwordReset == null || passwordReset.ResetCode != request.Code || passwordReset.IsReset)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid Code";
                        return apiResponseViewModel;
                    }
                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

                    if (!result.Succeeded)
                    {
                        var errorMessagesList = result.Errors.Select(x => x.Description).ToList();
                        var errorMessages = "";
                        foreach (var message in errorMessagesList)
                        {
                            errorMessages = errorMessages + " " + message;
                        }
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = errorMessages;
                        return apiResponseViewModel;
                    }
                    passwordReset.IsReset = true;
                    rewardsDBContext.SaveChanges();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully updated Password";
                    apiResponseViewModel.Successful = true;

                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Incorrect email or password";
                    return apiResponseViewModel;
                }
            }

        }
    }

}
