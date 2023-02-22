using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;


namespace Voupon.Merchant.WebApp.Areas.Admin.Services.CrawlerMerchant.Commands
{
    public class UpdateCrawlerStatusCommand : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }

        public byte Status { get; set; }

        public class UpdateCrawlerStatusCommandHandler : IRequestHandler<UpdateCrawlerStatusCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;


            public UpdateCrawlerStatusCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateCrawlerStatusCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                var merchant = await rewardsDBContext.ConsoleMerchantToCrawl.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (merchant != null)
                {

                    merchant.StatusId = request.Status;
                    rewardsDBContext.ConsoleMerchantToCrawl.Update(merchant);
                    await rewardsDBContext.SaveChangesAsync();


                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully updated";
                }
                else {

                    apiResponseViewModel.Message = "Merchant To Crawl Not Found";
                    apiResponseViewModel.Successful = false;

                }

                return apiResponseViewModel;

            }
        }
    }
}
