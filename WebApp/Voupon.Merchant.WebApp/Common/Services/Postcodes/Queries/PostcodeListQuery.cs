using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Postcodes.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Postcodes.Queries
{  
    public class PostcodeListQuery : IRequest<ApiResponseViewModel>
    {
        public int DistrictId { get; set; }
    }
    public class PostCodeListQueryHandler : IRequestHandler<PostcodeListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public PostCodeListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(PostcodeListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.PostCodes.Where(x => x.IsActivated == true && x.DistrictId == request.DistrictId).ToListAsync();
                List<PostcodeModel> list = new List<PostcodeModel>();
                foreach (var item in items)
                {
                    PostcodeModel newItem = new PostcodeModel();
                    newItem.Id = item.Id;
                    newItem.Name = item.Name;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Data = list;
                response.Message = "Get Postcode List Successfully";
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
