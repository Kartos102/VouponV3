using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Config.Commands
{
    public class UpdateConfigCommand : IRequest<ApiResponseViewModel>
    {
        public decimal RinggitPerVpoints { get; set; }
        public bool IsVPointsMultiplierEnabled { get; set; }
        public decimal VPointsMultiplier { get; set; }
        public decimal VPointsMultiplierCap { get; set; }
        public decimal DefaultCommission { get; set; }

        public bool IsCheckoutEnabled { get; set; }
        public bool IsPassPaymentGatewayEnabled { get; set; }
        public bool IsErrorLogEmailEnabled { get; set; }
        public byte MaxOrderFilter { get; set; }
        public Guid UpdatedBy { get; set; }

        public class UpdateConfigCommandHandler : IRequestHandler<UpdateConfigCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            public UpdateConfigCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateConfigCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    // var result = await UserManager.UpdateSecurityStampAsync(user.Id);
                    var appConfig = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                    if (appConfig == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [001]";
                        return apiResponseViewModel;
                    }
                    bool skipUpdateRinggitPerVpoints = false;
                    if (appConfig.RinggitPerVpoints == request.RinggitPerVpoints)
                    {
                        skipUpdateRinggitPerVpoints = true;
                    }
                    appConfig.RinggitPerVpoints = request.RinggitPerVpoints;
                    appConfig.IsCheckoutEnabled = request.IsCheckoutEnabled;
                    appConfig.IsPassPaymentGatewayEnabled = request.IsPassPaymentGatewayEnabled;
                    appConfig.IsErrorLogEmailEnabled = request.IsErrorLogEmailEnabled;
                    appConfig.IsVPointsMultiplierEnabled = request.IsVPointsMultiplierEnabled;
                    appConfig.VPointsMultiplier = request.VPointsMultiplier;
                    appConfig.VPointsMultiplierCap = request.VPointsMultiplierCap;
                    appConfig.MaxOrderFilter = request.MaxOrderFilter;
                    appConfig.LastUpdatedAt = DateTime.Now;
                    appConfig.LastUpdatedBy = request.UpdatedBy;

                    if(appConfig.DefaultCommission != request.DefaultCommission)
                    {
                        var merchants = await rewardsDBContext.Merchants.ToListAsync();
                        if(merchants != null)
                        {
                            foreach(var merchant in merchants)
                            {
                                if(merchant.Id == 1)
                                {
                                    continue;
                                }
                                merchant.DefaultCommission = request.DefaultCommission;
                                merchant.LastUpdatedAt = DateTime.Now;
                                merchant.LastUpdatedByUserId = request.UpdatedBy;
                                var productList = await rewardsDBContext.Products.Where(x => x.MerchantId == merchant.Id).ToListAsync();
                                if (productList != null)
                                {
                                    foreach (var product in productList)
                                    {
                                        product.DefaultCommission = request.DefaultCommission;
                                    }
                                }
                            }
                        }
                    }

                    rewardsDBContext.AppConfig.Update(appConfig);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = skipUpdateRinggitPerVpoints;

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to update config [0003]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
