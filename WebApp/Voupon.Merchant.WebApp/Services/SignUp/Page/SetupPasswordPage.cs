using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;

namespace Voupon.Merchant.WebApp.Services.SignUp.Page
{
    public class SetupPasswordPage : IRequest<SetupPasswordPageViewModel>
    {
        public Guid Id { get; set; }
        private class SetupPasswordPageHandler : IRequestHandler<SetupPasswordPage, SetupPasswordPageViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public SetupPasswordPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<SetupPasswordPageViewModel> Handle(SetupPasswordPage request, CancellationToken cancellationToken)
            {
                return await rewardsDBContext.TempUsers.AsNoTracking().Where(x => x.Id == request.Id).Take(1).Select(x => new SetupPasswordPageViewModel
                {
                    Id = x.Id,
                    BusinessName = x.BusinessName,
                    Email = x.Email,
                    MobileNumber = x.MobileNumber,
                    UserId = x.UserId,
                    TACVerifiedAt = x.TACVerifiedAt
                }).FirstOrDefaultAsync();
            }
        }
    }

    public class SetupPasswordPageViewModel
    {
        [Required]
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string BusinessName { get; set; }
        [Required]
        public string TAC { get; set; }
        public Guid? UserId { get; set; }

        public DateTime? TACVerifiedAt { get; set; }

        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }

    }
}
