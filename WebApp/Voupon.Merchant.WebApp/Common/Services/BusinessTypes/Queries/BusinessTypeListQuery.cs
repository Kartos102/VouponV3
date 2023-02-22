using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.BusinessTypes.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.BusinessTypes.Queries
{    
    public class BusinessTypeListQuery : IRequest<ApiResponseViewModel>
    {
    }
    public class BusinessTypeListQueryHandler : IRequestHandler<BusinessTypeListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public BusinessTypeListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(BusinessTypeListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var businessTypes = await rewardsDBContext.BusinessTypes.Where(x => x.IsActivated == true).ToListAsync();
                List<BusinessTypeModel> list = new List<BusinessTypeModel>();
                foreach (var type in businessTypes)
                {
                    BusinessTypeModel businessType = new BusinessTypeModel();
                    businessType.Id = type.Id;
                    businessType.Name = type.Name;
                    list.Add(businessType);
                }
                response.Successful = true;
                response.Message = "Get BusinessType List Successfully";
                response.Data = list;
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
