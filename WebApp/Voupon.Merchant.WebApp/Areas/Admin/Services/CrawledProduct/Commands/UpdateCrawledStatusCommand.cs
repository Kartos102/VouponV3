using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.CrawledProduct.Commands
{
    public class UpdateCrawledStatusCommand : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }

        public byte Status { get; set; }

        public class UpdateCrawledStatusCommandHandler : IRequestHandler<UpdateCrawledStatusCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;


            public UpdateCrawledStatusCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateCrawledStatusCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                var product = await rewardsDBContext.ConsoleProductJSON.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (product != null)
                {

                    product.StatusId = request.Status;
                    rewardsDBContext.ConsoleProductJSON.Update(product);
                    await rewardsDBContext.SaveChangesAsync();


                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully updated";
                }
                else
                {

                    apiResponseViewModel.Message = "Product Not Found";
                    apiResponseViewModel.Successful = false;

                }

                return apiResponseViewModel;

            }
        }
    }
}
