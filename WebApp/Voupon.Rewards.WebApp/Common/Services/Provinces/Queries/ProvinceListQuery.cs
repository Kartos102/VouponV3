using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Services.Provinces.Models;
using Voupon.Rewards.WebApp.ViewModels;


namespace Voupon.Rewards.WebApp.Common.Services.Provinces.Queries
{ 
    public class ProvinceListQuery : IRequest<ApiResponseViewModel>
    {
        public int CountryId { get; set; }
    }
    public class ProvinceListQueryHandler : IRequestHandler<ProvinceListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProvinceListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProvinceListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Provinces.Where(x => x.IsActivated == true && x.CountryId == request.CountryId).ToListAsync();
                List<ProvinceModel> list = new List<ProvinceModel>();
                foreach (var item in items)
                {
                    ProvinceModel newItem = new ProvinceModel();
                    newItem.Id = item.Id;
                    newItem.Name = item.Name;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Data = list;
                response.Message = "Get Province List Successfully";
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
