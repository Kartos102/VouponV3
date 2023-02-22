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

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.GoogleMerchant.Commands
{
    public class DeleteKeywordCommand : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }

        public class DeleteKeywordCommandHandler : IRequestHandler<DeleteKeywordCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;


            public DeleteKeywordCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(DeleteKeywordCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var keyword = await rewardsDBContext.GoogleMerchantKeywords.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                    if (keyword == null)
                    {
                        apiResponseViewModel.Message = "Invalid keywords";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    rewardsDBContext.GoogleMerchantKeywords.Remove(keyword);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully deleted keywords";
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to delete keywords";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
