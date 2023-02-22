using MediatR;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;

//using Voupon.Merchant.WebApp.Areas.App.Services.Merchants.Models;

namespace Voupon.Merchant.WebApp.Areas.App.Services.IdentityExtension
{
    public class GetMerchantId : ListQueryRequest<int>
    {
        public string UserId { get; set; }
    }
    public class GetMerchantIdHandler : IRequestHandler<GetMerchantId, int>
    {
        RewardsDBContext rewardsDBContext;
        public GetMerchantIdHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public Task<int> Handle(GetMerchantId request, CancellationToken cancellationToken )
        {
            var merchant = rewardsDBContext.Merchants.Where(x => x.CreatedByUserId.ToString() == request.UserId).FirstOrDefault();
                
            return Task.FromResult(merchant.Id);
        }
    }
}
