using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.SignUp.Queries.Single
{

    public class TempUserByIdQuery : IRequest<TempUserByIdQueryResponseViewModel>
    {
        public Guid Id { get; set; }
    }

    public class TempUserByIdQueryResponseViewModel
    {
        [Required]
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string BusinessName { get; set; }
        [Required]
        public string TAC { get; set; }
        public Guid? UserId {get;set;}

        public DateTime? TACVerifiedAt { get; set; }
    }
    public class TempUserByIdQueryHandler : IRequestHandler<TempUserByIdQuery, TempUserByIdQueryResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public TempUserByIdQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<TempUserByIdQueryResponseViewModel> Handle(TempUserByIdQuery request, CancellationToken cancellationToken)
        {
            return await rewardsDBContext.TempUsers.AsNoTracking().Where(x => x.Id == request.Id).Take(1).Select(x => new TempUserByIdQueryResponseViewModel
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
