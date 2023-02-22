using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.LuckyDraws.Queries
{
    public class UpdateLuckyDrawDateCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public DateTime LuckyDrawDate { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }
    public class UpdateLuckyDrawDateCommandHandler : IRequestHandler<UpdateLuckyDrawDateCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateLuckyDrawDateCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateLuckyDrawDateCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.LuckyDraws.FirstOrDefaultAsync(x => x.ProductId == request.ProductId);         
                if(item!=null)
                {
                    item.LuckyDrawDate = request.LuckyDrawDate;
                    item.LastUpdatedAt = request.LastUpdatedAt;
                    item.LastUpdatedByUserId = request.LastUpdatedByUserId;
                    await rewardsDBContext.SaveChangesAsync();
                    response.Successful = true;
                    response.Message = "Update Product Lucky Draw Successfully";
                    response.Data = item;
                }
                else
                {
                    var newItem = new Voupon.Database.Postgres.RewardsEntities.LuckyDraws();
                    newItem.ProductId = request.ProductId;
                    newItem.CreatedAt = request.LastUpdatedAt;
                    newItem.CreatedByUserId = request.LastUpdatedByUserId;
                    newItem.LuckyDrawDate = request.LuckyDrawDate;
                    newItem.WinningTicketId = null;
                    newItem.LuckyDrawTicketIssued = 0;
                    newItem.StatusTypeId = 1;
                    await rewardsDBContext.LuckyDraws.AddAsync(newItem);
                    await rewardsDBContext.SaveChangesAsync();

                    response.Successful = true;
                    response.Message = "Create Product Lucky Draw Successfully";
                    response.Data = null;
                }               
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
