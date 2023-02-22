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
    public class ItemFilterPage : IRequest<ApiResponseViewModel>
    {
        public string FileUrl { get; set; }
        private class ItemFilterPageHandler : IRequestHandler<ItemFilterPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ItemFilterPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ItemFilterPage request, CancellationToken cancellationToken)
            {
                return new ApiResponseViewModel
                {
                    Successful = true,
                    Data = new ItemFilterPageViewModel
                    {
                        CreateViewModel = new DataEntryViewModel(),
                        EditViewModel = new DataEntryViewModel()
                    }
                };
            }
        }
        public class ItemFilterPageViewModel
        {
            public DataEntryViewModel CreateViewModel { get; set; }
            public DataEntryViewModel EditViewModel { get; set; }
        }

        public class DataEntryViewModel
        {
            public int Id { get; set; }

            [Required(ErrorMessage = "Product URL is required")]
            public string ProductUrl { get; set; }

            public byte StatusId { get; set; }

            public DateTime CreatedAt { get; set; }
        }
    }
}
