using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Pages
{
    public class IndexPage : IRequest<ApiResponseViewModel>
    {
        public string FileUrl { get; set; }
        private class IndexPageHandler : IRequestHandler<IndexPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public IndexPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(IndexPage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                var result = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();

                if (result == null)
                {
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }

                var viewModel = new IndexPageViewModel
                {
                    Id = result.Id,
                    LastUpdatedAt = result.LastUpdatedAt,
                    LastUpdatedBy = result.LastUpdatedBy,
                    RinggitPerVpoints = result.RinggitPerVpoints,
                    IsCheckoutEnabled = result.IsCheckoutEnabled.Value,
                    IsPassPaymentGatewayEnabled = result.IsPassPaymentGatewayEnabled,
                    IsErrorLogEmailEnabled = result.IsErrorLogEmailEnabled,
                    IsVPointsMultiplierEnabled = result.IsVPointsMultiplierEnabled,
                    VPointsMultiplier = result.VPointsMultiplier,
                    VPointsMultiplierCap = result.VPointsMultiplierCap,
                    DefaultCommission = result.DefaultCommission,
                    MaxOrderFilter = result.MaxOrderFilter
                    
                };

                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Data = viewModel;
                return apiResponseViewModel;
            }
        }
        public class IndexPageViewModel
        {
            public int Id { get; set; }
            public bool IsCheckoutEnabled { get; set; }
            public bool IsPassPaymentGatewayEnabled { get; set; }
            public bool IsErrorLogEmailEnabled { get; set; }
            public decimal RinggitPerVpoints { get; set; }

            public bool IsVPointsMultiplierEnabled { get; set; }
            public decimal VPointsMultiplier { get; set; }
            public decimal VPointsMultiplierCap { get; set; }
            public decimal DefaultCommission { get; set; }
            public short MaxOrderFilter { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public Guid? LastUpdatedBy { get; set; }
        }

    }
}
