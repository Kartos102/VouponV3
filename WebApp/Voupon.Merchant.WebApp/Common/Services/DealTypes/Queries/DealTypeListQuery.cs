using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.DealTypes.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.DealTypes.Queries
{   
    public class DealTypeListQuery : IRequest<ApiResponseViewModel>
    {
    }
    public class DealTypeListQueryHandler : IRequestHandler<DealTypeListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DealTypeListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DealTypeListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.DealTypes.ToListAsync();
                List<DealTypeModel> list = new List<DealTypeModel>();
                foreach (var item in items)
                {
                    DealTypeModel newItem = new DealTypeModel();
                    newItem.Id = item.Id;
                    newItem.Name = item.Name;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.CreatedByUserId = item.CreatedByUserId;
                    newItem.IsActivated = item.IsActivated;
                    newItem.LastUpdatedAt = item.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Deal Type List Successfully";
                response.Data = list;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
