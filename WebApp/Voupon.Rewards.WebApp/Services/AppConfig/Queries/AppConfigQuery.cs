using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;

namespace Voupon.Rewards.WebApp.Services.AppConfig.Queries
{
    public class AppConfigModel
    {
        public int Id { get; set; }
        public decimal RinggitPerVpoints { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedBy { get; set; }
        public bool? IsCheckoutEnabled { get; set; }
        public bool IsPassPaymentGatewayEnabled { get; set; }
        public bool IsErrorLogEmailEnabled { get; set; }
        public bool IsVPointsMultiplierEnabled { get; set; }
        public decimal VPointsMultiplier { get; set; }
        public decimal VPointsMultiplierCap { get; set; }
        public int MaxOrderFilter { get; set; }
    }
    public class AppConfigQuery : IRequest<AppConfigModel>
    {

        public class AppConfigQueryHandler : IRequestHandler<AppConfigQuery, AppConfigModel>
        {
            RewardsDBContext rewardsDBContext;

            public AppConfigQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }


            public async Task<AppConfigModel> Handle(AppConfigQuery request, CancellationToken cancellationToken)
            {
                var result = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                return new AppConfigModel
                {
                    VPointsMultiplier = result.VPointsMultiplier,
                    VPointsMultiplierCap = result.VPointsMultiplierCap,
                    IsVPointsMultiplierEnabled = result.IsVPointsMultiplierEnabled,
                    MaxOrderFilter = result.MaxOrderFilter
                };
            }
        }
    }
}
