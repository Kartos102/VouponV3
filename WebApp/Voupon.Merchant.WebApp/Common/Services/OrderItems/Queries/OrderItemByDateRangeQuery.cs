using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class OrderItemByDateRangeQuery : IRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public int MerchantId { get; set; }

        public class OrderItemByDateRangeQueryHandler : IRequestHandler<OrderItemByDateRangeQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public OrderItemByDateRangeQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(OrderItemByDateRangeQuery request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var endDate = request.To.AddDays(1);

                    var orderDigital = await rewardsDBContext.DigitalRedemptionTokens
                    .AsNoTracking()
                    .Include(x => x.OrderItem)
                    .ThenInclude(x => x.Order)
                    .Where(x => x.OrderItem.Order.CreatedAt >= request.From &&
                    x.OrderItem.Order.CreatedAt < endDate &&
                    x.MerchantId == request.MerchantId).OrderByDescending(x => x.OrderItem.Order.CreatedAt).ToListAsync();

                        var groupedByOrderData = orderDigital.Where(x=>x.Token != null && x.Token != "").GroupBy(x => x.OrderItem.OrderId);
                        var OrdersList = new List<OrderItemViewModel>();
                        foreach (var orderData in groupedByOrderData)
                        {

                            var model = ConvertToOrderItemView(orderData);

                            OrdersList.Add(model);
                        }


                    var orderDelivery = await rewardsDBContext.DeliveryRedemptionTokens
                            .AsNoTracking()
                            .Include(x => x.OrderItem)
                            .ThenInclude(x => x.Order)
                            .Where(x => x.OrderItem.Order.CreatedAt >= request.From &&
                            x.OrderItem.Order.CreatedAt < endDate &&
                            x.MerchantId == request.MerchantId &&
                            (x.OrderItem.Status == (byte)OrderStatus.Completed)).OrderByDescending(x => x.OrderItem.Order.CreatedAt).ToListAsync();
                    var groupedByOrder = orderDelivery.GroupBy(x => x.OrderItem.OrderId);
                    foreach (var orderData in groupedByOrder)
                    {
                        var model = ConvertToOrderItemView(orderData);
                        OrdersList.Add(model);
                    }
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = OrdersList.OrderByDescending(x=>x.CreatedAt);

                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;
                }
                return apiResponseViewModel;
            }
        }

        public static OrderItemViewModel ConvertToOrderItemView(IGrouping<Guid, Voupon.Database.Postgres.RewardsEntities.DigitalRedemptionTokens> orderData)
        {
            var model = new OrderItemViewModel()
            {
                Id = orderData.Key,
                MasterMemberProfileId = orderData.FirstOrDefault().OrderItem.Order.MasterMemberProfileId,
                Email = orderData.FirstOrDefault().OrderItem.Order.Email,
                Revenue = orderData.ToList().Sum(x => x.OrderItem.Price),
                OrderStatus = (OrderStatus)orderData.FirstOrDefault().OrderItem.Order.OrderStatus,
                OrderItemStatus = (OrderStatus)orderData.FirstOrDefault().OrderItem.Status,
                Points = orderData.FirstOrDefault().OrderItem.Points,
                CreatedAt = orderData.FirstOrDefault().OrderItem.Order.CreatedAt,
                ShortId = orderData.FirstOrDefault().OrderItem.ShortId,
                ProductTitle = orderData.FirstOrDefault().ProductTitle,
                ProductImageUrl = orderData.FirstOrDefault().OrderItem.ProductImageFolderUrl.Replace("http://", "https://"),
                CreatedAtString = orderData.FirstOrDefault().OrderItem.Order.CreatedAt.ToString("dd/MM/yyyy"),
                PromoCodeDiscount = orderData.FirstOrDefault().OrderItem.Order.TotalPromoCodeDiscount,
                Items = new List<ItemViewModel>()
            };
            foreach (var item in orderData)
            {
                ItemViewModel itemViewModel = new ItemViewModel();
                itemViewModel.Id = item.ProductId;
                itemViewModel.Email = item.Email;
                itemViewModel.OrderId = item.OrderItem.Id;
                itemViewModel.Quantity = 1;
                itemViewModel.OrderItemId = item.OrderItemId;
                itemViewModel.ProductTitle = item.ProductTitle;
                itemViewModel.ShippingName = item.OrderItem.Order.ShippingPersonFirstName + " " + item.OrderItem.Order.ShippingPersonLastName;
                itemViewModel.ShippingAddress = item.OrderItem.Order.ShippingAddressLine1 + " " + item.OrderItem.Order.ShippingAddressLine2;
                itemViewModel.Revenue = item.OrderItem.Order.OrderItems.Where(x => x.ProductId == item.ProductId && x.OrderId == item.OrderItem.OrderId).Select(x => x.Price).First();
                itemViewModel.ShortOrderId = item.OrderItem.ShortId;
                itemViewModel.Variant = item.OrderItem.VariationText;
                itemViewModel.OrderItemStatus = (OrderStatus)item.OrderItem.Status;
                itemViewModel.Type = 1;
                model.Items.Add(itemViewModel);
            }
            return model;
        }

        public static OrderItemViewModel ConvertToOrderItemView(IGrouping<Guid, Voupon.Database.Postgres.RewardsEntities.DeliveryRedemptionTokens> orderData)
        {
            var model = new OrderItemViewModel()
            {
                Id = orderData.Key,
                MasterMemberProfileId = orderData.FirstOrDefault().OrderItem.Order.MasterMemberProfileId,
                Email = orderData.FirstOrDefault().OrderItem.Order.Email,
                Revenue = orderData.ToList().Sum(x => x.OrderItem.Price),
                OrderStatus = (OrderStatus)orderData.FirstOrDefault().OrderItem.Order.OrderStatus,
                OrderItemStatus = (OrderStatus)orderData.FirstOrDefault().OrderItem.Status,
                Points = orderData.FirstOrDefault().OrderItem.Points,
                CreatedAt = orderData.FirstOrDefault().OrderItem.Order.CreatedAt,
                ShortId = orderData.FirstOrDefault().OrderItem.ShortId,
                ProductTitle = orderData.FirstOrDefault().ProductTitle,
                ProductImageUrl = orderData.FirstOrDefault().OrderItem.ProductImageFolderUrl.Replace("http://", "https://"),
                CreatedAtString = orderData.FirstOrDefault().OrderItem.Order.CreatedAt.ToString("dd/MM/yyyy"),
                PromoCodeDiscount = orderData.FirstOrDefault().OrderItem.Order.TotalPromoCodeDiscount,
                Items = new List<ItemViewModel>()
            };
            foreach (var item in orderData)
            {
                ItemViewModel itemViewModel = new ItemViewModel();
                itemViewModel.Id = item.ProductId;
                itemViewModel.Email = item.Email;
                itemViewModel.OrderId = item.OrderItem.Id;
                itemViewModel.Quantity = 1;
                itemViewModel.OrderItemId = item.OrderItemId;
                itemViewModel.ProductTitle = item.ProductTitle;
                itemViewModel.ShippingName = item.OrderItem.Order.ShippingPersonFirstName + " " + item.OrderItem.Order.ShippingPersonLastName;
                itemViewModel.ShippingAddress = item.OrderItem.Order.ShippingAddressLine1 + " " + item.OrderItem.Order.ShippingAddressLine2;
                itemViewModel.Revenue = item.OrderItem.Order.OrderItems.Where(x => x.ProductId == item.ProductId && x.OrderId == item.OrderItem.OrderId).Select(x => x.Price).First();
                itemViewModel.ShortOrderId = item.OrderItem.ShortId;
                itemViewModel.Variant = item.OrderItem.VariationText;
                itemViewModel.OrderItemStatus = (OrderStatus)item.OrderItem.Status;
                itemViewModel.Type = 1;
                model.Items.Add(itemViewModel);
            }
            return model;
        }







        public class OrderItemViewModel
        {
            public Guid Id { get; set; }
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public decimal Price { get; set; }

            public decimal Revenue { get; set; }
            public int Points { get; set; }
            public OrderStatus OrderStatus { get; set; }
            public OrderStatus OrderItemStatus { get; set; }
            public DateTime CreatedAt { get; set; }

            public string ShortId { get; set; }

            public string ProductTitle { get; set; }
            public string ProductImageUrl { get; set; }

            public string CreatedAtString { get; set; }

            public decimal PromoCodeDiscount { get; set; }
            public List<ItemViewModel> Items { get; set; }

        }

        public class ItemViewModel
        {
            public int Id { get; set; }

            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public Guid OrderItemId { get; set; }
            public OrderStatus OrderItemStatus { get; set; }
            public string ProductTitle { get; set; }
            public string Token { get; set; }
            public bool IsRedeemed { get; set; }
            public bool IsActivated { get; set; }
            public DateTime CreatedAt { get; set; }
            //public DateTime LastUpdatedAt { get; set; }
            public Guid OrderId { get; set; }
            //public string LastUpdatedBy { get; set; }
            public DateTime? RedeemedAt { get; set; }
            public decimal Revenue { get; set; }
            public string Email { get; set; }
            public byte Status { get; set; }
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
