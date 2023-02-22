using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Order.Pages
{
    public class RefundsPage : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
        private class RefundsPageHandler : IRequestHandler<RefundsPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            public RefundsPageHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(RefundsPage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var refunds = await rewardsDBContext.Refunds.Include(x => x.OrderItem).Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId).Select(x => new RefundsItemViewModel
                {
                    Id = x.Id,
                    CreatedAt = x.CreatedAt,
                    ProductImageUrl = x.OrderItem.ProductImageFolderUrl.Replace("http://", "https://"),
                    ProductTitle = x.OrderItem.ProductTitle,
                    MerchantName = x.OrderItem.MerchantDisplayName,
                    VariationText = x.OrderItem.VariationText,
                    IsVariationProduct = x.OrderItem.IsVariationProduct,
                    PointsRefunded = x.PointsRefunded,
                    MoneyRefunded = x.MoneyRefunded,
                    Remark = x.Remark,
                    Status = (byte)x.Status,
                    ShortId = x.ShortId,
                    OrderItemShortId = x.OrderItem.ShortId
                }).ToListAsync();

                var viewModel = new RefundsPageViewModel
                {
                    RefundedItems = refunds,
                    TotalItemRefunded = refunds.Count(),
                    TotalMoneyRefunded = refunds.Sum(x => x.MoneyRefunded),
                    TotalPointsRefunded = refunds.Sum(x => x.PointsRefunded)
                };

                apiResponseViewModel.Data = viewModel;
                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
        }
        public class RefundsPageViewModel
        {
            public int TotalPointsRefunded { get; set; }
            public decimal TotalMoneyRefunded { get; set; }
            public int TotalItemRefunded { get; set; }
            public List<RefundsItemViewModel> RefundedItems { get; set; }
        }

        public class RefundsItemViewModel
        {
            public Guid Id { get; set; }
            public Guid OrderItemId { get; set; }
            public byte Type { get; set; }
            public byte Status { get; set; }
            public int PointsRefunded { get; set; }
            public decimal MoneyRefunded { get; set; }
            public string Remark { get; set; }
            public DateTime CreatedAt { get; set; }
            public Guid CreatedBy { get; set; }

            public string MerchantName { get; set; }
            public string ProductTitle { get; set; }
            public string ProductImageUrl { get; set; }

            public string OrderItemShortId { get; set; }

            public int MasterMemberProfileId { get; set; }

            public string ShortId { get; set; }

            public string VariationText { get; set; }
            public bool IsVariationProduct { get; set; }
        }

    }

}
