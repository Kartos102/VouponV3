using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;


namespace Voupon.Merchant.WebApp.Areas.Admin.Services.CrawlerMerchant.Commands
{
    public class DeleteCrawlerCommand : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCrawlerCommandHandler : IRequestHandler<DeleteCrawlerCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DeleteCrawlerCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DeleteCrawlerCommand request, CancellationToken cancellationToken)
        {

            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                var crawler = await rewardsDBContext.ConsoleMerchantToCrawl.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (crawler == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Crawler Merchant Not Found";
                    return apiResponseViewModel;
                }
               
                rewardsDBContext.ConsoleMerchantToCrawl.Remove(crawler);
                await rewardsDBContext.SaveChangesAsync();

                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully Deleted Merchant";
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = ex.Message;
                return apiResponseViewModel;
            }
        }
    }
}
