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
    public class RefundOrderItemQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public int Status { get; set; }

        public class RefundOrderItemQueryHandler : IRequestHandler<RefundOrderItemQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public RefundOrderItemQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(RefundOrderItemQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var viewModel = new RefundViewModel();
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
                    var promoCodes = await rewardsDBContext.PromoCodes.ToDictionaryAsync(x=> x.Id);
                    PromoCodes promo = new PromoCodes();
                    var digitalPendingHandling = await rewardsDBContext.DigitalRedemptionTokens.AsNoTracking().Include(x => x.OrderItem).ThenInclude(x => x.Order).Include(x => x.Product).Where(x => x.MerchantId == request.MerchantId && x.UpdateTokenAt == null && x.OrderItem.Order.OrderStatus == 2 &&( x.OrderItem.Status == 4 || x.OrderItem.Status == 5 || x.OrderItem.Status == 8 || x.OrderItem.Status == 9)).Select(x => new PendingHandlingItemViewModel
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
                        Quantity = 1,
                        PromoCodeDiscount = x.OrderItem.Order.PromoCodeId != null? x.OrderItem.Order.PromoCodeMaxDiscountValue: null,
                        PromoCodeName = promoCodes.TryGetValue((Guid)x.OrderItem.Order.PromoCodeId, out promo) ? promo.Description: null,
                    }).ToListAsync();

                    if (digitalPendingHandling != null && digitalPendingHandling.Any())
                    {
                        items.AddRange(digitalPendingHandling);
                        viewModel.TotalDigital = digitalPendingHandling.Count();
                    }

                    var deliveryPendingHandling = await rewardsDBContext.DeliveryRedemptionTokens.AsNoTracking().Include(x => x.OrderItem).ThenInclude(x => x.Order).Where(x => x.MerchantId == request.MerchantId && (x.OrderItem.Status == 4 || x.OrderItem.Status == 5 || x.OrderItem.Status == 8 || x.OrderItem.Status == 9)).Select(x => new PendingHandlingItemViewModel
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
                        , Variant= x.OrderItem.VariationText,
                        ShippingAddress = x.OrderItem.Order.ShippingAddressLine1,
                        ShippingName = x.OrderItem.Order.ShippingPersonFirstName + " " + x.OrderItem.Order.ShippingPersonLastName,
                        OrderId = x.OrderItem.OrderId,
                        PromoCodeDiscount = x.OrderItem.Order.PromoCodeId != null ? x.OrderItem.Order.PromoCodeMaxDiscountValue : null,
                        PromoCodeName = promoCodes.TryGetValue((Guid)x.OrderItem.Order.PromoCodeId, out promo) ? promo.Description : null,
                    }).ToListAsync();

                    if (deliveryPendingHandling != null && deliveryPendingHandling.Any())
                    {
                        int tempProductId = 0;
                        string tempProductorderId = "";
                        foreach(var deliveryItem in deliveryPendingHandling)
                        {
                            if(tempProductId == deliveryItem.ProductId && new Guid(tempProductorderId) == deliveryItem.OrderId)
                            {
                                continue;
                            }
                            deliveryItem.Quantity = deliveryPendingHandling.Where(x => x.ProductId == deliveryItem.ProductId && x.OrderId == deliveryItem.OrderId).Count();
                            deliveryItem.Revenue = deliveryItem.Revenue * deliveryItem.Quantity;
                            items.Add(deliveryItem);
                            tempProductId = deliveryItem.ProductId;
                            tempProductorderId = deliveryItem.OrderId.ToString();
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

        public class RefundViewModel
        {
            public int TotalDelivery { get; set; }
            public int TotalInStore { get; set; }
            public int TotalDigital { get; set; }
            public List<PendingHandlingItemViewModel> PendingHandlingItems { get; set; }
        }

        public class RefundsViewModel
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
