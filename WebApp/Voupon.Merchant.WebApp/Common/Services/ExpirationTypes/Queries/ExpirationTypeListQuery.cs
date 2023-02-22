using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Merchant.WebApp.Common.Services.ExpirationTypes.Models;

namespace Voupon.Merchant.WebApp.Common.Services.ExpirationTypes.Queries
{   
    public class ExpirationTypeListQuery : IRequest<ApiResponseViewModel>
    {
    }
    public class DExpirationTypeListQueryHandler : IRequestHandler<ExpirationTypeListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DExpirationTypeListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ExpirationTypeListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.ExpirationTypes.Where(x=>x.IsActivated).ToListAsync();
                List<ExpirationTypeModel> list = new List<ExpirationTypeModel>();
                foreach (var item in items)
                {
                    ExpirationTypeModel newItem = new ExpirationTypeModel();
                    newItem.Id = item.Id;
                    newItem.Name = item.Name;
                    newItem.Description = item.Description;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.CreatedByUserId = item.CreatedByUserId;
                    newItem.IsActivated = item.IsActivated;
                    newItem.LastUpdatedAt = item.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Expiration Type List Successfully";
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
