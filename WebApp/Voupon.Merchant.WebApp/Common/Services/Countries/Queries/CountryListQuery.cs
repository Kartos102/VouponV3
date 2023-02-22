using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Countries.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Countries.Queries
{
    public class CountryListQuery : IRequest<ApiResponseViewModel>
    {
    }   

    public class CountryListQueryHandler : IRequestHandler<CountryListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CountryListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CountryListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Countries.Where(x => x.IsActivated == true).ToListAsync();
                List<CountryModel> list = new List<CountryModel>();
                foreach (var item in items)
                {
                    CountryModel newItem = new CountryModel();
                    newItem.Id = item.Id;
                    newItem.Name = item.Name;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Country List Successfully";
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
