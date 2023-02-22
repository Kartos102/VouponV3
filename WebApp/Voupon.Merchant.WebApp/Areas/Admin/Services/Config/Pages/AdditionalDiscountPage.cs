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
    public class AdditionalDiscountPage : IRequest<ApiResponseViewModel>
    {
        private class AdditionalDiscountPageHandler : IRequestHandler<AdditionalDiscountPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public AdditionalDiscountPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(AdditionalDiscountPage request, CancellationToken cancellationToken)
            {
                return new ApiResponseViewModel
                {
                    Successful = true,
                    Data = new AdditionalDiscountPageViewModel
                    {
                        CreateViewModel = new DataEntryViewModel(),
                        EditViewModel = new DataEntryViewModel()
                    }
                };
            }
        }
        public class AdditionalDiscountPageViewModel
        {
            public DataEntryViewModel CreateViewModel { get; set; }
            public DataEntryViewModel EditViewModel { get; set; }
        }

        public class DataEntryViewModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Max Price is required")]
            public decimal MaxPrice { get; set; }

            [Required(ErrorMessage = "Discount percentage is required")]
            public decimal DiscountPercentage { get; set; }

            [Required(ErrorMessage = "Points is required")]
            public short Points { get; set; }

            public byte StatusId { get; set; }

            public DateTime CreatedAt { get; set; }
        }
    }
}
