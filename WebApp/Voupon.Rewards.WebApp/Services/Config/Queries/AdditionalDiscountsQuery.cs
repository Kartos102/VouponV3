using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;

namespace Voupon.Rewards.WebApp.Services.Config.Queries
{
    public class AdditionalDiscounts
    {
        public int Id { get; set; }
        public short StatusId { get; set; }
        public decimal MaxPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public short Points { get; set; }
    }

    public class AdditionalDiscountsQuery : IRequest<List<AdditionalDiscounts>>
    {

        public class AdditionalDiscountsQueryHandler : IRequestHandler<AdditionalDiscountsQuery, List<AdditionalDiscounts>>
        {
            RewardsDBContext rewardsDBContext;

            public AdditionalDiscountsQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<List<AdditionalDiscounts>> Handle(AdditionalDiscountsQuery request, CancellationToken cancellationToken)
            {
                return rewardsDBContext.AdditionalDiscounts.AsNoTracking().Where(x => x.StatusId == 1).OrderByDescending(x => x.MaxPrice).ToList().Select(x => new AdditionalDiscounts
                {
                    Id = x.Id,
                    DiscountPercentage = x.DiscountPercentage,
                    MaxPrice = x.MaxPrice,
                    Points = x.Points,
                    StatusId = x.StatusId
                }).ToList();

            }
        }
    }
}
