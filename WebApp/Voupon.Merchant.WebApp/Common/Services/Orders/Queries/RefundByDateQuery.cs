using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Orders.Queries
{
    public class RefundByDateQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public class RefundByDateQueryHandler : IRequestHandler<RefundByDateQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public RefundByDateQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(RefundByDateQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var refundItems = new List<RefundItemViewModel>();
                    if(request.MerchantId == 0)
                    {

                        refundItems = await rewardsDBContext.Refunds.Include(x => x.OrderItem).ThenInclude(x => x.Order).Where(x => x.CreatedAt.Date >= request.From && x.CreatedAt.Date <= request.To).OrderByDescending(x => x.CreatedAt).Select(x => new RefundItemViewModel
                        {
                            BuyerEmail = x.OrderItem.Order.Email,
                            BuyerMobileNumber = x.OrderItem.Order.BillingMobileCountryCode + x.OrderItem.Order.BillingMobileNumber,
                            BuyerName = x.OrderItem.Order.BillingPersonFirstName + " " + x.OrderItem.Order.BillingPersonLastName,
                            MasterMemberProfileId = x.OrderItem.Order.MasterMemberProfileId,
                            MoneyRefunded = x.MoneyRefunded,
                            PointsRefunded = x.PointsRefunded,
                            ProductImageurl = x.OrderItem.ProductImageFolderUrl.Replace("http://", "https://"),
                            ProductTitle = x.OrderItem.ProductTitle,
                            RefundedAt = x.CreatedAt,
                            RefundedAtString = x.CreatedAt.ToString("dd/MM/yyyy"),
                            RefundId = x.Id,
                            MerchantId = x.OrderItem.MerchantId,
                            MerchantName = x.OrderItem.MerchantDisplayName,
                            OrderItemStatus = x.OrderItem.Status
                        }).ToListAsync();
                    }
                    else
                    {
                        refundItems = await rewardsDBContext.Refunds.Include(x => x.OrderItem).ThenInclude(x => x.Order).Where(x => x.CreatedAt.Date >= request.From && x.CreatedAt.Date <= request.To && x.OrderItem.MerchantId == request.MerchantId).OrderByDescending(x => x.CreatedAt).Select(x => new RefundItemViewModel
                        {
                            BuyerEmail = x.OrderItem.Order.Email,
                            BuyerMobileNumber = x.OrderItem.Order.BillingMobileCountryCode + x.OrderItem.Order.BillingMobileNumber,
                            BuyerName = x.OrderItem.Order.BillingPersonFirstName + " " + x.OrderItem.Order.BillingPersonLastName,
                            MasterMemberProfileId = x.OrderItem.Order.MasterMemberProfileId,
                            MoneyRefunded = x.MoneyRefunded,
                            PointsRefunded = x.PointsRefunded,
                            ProductImageurl = x.OrderItem.ProductImageFolderUrl.Replace("http://", "https://"),
                            ProductTitle = x.OrderItem.ProductTitle,
                            RefundedAt = x.CreatedAt,
                            RefundedAtString = x.CreatedAt.ToString("dd/MM/yyyy"),
                            RefundId = x.Id,
                            OrderItemStatus = x.OrderItem.Status
                        }).ToListAsync();
                    }

                    if (refundItems != null && refundItems.Any())
                    {
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Data = refundItems;
                    }
                    else
                    {
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Message = "No items";
                        apiResponseViewModel.Data = null;
                    }
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = ex.Message;
                }

                return apiResponseViewModel;
            }
        }

        public class RefundItemViewModel
        {
            public Guid RefundId { get; set; }
            public DateTime RefundedAt { get; set; }

            public string RefundedAtString { get; set; }
            public int PointsRefunded { get; set; }

            public decimal MoneyRefunded { get; set; }

            public string ProductTitle { get; set; }
            public string ProductImageurl { get; set; }

            public string BuyerEmail { get; set; }
            public string BuyerName { get; set; }

            public string BuyerMobileNumber { get; set; }
            public int MasterMemberProfileId { get; set; }
            public int OrderItemStatus { get; set; }

            public string MerchantName { get; set; }

            public int MerchantId { get; set; }

        }


    }

}
