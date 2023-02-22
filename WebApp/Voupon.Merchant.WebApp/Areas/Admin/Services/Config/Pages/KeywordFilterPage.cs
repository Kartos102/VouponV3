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
    public class KeywordFilterPage : IRequest<ApiResponseViewModel>
    {
        private class KeywordFilterPageHandler : IRequestHandler<KeywordFilterPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public KeywordFilterPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(KeywordFilterPage request, CancellationToken cancellationToken)
            {
                return new ApiResponseViewModel
                {
                    Successful = true,
                    Data = new KeywordFilterPageViewModel
                    {
                        CreateViewModel = new DataEntryViewModel(),
                        EditViewModel = new DataEntryViewModel()
                    }
                };
            }
        }
        public class KeywordFilterPageViewModel
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

            public DateTime CreatedAt { get; set; }
        }
    }
}
