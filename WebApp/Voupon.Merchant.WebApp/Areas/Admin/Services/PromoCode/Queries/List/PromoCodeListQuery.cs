using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;
using static Voupon.Merchant.WebApp.Areas.Admin.Services.PromoCode.Queries.List.PromoCodeListQuery;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.PromoCode.Queries.List
{
    public class PromoCodeListQuery : ListQueryRequest<ListView<PromoCodeViewModel>>
    {

        public class PromoCodeViewModel
        {
            public Guid Id { get; set; }
            public string PromoCode { get; set; }
            public string Description { get; set; }
            public short Status { get; set; }
            public short DiscountType { get; set; }
            public decimal DiscountValue { get; set; }
            public DateTime ExpireOn { get; set; }
            public decimal MinSpend { get; set; }
            public decimal MaxDiscountValue { get; set; }
            public int TotalRedemptionAllowed { get; set; }
            public int TotalRedeemed { get; set; }
            public bool IsFirstTimeUserOnly { get; set; }
            public bool IsNewSignupUserOnly { get; set; }
            public bool IsSelectedUserOnly { get; set; }
            public bool IsShipCostDeduct { get; set; }
            public int TotalAllowedPerUser { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
        }

        public class PromoCodeListQueryHandler : IRequestHandler<PromoCodeListQuery, ListView<PromoCodeViewModel>>
        {
            RewardsDBContext rewardsDBContext;
            public PromoCodeListQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }
            public async Task<ListView<PromoCodeViewModel>> Handle(PromoCodeListQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var totalItems = await rewardsDBContext.PromoCodes.CountAsync();
                    var promoCodes = await rewardsDBContext.PromoCodes.OrderBy(x => x.PromoCode).ToListAsync();

                    return new ListView<PromoCodeViewModel>(totalItems, request.PageSize, request.PageIndex)
                    {
                        Items = promoCodes.Select(x => new PromoCodeViewModel
                        {
                            Id = x.Id,
                            PromoCode = x.PromoCode,
                            ExpireOn = x.ExpireOn,
                            Description = x.Description,
                            DiscountType = x.DiscountType,
                            DiscountValue = x.DiscountValue,
                            MaxDiscountValue = x.MaxDiscountValue,
                            IsFirstTimeUserOnly = x.IsFirstTimeUserOnly,
                            IsNewSignupUserOnly = x.IsNewSignupUserOnly,
                            IsSelectedUserOnly = x.IsSelectedUserOnly,
                            MinSpend = x.MinSpend,
                            Status = x.Status,
                            TotalAllowedPerUser = x.TotalAllowedPerUser,
                            TotalRedeemed = x.TotalRedeemed,
                            TotalRedemptionAllowed = x.TotalRedemptionAllowed,
                            IsShipCostDeduct = x.IsShipCostDeduct

                        }).ToList()
                    };

                }
                catch (Exception ex)
                {
                    return new ListView<PromoCodeViewModel>(0, request.PageSize, request.PageIndex, error: "Fail to get items");
                }
            }
        }
    }
}