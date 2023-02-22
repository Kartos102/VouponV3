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
    public class CreateKeywordCommand : IRequest<ApiResponseViewModel>
    {
        public Guid Id { get; set; }
        public string Keyword { get; set; }
        public string SortBy { get; set; }
        public int TotalListing { get; set; }


        public class CreateKeywordCommandHandler : IRequestHandler<CreateKeywordCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;


            public CreateKeywordCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(CreateKeywordCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
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

                    var newKeyword = new GoogleMerchantKeywords
                    {
                        Id = new Guid(),
                        Keyword = request.Keyword,
                        SortBy = request.SortBy,
                        TotalListing = request.TotalListing
                    };

                    await rewardsDBContext.GoogleMerchantKeywords.AddAsync(newKeyword);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully created keywords";
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to create keyword";
                }

                return apiResponseViewModel;
            }
        }

    }
}
