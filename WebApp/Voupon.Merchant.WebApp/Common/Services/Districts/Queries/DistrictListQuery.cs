using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Districts.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Districts.Queries
{
    public class DistrictListQuery : IRequest<ApiResponseViewModel>
    {
        public int ProvinceId { get; set; }
    }
    public class DistrictListQueryHandler : IRequestHandler<DistrictListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DistrictListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DistrictListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Districts.Where(x => x.IsActivated == true && x.ProvinceId == request.ProvinceId).ToListAsync();
                List<DistrictModel> list = new List<DistrictModel>();
                foreach (var item in items)
                {
                    DistrictModel newItem = new DistrictModel();
                    newItem.Id = item.Id;
                    newItem.Name = item.Name;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get District List Successfully";
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
