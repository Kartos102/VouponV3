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
    public class MaxOrderFilterPage : IRequest<ApiResponseViewModel>
    {
        private class MaxOrderFilterPageHandler : IRequestHandler<MaxOrderFilterPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public MaxOrderFilterPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(MaxOrderFilterPage request, CancellationToken cancellationToken)
            {
                return new ApiResponseViewModel
                {
                    Successful = true,
                    Data = new MaxOrderFilterPageViewModel
                    {
                        CreateViewModel = new DataEntryViewModel
                        {
                            MaxQuantity = 1
                        },
                        EditViewModel = new DataEntryViewModel()
                    }
                };
            }
        }
        public class MaxOrderFilterPageViewModel
        {
            public DataEntryViewModel CreateViewModel { get; set; }
            public DataEntryViewModel EditViewModel { get; set; }
        }

        public class DataEntryViewModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Keyword is required")]
            public string Keyword { get; set; }

            public byte StatusId { get; set; }

            [Required(ErrorMessage = "Max Order Quantity is required")]
            [Range(0, int.MaxValue, ErrorMessage = "Max order quantity must be 1 or more")]
            public byte MaxQuantity { get; set; }

            public DateTime CreatedAt { get; set; }
        }
    }
}
