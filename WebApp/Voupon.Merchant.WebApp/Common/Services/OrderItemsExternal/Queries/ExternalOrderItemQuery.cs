using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.OrderItemsExternal.Queries
{
    public class ExternalOrderItemQuery : IRequest<ApiResponseViewModel>
    {
        public short WebsiteTypeId { get; set; }
        public bool IsConmpleted { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public class ExternalOrderItemQueryHandler : IRequestHandler<ExternalOrderItemQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ExternalOrderItemQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ExternalOrderItemQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var OrderShopModelTtems = new List<OrderShopExternalViewModel>();

                    var externalOrders = new List<OrderShopExternal>();
                    if (request.IsConmpleted)
                    {
                        externalOrders = await rewardsDBContext.OrderShopExternal.Include(x => x.Order.OrderPayments).Include(x => x.OrderItemExternal).AsNoTracking().Where(x => x.Order.CreatedAt.Date >= request.From && x.Order.CreatedAt.Date <= request.To && (x.OrderItemExternal.All(y => y.OrderItemExternalStatus != 1) && x.OrderItemExternal.All(y => y.OrderItemExternalStatus != 2) && x.OrderItemExternal.All(y => y.OrderItemExternalStatus != 3))).OrderByDescending(x => x.Order.CreatedAt).ToListAsync();
                    }
                    else
                    {
                        externalOrders = await rewardsDBContext.OrderShopExternal.Include(x => x.Order.OrderPayments).Include(x => x.OrderItemExternal).AsNoTracking().Where(x => x.Order.CreatedAt.Date >= request.From && x.Order.CreatedAt.Date <= request.To && (x.OrderItemExternal.Any(y => y.OrderItemExternalStatus == 1) || x.OrderItemExternal.Any(y => y.OrderItemExternalStatus == 2) || x.OrderItemExternal.Any(y => y.OrderItemExternalStatus == 3))).OrderByDescending(x => x.Order.CreatedAt).ToListAsync();
                    }


                    if (request.WebsiteTypeId != 0)
                        externalOrders = externalOrders.Where(x => x.ExternalTypeId == request.WebsiteTypeId).ToList();

                    var promoCodes = await rewardsDBContext.PromoCodes.ToListAsync();
                    foreach (var OrderShopExternal in externalOrders)
                    {
                        if (!OrderShopExternal.Order.OrderPayments.Any())
                        {
                            continue;
                        }

                        if (!OrderShopExternal.Order.OrderPayments.Where(x => x.Status == "SUCCESS" || x.Status == "1").Any())
                        {
                            continue;
                        }

                        var shopModel = new OrderShopExternalViewModel()
                        {
                            OrderShortId = OrderShopExternal.Order.ShortId,
                            CreatedAt = OrderShopExternal.Order.CreatedAt,
                            Email = OrderShopExternal.Order.Email,
                            OrderId = OrderShopExternal.Order.Id,
                            ExternalOrderId = OrderShopExternal.ExternalOrderId,
                            Id = OrderShopExternal.Id,
                            ExternalShopUrl = OrderShopExternal.ExternalShopUrl,
                            ShippingCost = OrderShopExternal.ShippingCost,
                            ShippingHtml = OrderShopExternal.ShippingDetailsJson,
                            Status = OrderShopExternal.OrderShippingExternalStatus,
                            OrderStatus = OrderShopExternal.Order.OrderStatus,
                            TrackingNo = OrderShopExternal.TrackingNo,
                            LastUpdatedAt = OrderShopExternal.LastUpdatedAt,
                            LastUpdatedByUserId = OrderShopExternal.LastUpdatedByUser,
                            ExternalShopName = OrderShopExternal.ExternalShopName,
                            ExternalTypeId = OrderShopExternal.ExternalTypeId,
                            OrderItemViewModels = new List<ExternalOrderItemViewModel>()
                        };
                        if (promoCodes.Count > 0 && promoCodes != null && OrderShopExternal.Order.PromoCodeId != null)
                        {
                            shopModel.PromoCodeName = promoCodes.Where(x => x.Id == OrderShopExternal.Order.PromoCodeId).FirstOrDefault().Description;
                            shopModel.PromoCodeDiscount = OrderShopExternal.Order.PromoCodeMaxDiscountValue;
                        }

                        var modelTtems = new List<ExternalOrderItemViewModel>();
                        Regex regex = new Regex("[*'\",_%&#^@]");

                        foreach (var item in OrderShopExternal.OrderItemExternal)
                        {
                            var model = new ExternalOrderItemViewModel()
                            {
                                Id = item.Id,
                                ExternalUrl = regex.Replace(item.ExternalUrl, string.Empty),
                                ProductTitle = item.ProductTitle,
                                Status = item.OrderItemExternalStatus,
                                Revenue = item.TotalPrice,
                                LastUpdatedAt = item.LastUpdatedAt,
                                LastUpdatedByUserId = item.LastUpdatedByUser,
                                Quantity = item.Quantity,
                                Variant = item.VariationText,
                                ShortOrderId = OrderShopExternal.Order.ShortId,
                                VpointsDiscount = item.DiscountedAmount
                            };
                            modelTtems.Add(model);
                        }

                        shopModel.OrderItemViewModels.AddRange(modelTtems);
                        shopModel.Revenue = shopModel.OrderItemViewModels.Sum(x => x.Revenue);
                        shopModel.VpointsDiscount = shopModel.OrderItemViewModels.Sum(x => x.VpointsDiscount);

                        var orderPayments = rewardsDBContext.OrderPayments.Where(x => x.RefNo == shopModel.OrderId).FirstOrDefault();
                        if (orderPayments != null)
                        {
                            shopModel.Paid = orderPayments.Amount;
                        }

                        shopModel.OverAllItemsStatus = shopModel.OrderItemViewModels.Min(x => x.Status);
                        if (shopModel.ShippingHtml != null && shopModel.ShippingHtml != "Loading...")
                        {
                            var jsonStr = shopModel.ShippingHtml.Substring(1, shopModel.ShippingHtml.Length - 2);

                            var jsonShippingDetails = JsonConvert.DeserializeObject<ShippingJsonObj>(jsonStr);
                            if (jsonShippingDetails.address != null)
                            {
                                shopModel.ShippingAddress = jsonShippingDetails.address.shippingAddress;
                                shopModel.ShippingName = jsonShippingDetails.address.shippingName;
                            }
                            else
                            {
                                shopModel.ShippingAddress = OrderShopExternal.Order.ShippingAddressLine1;
                                shopModel.ShippingName = OrderShopExternal.Order.ShippingPersonFirstName + " " + OrderShopExternal.Order.ShippingPersonLastName;
                            }
                        }
                        else
                        {
                            shopModel.ShippingAddress = OrderShopExternal.Order.ShippingAddressLine1;
                            shopModel.ShippingName = OrderShopExternal.Order.ShippingPersonFirstName + " " + OrderShopExternal.Order.ShippingPersonLastName;
                        }
                        OrderShopModelTtems.Add(shopModel);

                    }
                    apiResponseViewModel.Data = OrderShopModelTtems;
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

        public class ShippingJsonObj
        {
            public int numberOfParcel { get; set; }
            public CourierModel courier { get; set; }
            public AddressModel address { get; set; }
            public statuses statuses { get; set; }
            public List<JsonTrackingList> trackingList { get; set; }
            public string trackingNumber { get; set; }

        }
        public class CourierModel
        {
            public string text { get; set; }

        }

        public class AddressModel
        {
            public string shippingAddress { get; set; }
            public string shippingName { get; set; }
            public string shippingPhone { get; set; }

        }

        public class JsonTrackingList
        {
            public double createdAt { get; set; }
            public string description { get; set; }

        }
        public class statuses
        {
            public double paidAt { get; set; }
            public double paidAmount { get; set; }
            public bool isRated { get; set; }
            public string orderSerialNumber { get; set; }
            public double orderPlacedAt { get; set; }
            public double orderShipppedOutAt { get; set; }
            public double orderReceivedAt { get; set; }
            public double completedAt { get; set; }
        }

        public class ExternalOrdersModel
        {
            public string PhoneNumber { get; set; }
            public string Address { get; set; }
            public string OrderShortId { get; set; }
            public Guid OrderId { get; set; }
            public string Email { get; set; }
            public decimal Revenue { get; set; }
            public decimal Paid { get; set; }
            public short Status { get; set; }
            public string LastUpdatedByUserId { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? LastUpdatedAt { get; set; }

            public decimal? PromoCodeDiscount { get; set; }
            public decimal ShippingCost { get; set; }
            public decimal VpointsDiscount { get; set; }
            public string PromoCodeName { get; set; }


            public List<OrderShopExternalViewModel> orderShopExternalViewModel { get; set; }
        }
        public class OrderShopExternalViewModel
        {
            public string OrderShortId { get; set; }
            public Guid Id { get; set; }
            public Guid OrderId { get; set; }
            public string Email { get; set; }
            public string ExternalShopUrl { get; set; }
            public string ExternalShopName { get; set; }
            public string ExternalOrderId { get; set; }
            public int ExternalTypeId { get; set; }

            public DateTime CreatedAt { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public string LastUpdatedByUserId { get; set; }
            public string TrackingNo { get; set; }
            public string ShippingHtml { get; set; }
            public string ShippingName { get; set; }
            public string ShippingAddress { get; set; }
            public decimal ShippingCost { get; set; }
            public short Status { get; set; }
            public short OrderStatus { get; set; }
            public short OverAllItemsStatus { get; set; }
            public decimal Revenue { get; set; }
            public decimal Paid { get; set; }
            public decimal? PromoCodeDiscount { get; set; }
            public string PromoCodeName { get; set; }
            public decimal VpointsDiscount { get; set; }

            public List<ExternalOrderItemViewModel> OrderItemViewModels { get; set; }

        }

        public class ExternalOrderItemViewModel
        {
            public Guid Id { get; set; }
            public string ExternalUrl { get; set; }
            public string ProductTitle { get; set; }
            public short Status { get; set; }
            public decimal Revenue { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public string LastUpdatedByUserId { get; set; }
            public string ShortOrderId { get; set; }
            public string Variant { get; set; }
            public int Quantity { get; set; }
            public decimal VpointsDiscount { get; set; }

        }
    }
}
