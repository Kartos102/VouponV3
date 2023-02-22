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
using static Voupon.Merchant.WebApp.Areas.Admin.Services.PromoCode.Queries.List.MailingListQuery;
using static Voupon.Merchant.WebApp.Areas.Admin.Services.PromoCode.Queries.List.PromoCodeListQuery;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.PromoCode.Queries.List
{
    public class MailingListQuery : ListQueryRequest<ListView<MailingListViewModel>>
    {
        public class MailingListViewModel
        {
            public string Email { get; set; }
            public DateTime CreatedAt { get; set; }
            public bool? IsSubscribe { get; set; }
            public DateTime? LastEmailedAt { get; set; }
            public DateTime? UnSubscribeAt { get; set; }
        }

        public class MailingListQueryHandler : IRequestHandler<MailingListQuery, ListView<MailingListViewModel>>
        {
            RewardsDBContext rewardsDBContext;
            public MailingListQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }
            public async Task<ListView<MailingListViewModel>> Handle(MailingListQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var totalItems = await rewardsDBContext.MailingLists.CountAsync();
                    var mailingList = await rewardsDBContext.MailingLists.OrderByDescending(x => x.CreatedAt).ToListAsync();

                    return new ListView<MailingListViewModel>(totalItems, request.PageSize, request.PageIndex)
                    {
                        Items = mailingList.Select(x => new MailingListViewModel
                        {
                            Email = x.Email,
                            CreatedAt = x.CreatedAt,
                            IsSubscribe = x.IsSubscribe
                        }).ToList()
                    };

                }
                catch (Exception ex)
                {
                    return new ListView<MailingListViewModel>(0, request.PageSize, request.PageIndex, error: "Fail to get items");
                }
            }
        }
    }
}
