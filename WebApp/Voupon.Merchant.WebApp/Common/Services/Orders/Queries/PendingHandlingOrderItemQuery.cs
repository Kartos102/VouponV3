using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Orders.Queries
{
    public class PendingHandlingOrderItemQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public int Status { get; set; }

        public class PendingHandlingOrderItemQueryHandler : IRequestHandler<PendingHandlingOrderItemQuery, ApiResponseViewModel>
        {
            private readonly IOptions<AppSettings> appSettings;
            RewardsDBContext rewardsDBContext;
            public PendingHandlingOrderItemQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(PendingHandlingOrderItemQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var viewModel = new PendingHandlingViewModel();
                    var items = new List<PendingHandlingItemViewModel>();
                    /*
                    var inStorePendingHandling = await rewardsDBContext.InStoreRedemptionTokens.Include(x => x.OrderItem).ThenInclude(x => x.Order).Where(x => x.MerchantId == request.MerchantId && x.OrderItem.Order.OrderStatus == 2).Select(x => new PendingHandlingItemViewModel
                    {
                        Id = x.Id,
                        ProductId = x.ProductId,
                        CreatedAt = x.CreatedAt,
                        OrderItemId = x.OrderItemId,
                        ProductTitle = x.ProductTitle,
                        IsRedeemed = x.IsRedeemed,
                        IsActivated = x.IsActivated,
                        ShortOrderItemId = x.OrderItem.ShortId,
                        ShortOrderId = x.OrderItem.Order.ShortId,
                        Revenue = x.OrderItem.Price,
                        Email = x.Email,
                        Status = x.OrderItem.Status.ToString(),
                        Type = 1 // Move to enum?
                    }).ToListAsync();
                    

                    if (inStorePendingHandling != null && inStorePendingHandling.Any())
                    {
                        items.AddRange(inStorePendingHandling);
                        viewModel.TotalDigital = inStorePendingHandling.Count();
                    }
                    */
                    var promoCodes = await rewardsDBContext.PromoCodes.ToDictionaryAsync(x => x.Id);
                    PromoCodes promo = new PromoCodes();
                    var digitalPendingHandling = await rewardsDBContext.DigitalRedemptionTokens.
                        AsNoTracking().
                        Include(x => x.OrderItem).
                        ThenInclude(x => x.Order).
                        Include(x => x.Product).
                        Where(x => x.MerchantId == request.MerchantId && x.UpdateTokenAt == null && (x.OrderItem.Order.OrderStatus != (int)OrderStatus.Done || x.OrderItem.Order.OrderStatus != (int)OrderStatus.Completed) && (x.OrderItem.Status == (int)OrderStatus.Pending || x.OrderItem.Status == (int)OrderStatus.Sent || x.OrderItem.Status == (int)OrderStatus.PendingPayment || x.OrderItem.Status == (int)OrderStatus.PendingRefund)).Select(x => new PendingHandlingItemViewModel
                        {
                            Id = x.Id,
                            ProductId = x.ProductId,
                            CreatedAt = x.CreatedAt,
                            OrderItemId = x.OrderItemId,
                            ProductTitle = x.ProductTitle,
                            IsRedeemed = x.IsRedeemed,
                            IsActivated = x.IsActivated,
                            ShortOrderItemId = x.OrderItem.ShortId,
                            ShortOrderId = x.OrderItem.Order.ShortId,
                            Revenue = x.OrderItem.Price,
                            Email = x.Email,
                            Status = x.OrderItem.Status,
                            Type = 3, // Move to enum?
                            ThirdPartyTypeId = (x.OrderItem.Product.ThirdPartyTypeId.HasValue ? 1 : 0),
                            OrderId = x.OrderItem.OrderId,
                            ShippingAddress = x.OrderItem.Order.ShippingAddressLine1,
                            ShippingName = x.OrderItem.Order.ShippingPersonFirstName + " " + x.OrderItem.Order.ShippingPersonLastName,
                            Quantity = 1,
                            PromoCodeDiscount = x.OrderItem.Order.PromoCodeId != null ? x.OrderItem.Order.PromoCodeMaxDiscountValue : null,
                            PromoCodeName = promoCodes.TryGetValue((Guid)x.OrderItem.Order.PromoCodeId, out promo) ? promo.Description : null,
                            ProductUrl = x.ProductId != 0 ? appSettings.Value.App.VouponUrl + "/product/" + x.ProductId : null,
                        }).ToListAsync();

                    if (digitalPendingHandling != null && digitalPendingHandling.Any())
                    {
                        items.AddRange(digitalPendingHandling);
                        viewModel.TotalDigital = digitalPendingHandling.Count();
                    }

                    var deliveryPendingHandling = await rewardsDBContext.DeliveryRedemptionTokens.
                        AsNoTracking().
                        Include(x => x.OrderItem).
                        ThenInclude(x => x.Order).
                        Where(x => x.MerchantId == request.MerchantId && x.UpdateTokenAt == null && (x.OrderItem.Order.OrderStatus != (int)OrderStatus.Done || x.OrderItem.Order.OrderStatus != (int)OrderStatus.Completed) && (x.OrderItem.Status == (int)OrderStatus.Pending || x.OrderItem.Status == (int)OrderStatus.Sent || x.OrderItem.Status == (int)OrderStatus.PendingPayment || x.OrderItem.Status == (int)OrderStatus.PendingRefund)).Select(x => new PendingHandlingItemViewModel
                        {

                            Id = x.Id,
                            ProductId = x.ProductId,
                            CreatedAt = x.CreatedAt,
                            OrderItemId = x.OrderItemId,
                            ProductTitle = x.ProductTitle,
                            IsRedeemed = x.IsRedeemed,
                            IsActivated = x.IsActivated,
                            ShortOrderItemId = x.OrderItem.ShortId,
                            ShortOrderId = x.OrderItem.Order.ShortId,
                            Revenue = x.OrderItem.Price,
                            Email = x.Email,
                            Status = x.OrderItem.Status,
                            Type = 2 // Move to enum?
                        ,
                            Variant = x.OrderItem.VariationText,
                            ShippingAddress = x.OrderItem.Order.ShippingAddressLine1,
                            ShippingName = x.OrderItem.Order.ShippingPersonFirstName + " " + x.OrderItem.Order.ShippingPersonLastName,
                            OrderId = x.OrderItem.OrderId,
                            PromoCodeDiscount = x.OrderItem.Order.PromoCodeId != null ? x.OrderItem.Order.PromoCodeMaxDiscountValue : null,
                            PromoCodeName = promoCodes.TryGetValue((Guid)x.OrderItem.Order.PromoCodeId, out promo) ? promo.Description : null,
                            ProductUrl = x.ProductId != 0 ? appSettings.Value.App.VouponUrl + "/product/" + x.ProductId : null,
                        }).ToListAsync();


                    if (deliveryPendingHandling != null && deliveryPendingHandling.Any())
                    {
                        int tempProductId = 0;
                        var orderId = new Guid();
                        foreach (var deliveryItem in deliveryPendingHandling)
                        {
                            if (tempProductId == deliveryItem.ProductId && orderId == deliveryItem.OrderId)
                            {
                                continue;
                            }

                            //For some orders duplicate DELIVERY INFO IS AVALIABLE FOR SAME ORDER ITEM, this was used to omit it. Without these orders will show double order quantity in the pending screen
                            var deliveryUniqueOrderCount = deliveryPendingHandling.Where(x => x.ProductId == deliveryItem.ProductId && x.OrderId == deliveryItem.OrderId && x.OrderItemId == deliveryItem.OrderItemId).Count();

                            if (deliveryUniqueOrderCount > 1)
                            {
                                deliveryItem.Quantity = (deliveryPendingHandling.Where(x => x.ProductId == deliveryItem.ProductId && x.OrderId == deliveryItem.OrderId).Count()) / deliveryUniqueOrderCount;

                            }

                            else
                            {
                                deliveryItem.Quantity = deliveryPendingHandling.Where(x => x.ProductId == deliveryItem.ProductId && x.OrderId == deliveryItem.OrderId).Count();

                            }

                            deliveryItem.Revenue = deliveryItem.Revenue * deliveryItem.Quantity;
                            items.Add(deliveryItem);
                            tempProductId = deliveryItem.ProductId;
                            orderId = deliveryItem.OrderId;
                        }
                        //items.AddRange(deliveryPendingHandling);
                        viewModel.TotalDelivery = deliveryPendingHandling.Count();
                    }

                    if (items != null && items.Any())
                    {
                        viewModel.PendingHandlingItems = items.OrderBy(x => x.CreatedAt).ToList();
                        apiResponseViewModel.Data = viewModel;
                    }
                    else
                    {
                        apiResponseViewModel.Data = null;
                    }
                    apiResponseViewModel.Successful = true;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;
                }
                return apiResponseViewModel;
            }
        }

        public class PendingHandlingViewModel
        {
            public int TotalDelivery { get; set; }
            public int TotalInStore { get; set; }
            public int TotalDigital { get; set; }
            public List<PendingHandlingItemViewModel> PendingHandlingItems { get; set; }
        }

        public class PendingOrdersViewModel
        {
            public Guid OrderId { get; set; }
            public string Email { get; set; }
            public decimal Revenue { get; set; }
            public short Status { get; set; }
            //public string LastUpdatedByUserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public string ShippingAddress { get; set; }
            public string ShippingName { get; set; }
            //public DateTime? LastUpdatedAt { get; set; }
            public decimal? PromoCodeDiscount { get; set; }
            public string PromoCodeName { get; set; }

            public string ShortOrderId { get; set; }
            public List<PendingHandlingItemViewModel> pendingHandlingItemViewModels { get; set; }
        }
        public class PendingHandlingItemViewModel
        {
            public int Id { get; set; }

            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public Guid OrderItemId { get; set; }
            public string ProductTitle { get; set; }
            public string Token { get; set; }
            public bool IsRedeemed { get; set; }
            public bool IsActivated { get; set; }
            public DateTime CreatedAt { get; set; }
            //public DateTime LastUpdatedAt { get; set; }
            public Guid OrderId { get; set; }
            //public string LastUpdatedBy { get; set; }
            public DateTime? RedeemedAt { get; set; }
            public decimal? Revenue { get; set; }
            public string Email { get; set; }
            public short Status { get; set; }
            public int Type { get; set; }
            public string ProductUrl { get; set; }
            public int ThirdPartyTypeId { get; set; }

            public string ShortOrderItemId { get; set; }
            public string ShortOrderId { get; set; }
            public string ShippingAddress { get; set; }
            public string ShippingName { get; set; }
            public string Variant { get; set; }
            public decimal? PromoCodeDiscount { get; set; }
            public string PromoCodeName { get; set; }
        }


    }

}
