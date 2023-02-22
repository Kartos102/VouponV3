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
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.Identity.Commands.Update
{

    public class ResetPasswordForUserViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Email")]
        public string AdminEmail { get; set; }
        public string AdminPassword { get; set; }

        public string Email { get; set; }


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

    public class ResetPasswordForUserCommand : IRequest<ApiResponseViewModel>
    {
        public string AdminUserId{ get; set; }
        public string AdminPassword { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public class ResetPasswordForUserCommandHandler : IRequestHandler<ResetPasswordForUserCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            private readonly UserManager<Voupon.Database.Postgres.RewardsEntities.Users> userManager;
            private readonly IOptions<AppSettings> appSettings;
            private readonly SignInManager<Voupon.Database.Postgres.RewardsEntities.Users> signInManager;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public ResetPasswordForUserCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings, UserManager<Voupon.Database.Postgres.RewardsEntities.Users> userManager, SignInManager<Voupon.Database.Postgres.RewardsEntities.Users> signInManager)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
            }

            public async Task<ApiResponseViewModel> Handle(ResetPasswordForUserCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {
                   
                    var user = userManager.Users.FirstOrDefault(x => x.Id.ToString() == request.AdminUserId.ToString());


                    if (request.AdminUserId != user.Id.ToString())
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Incorrect Email";
                        return apiResponseViewModel;
                    }

                    var identity = await signInManager.PasswordSignInAsync(user, request.AdminPassword, false, false);
                    if (!identity.Succeeded)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Incorrect Admin password";
                        return apiResponseViewModel;
                    }

                    if (request.NewPassword != request.ConfirmPassword)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "New Password does not match Confirm Password Email";
                        return apiResponseViewModel;
                    }

                    if (request.NewPassword == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "New Password can not be empty";
                        return apiResponseViewModel;
                    }


                    user = userManager.Users.FirstOrDefault(x => x.Email == request.Email.ToString());


                    if (request.Email != user.Email)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Incorrect Email";
                        return apiResponseViewModel;
                    }


                    var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                    var result = await userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

                    //var result = await userManager.ChangePasswordAsync(user, user.PasswordHash.ge, request.NewPassword);
                    if (!result.Succeeded)
                    {
                        var errorMessagesList = result.Errors.Select(x => x.Description).ToList();
                        var errorMessages = "";
                        foreach(var message in errorMessagesList)
                        {
                            errorMessages = errorMessages + " " + message;
                        }
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = errorMessages;
                        return apiResponseViewModel;
                    }
                    //passwordReset.IsReset = true;
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
