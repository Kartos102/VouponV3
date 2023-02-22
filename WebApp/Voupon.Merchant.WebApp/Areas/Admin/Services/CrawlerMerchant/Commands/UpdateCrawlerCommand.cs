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
    public class UpdateCrawlerCommand : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }

        public string MerchantName { get; set; }
        public string ShopeeUrl { get; set; }
        public class UpdateCrawlerCommandHandler : IRequestHandler<UpdateCrawlerCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;


            public UpdateCrawlerCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateCrawlerCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                var merchant = await rewardsDBContext.ConsoleMerchantToCrawl.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (merchant != null)
                {

                    merchant.MerchantName = request.MerchantName;
                    merchant.Url = request.ShopeeUrl;

                    rewardsDBContext.ConsoleMerchantToCrawl.Update(merchant);
                    await rewardsDBContext.SaveChangesAsync();


                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully updated crawler";
                }
                else
                {

                    apiResponseViewModel.Message = "Crawler Not Found";
                    apiResponseViewModel.Successful = false;

                }

                return apiResponseViewModel;
            }
        }
    }
}
