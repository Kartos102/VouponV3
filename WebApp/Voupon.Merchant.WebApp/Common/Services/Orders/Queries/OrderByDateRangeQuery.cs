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
    public class OrderByDateRangeQuery : IRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public class OrderByDateRangeQueryHandler : IRequestHandler<OrderByDateRangeQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public OrderByDateRangeQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public string GetItemStatus(byte status)
            {
                return "Done";
            }


            public async Task<ApiResponseViewModel> Handle(OrderByDateRangeQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var list = new List<OrderViewModel>();

                    var promoCodes = await rewardsDBContext.PromoCodes.ToDictionaryAsync(x => x.Id);
                    PromoCodes promo = new PromoCodes();
                    var digitalPendingHandling = await rewardsDBContext.DigitalRedemptionTokens.
                        AsNoTracking().
                        Include(x => x.OrderItem).
                        ThenInclude(x => x.Order).
                        Include(x => x.Product).
                        Where(x => x.OrderItem.Order.CreatedAt >= request.From && x.OrderItem.Order.CreatedAt <= request.To.AddDays(1) && (x.OrderItem.Order.OrderStatus > 1)).Select(x => new OrderViewModel
                        {
                            OrderId = x.OrderItem.OrderId,
                            CreatedAtString = x.OrderItem.Order.CreatedAt.ToString("dd/MM/yyyy"),
                            CreatedAt = x.CreatedAt,
                            OrderStatus = x.OrderItem.Status,
                            Email = x.OrderItem.Order.Email,
                            ShortId = x.OrderItem.Order.ShortId,
                            MerchantName = x.OrderItem.MerchantDisplayName,
                            ProductTitle = x.OrderItem.ProductTitle + (string.IsNullOrEmpty(x.OrderItem.VariationText) ? "" : "(" + x.OrderItem.VariationText + ")"),
                            ProductVariation = x.OrderItem.VariationText,
                            TotalItems = 1,
                            OrderItemStatus = x.OrderItem.Status,
                            TotalPoints = x.OrderItem.Points,
                            TotalPrice = x.OrderItem.Price,
                            ItemShortId = x.OrderItem.ShortId,
                            Name = (x.OrderItem.Order.BillingPersonFirstName + " " + x.OrderItem.Order.BillingPersonLastName),
                            Address = (x.OrderItem.Order.ShippingAddressLine1 + " " + x.OrderItem.Order.ShippingAddressLine2 + " " + x.OrderItem.Order.ShippingPostcode + " " + x.OrderItem.Order.ShippingCity + " " + x.OrderItem.Order.ShippingCountry),
                            MobileNumber = (x.OrderItem.Order.BillingMobileCountryCode + x.OrderItem.Order.BillingMobileNumber)



                        }).ToListAsync();

                    if (digitalPendingHandling != null && digitalPendingHandling.Any())
                    {
                        list.AddRange(digitalPendingHandling);
                    }

                    var deliveryPendingHandling = await rewardsDBContext.DeliveryRedemptionTokens.
                        AsNoTracking().
                        Include(x => x.OrderItem).
                        ThenInclude(x => x.Order).
                        Where(x => x.OrderItem.Order.CreatedAt >= request.From && x.OrderItem.Order.CreatedAt <= request.To.AddDays(1) && (x.OrderItem.Order.OrderStatus > 1)).Select(x => new OrderViewModel
                        {
                            OrderId = x.OrderItem.OrderId,
                            CreatedAtString = x.OrderItem.Order.CreatedAt.ToString("dd/MM/yyyy"),
                            CreatedAt = x.CreatedAt,
                            OrderStatus = x.OrderItem.Status,
                            Email = x.OrderItem.Order.Email,
                            ShortId = x.OrderItem.Order.ShortId,
                            MerchantName = x.OrderItem.MerchantDisplayName,
                            ProductTitle = x.OrderItem.ProductTitle + (string.IsNullOrEmpty(x.OrderItem.VariationText) ? "" : "(" + x.OrderItem.VariationText + ")"),
                            ProductVariation = x.OrderItem.VariationText,
                            TotalItems = 1,
                            OrderItemStatus = x.OrderItem.Status,
                            TotalPoints = x.OrderItem.Points,
                            TotalPrice = x.OrderItem.Price,
                            ProductId = x.OrderItem.ProductId,
                            ItemShortId = x.OrderItem.ShortId,
                            Name = (x.OrderItem.Order.BillingPersonFirstName + " " + x.OrderItem.Order.BillingPersonLastName),
                            Address = (x.OrderItem.Order.ShippingAddressLine1 + " " + x.OrderItem.Order.ShippingAddressLine2 + " " + x.OrderItem.Order.ShippingPostcode + " " + x.OrderItem.Order.ShippingCity + " " + x.OrderItem.Order.ShippingCountry),
                            MobileNumber = (x.OrderItem.Order.BillingMobileCountryCode + x.OrderItem.Order.BillingMobileNumber)
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
                            deliveryItem.TotalItems = deliveryPendingHandling.Where(x => x.ProductId == deliveryItem.ProductId && x.OrderId == deliveryItem.OrderId).Count();
                            deliveryItem.TotalPrice = deliveryItem.TotalPrice * deliveryItem.TotalItems;
                            list.Add(deliveryItem);
                            tempProductId = deliveryItem.ProductId;
                            orderId = deliveryItem.OrderId;
                        }
                    }

                    apiResponseViewModel.Data = list.OrderByDescending(x=>x.CreatedAt).ThenBy(x=>x.OrderId);
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

       
        public class OrderViewModel
        {
            public string StatusString { get; set; }
            public string ItemShortId { get; set; }
            public int ProductId { get; set;}
            public string MerchantName { get; set; }
            public Guid OrderId { get; set; }
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public decimal TotalPrice { get; set; }
            public int TotalPoints { get; set; }
            public int TotalItems { get; set; }
            public short OrderStatus { get; set; }
            public DateTime CreatedAt { get; set; }
            public int OrderItemStatus { get; set; }
            public string ShortId { get; set; }
            public string ProductTitle { get; set; }
            public string ProductVariation { get; set; }
            public string CreatedAtString { get; set; }

            public string Name { get; set; }

            public string Address { get; set; }

            public string MobileNumber { get; set; }

            public decimal ShippingCost { get; set; }

            public string PromoCode { get; set; }

            public decimal PromoCodeDiscount { get; set; }


        }


    }

}
         