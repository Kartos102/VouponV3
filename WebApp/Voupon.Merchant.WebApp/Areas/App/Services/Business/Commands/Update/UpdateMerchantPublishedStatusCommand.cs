using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.App.Services.Business.ViewModels;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Business.Commands.Update
{
    public class UpdateMerchantPublishedStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public bool Status { get; set; }
       
    }

    public class UpdateMerchantPublishedStatusCommandHandler : IRequestHandler<UpdateMerchantPublishedStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateMerchantPublishedStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateMerchantPublishedStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            
            var merchant = await rewardsDBContext.Merchants.FirstAsync(x => x.Id == request.MerchantId);
            if(merchant!=null)
            {
                merchant.IsPublished = request.Status;             
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Merchant Published Successfully";
            }
            else
            {
                response.Message = "Merchant not found";
            }         
            return response;
        }
    }
}
