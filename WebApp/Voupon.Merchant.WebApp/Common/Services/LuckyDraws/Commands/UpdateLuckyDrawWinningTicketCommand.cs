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
    public class UpdateLuckyDrawWinningTicketCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public int? WinningTicket { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }
    public class UpdateLuckyDrawWinningTicketCommandHandler : IRequestHandler<UpdateLuckyDrawWinningTicketCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateLuckyDrawWinningTicketCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateLuckyDrawWinningTicketCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.LuckyDraws.FirstOrDefaultAsync(x => x.ProductId == request.ProductId);         
                if(item!=null)
                {
                    item.WinningTicketId = request.WinningTicket;
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
                    newItem.LuckyDrawDate = null;
                    newItem.WinningTicketId = request.WinningTicket;
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
