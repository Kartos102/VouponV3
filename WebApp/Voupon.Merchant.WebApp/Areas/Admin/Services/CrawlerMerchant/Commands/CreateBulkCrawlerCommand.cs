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
    public class CreateBulkCrawlerCommand : IRequest<ApiResponseViewModel>
    {
        public List<string> ShopeeUrls { get; set; }
    }

    public class CreateBulkCrawlerCommandHandler : IRequestHandler<CreateBulkCrawlerCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CreateBulkCrawlerCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CreateBulkCrawlerCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            var allMerchant = rewardsDBContext.ConsoleMerchantToCrawl.ToList();

            for (var i = 0; i < request.ShopeeUrls.Count(); i++)
            {
                request.ShopeeUrls[i] = request.ShopeeUrls[i].ToLower();
            }


            foreach (var merchant in allMerchant)
            {
                if (request.ShopeeUrls.Contains(merchant.Url))
                {
                    request.ShopeeUrls.Remove(merchant.Url);
                }
            }


            foreach (string s in request.ShopeeUrls) {

                var crwler = new ConsoleMerchantToCrawl();
                crwler.Id = Guid.NewGuid();
                crwler.StatusId = 1;
                crwler.Url = s;
                crwler.CreatedAt = DateTime.Now;
                crwler.LastUpdatedAt = DateTime.Now;
                crwler.ExternalTypeId = 1;
                crwler.CurrentProcess = 0;
                await rewardsDBContext.ConsoleMerchantToCrawl.AddAsync(crwler);

            }
            try {

                await rewardsDBContext.SaveChangesAsync();
                response.Successful = true;
                response.Message = "Successfully add crawler";
            } catch (Exception ex) {
                response.Successful = false;
                response.Message = ex.Message;
            }
            

            return response;
        }
    }
}
