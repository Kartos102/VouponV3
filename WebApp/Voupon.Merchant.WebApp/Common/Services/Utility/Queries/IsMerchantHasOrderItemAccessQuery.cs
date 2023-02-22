using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Utility.Queries
{
    public class IsMerchantHasOrderItemAccessQuery : IRequest<bool>
    {
        public int MerchantId { get; set; }
        public Guid OrderItemId { get; set; }
        public class IsMerchantHasOrderItemAccessQueryHandler : IRequestHandler<IsMerchantHasOrderItemAccessQuery, bool>
        {
            RewardsDBContext rewardsDBContext;
            public IsMerchantHasOrderItemAccessQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<bool> Handle(IsMerchantHasOrderItemAccessQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {

                    var orderItem = await rewardsDBContext.OrderItems.Where(x => x.Id == request.OrderItemId).FirstOrDefaultAsync();
                    if(orderItem == null)
                    {
                        return false;
                    }

                    if(orderItem.MerchantId == request.MerchantId)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;
                }
                return false;
            }
        }
    }
}
   
