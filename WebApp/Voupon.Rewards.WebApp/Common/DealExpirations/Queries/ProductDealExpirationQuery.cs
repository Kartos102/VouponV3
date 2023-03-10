using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;

using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Services.DealExpirations.Queries
{
    public class ProductDealExpirationQuery : IRequest<ApiResponseViewModel>
    {
        public int DealExpirationId { get; set; }
    }
    public class ProductDealExpirationQueryHandler : IRequestHandler<ProductDealExpirationQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductDealExpirationQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductDealExpirationQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.DealExpirations.Include(x => x.Product).Include(x => x.ExpirationType).FirstOrDefaultAsync(x => x.Id == request.DealExpirationId);

                //DealExpirationModel newItem = new DealExpirationModel();
                //newItem.Id = item.Id;
                //newItem.ExpirationType = item.ExpirationType.Name;
                //newItem.ExpirationTypeId = item.ExpirationTypeId;
                //newItem.ProductId = item.ProductId;
                //newItem.CreatedAt = item.CreatedAt;
                //newItem.CreatedByUserId = new Guid(item.CreatedByUserId);
                //newItem.ExpiredDate = item.ExpiredDate;
                //newItem.StartDate = item.StartDate;
                //newItem.TotalValidDays = item.TotalValidDays;
                //newItem.LastUpdatedAt = item.LastUpdatedAt;
                //newItem.LastUpdatedByUserId = item.LastUpdatedByUserId;
                response.Successful = true;
                response.Message = "Get Product Deal Expiration Successfully";
                response.Data = item;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
