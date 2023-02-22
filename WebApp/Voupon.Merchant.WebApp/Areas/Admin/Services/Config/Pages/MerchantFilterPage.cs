using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages
{
    public class MerchantFilterPage : IRequest<ApiResponseViewModel>
    {
        public string FileUrl { get; set; }
        private class MerchantFilterPageHandler : IRequestHandler<MerchantFilterPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public MerchantFilterPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(MerchantFilterPage request, CancellationToken cancellationToken)
            {
                return new ApiResponseViewModel
                {
                    Successful = true,
                    Data = new MerchantFilterPageViewModel
                    {
                        CreateViewModel = new DataEntryViewModel(),
                        EditViewModel = new DataEntryViewModel()
                    }
                };
            }
        }
        public class MerchantFilterPageViewModel
        {
            public DataEntryViewModel CreateViewModel { get; set; }
            public DataEntryViewModel EditViewModel { get; set; }
        }

        public class DataEntryViewModel
        {
            public int Id { get; set; }

            public string MerchantId { get; set; }

            [Required(ErrorMessage = "MerchantId is required")]
            public string MerchantUsername { get; set; }

            public byte StatusId { get; set; }

            public DateTime CreatedAt { get; set; }
        }
    }
}
