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
    public class UpdateKeywordCommand : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; }
        public string SortBy { get; set; }
        public int TotalListing { get; set; }


        public class UpdateKeywordCommandHandler : IRequestHandler<UpdateKeywordCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;


            public UpdateKeywordCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateKeywordCommand request, CancellationToken cancellationToken)
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

                    var existingKeyword = await rewardsDBContext.GoogleMerchantKeywords.Where(x => x.Keyword == request.Keyword).FirstOrDefaultAsync();
                    if (existingKeyword != null)
                    {
                        if (existingKeyword.Id != request.Id)
                        {
                            apiResponseViewModel.Message = "Keyword already exists";
                            apiResponseViewModel.Successful = false;
                            return apiResponseViewModel;
                        }
                    }

                    keyword.SortBy = request.SortBy;
                    keyword.TotalListing = request.TotalListing;
                    keyword.Keyword = request.Keyword;

                    rewardsDBContext.GoogleMerchantKeywords.Update(keyword);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully updated keywords";
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to update keyword";
                }

                return apiResponseViewModel;
            }
        }

    }
}
