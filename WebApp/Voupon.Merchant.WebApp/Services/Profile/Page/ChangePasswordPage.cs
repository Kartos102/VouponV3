using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;

namespace Voupon.Merchant.WebApp.Services.Profile.Page
{
    public class ResetPasswordPage : IRequest<ChangePasswordViewModel>
    {
        public string Email { get; set; }
        private class ChangePasswordPageHandler : IRequestHandler<ResetPasswordPage, ChangePasswordViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ChangePasswordPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ChangePasswordViewModel> Handle(ResetPasswordPage request, CancellationToken cancellationToken)
            {
                var viewModel = new ChangePasswordViewModel();
                var user = await rewardsDBContext.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync();

                if (user != null)
                {
                    viewModel.Email = user.Email;
                }

                return viewModel;
            }
        }
    }

    public class ChangePasswordViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }
        public string UserId { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

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
}
