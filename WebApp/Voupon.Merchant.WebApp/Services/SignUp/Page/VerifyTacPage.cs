using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;

namespace Voupon.Merchant.WebApp.Services.SignUp.Page
{ 
    public class VerifyTACPage : IRequest<VerifyTACPageViewModel>
    {
        public Guid Id { get; set; }
        private class VerifyTACPageHandler : IRequestHandler<VerifyTACPage, VerifyTACPageViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public VerifyTACPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<VerifyTACPageViewModel> Handle(VerifyTACPage request, CancellationToken cancellationToken)
            {
                return await rewardsDBContext.TempUsers.AsNoTracking().Where(x => x.Id == request.Id).Take(1).Select(x => new VerifyTACPageViewModel
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

    public class VerifyTACPageViewModel
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
    }
}
