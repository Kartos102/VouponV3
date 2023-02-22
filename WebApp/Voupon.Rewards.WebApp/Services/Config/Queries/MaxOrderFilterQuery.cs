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
    public class MaxOrderFilter
    {
        public int Id { get; set; }
        public string Keyword { get; set; }
        public byte StatusId { get; set; }
        public byte MaxQuantity { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class MaxOrderFilterQuery : IRequest<List<MaxOrderFilter>>
    {

        public class MaxOrderFilterQueryHandler : IRequestHandler<MaxOrderFilterQuery, List<MaxOrderFilter>>
        {
            RewardsDBContext rewardsDBContext;

            public MaxOrderFilterQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<List<MaxOrderFilter>> Handle(MaxOrderFilterQuery request, CancellationToken cancellationToken)
            {
                return rewardsDBContext.AggregatorMaxQuantityFilters.AsNoTracking().Where(x => x.StatusId == 1).OrderByDescending(x => x.MaxQuantity).ToList().Select(x => new MaxOrderFilter
                {
                    Id = x.Id,
                    MaxQuantity = (byte)x.MaxQuantity,
                    Keyword = x.Keyword
                }).ToList();

            }
        }
    }
}
