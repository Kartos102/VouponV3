using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Microsoft.EntityFrameworkCore;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Queries.List
{
    public class ListBanksQuery : ListQueryRequest<ListView<ListBanksQueryViewModel>>
    {
    }
    public class ListBanksQueryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string BankCode { get; set; }
        public string SWIFTCode { get; set; }
        public int CountryId { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
    }

    public class ListAdsPlacementQueryHandler : IRequestHandler<ListBanksQuery, ListView<ListBanksQueryViewModel>>
    {
        RewardsDBContext rewardsDBContext;
        public ListAdsPlacementQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ListView<ListBanksQueryViewModel>> Handle(ListBanksQuery request, CancellationToken cancellationToken)
        {

            var banks = await rewardsDBContext.Banks.ToListAsync();

            return new ListView<ListBanksQueryViewModel>(banks.Count(), request.PageSize, request.PageIndex)
            {
                Items = banks.Select(x => new ListBanksQueryViewModel
                {
                    Id = x.Id,
                }).ToList()
            };
        }
    }
}
