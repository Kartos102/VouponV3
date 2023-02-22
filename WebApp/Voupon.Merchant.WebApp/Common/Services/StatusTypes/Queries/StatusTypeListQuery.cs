using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.StatusTypes.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.StatusTypes.Queries
{   
    public class StatusTypeListQuery : IRequest<ApiResponseViewModel>
    {
    }
    public class StatusTypeListQueryHandler : IRequestHandler<StatusTypeListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public StatusTypeListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(StatusTypeListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var statusTypes = await rewardsDBContext.StatusTypes.Where(x => x.IsActivated == true).ToListAsync();
                List<StatusTypeModel> statusTypeList = new List<StatusTypeModel>();
                foreach (var status in statusTypes)
                {
                    StatusTypeModel statusType = new StatusTypeModel();
                    statusType.Id = status.Id;
                    statusType.Name = status.Name;
                    statusType.Description = status.Description;
                    statusTypeList.Add(statusType);
                }
                response.Successful = true;
                response.Message = "Get Status Tyle List Successfully";
                response.Data = statusTypeList;
            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
