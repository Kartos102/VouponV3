using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.Services.Cart.Models;
using Voupon.Rewards.WebApp.ViewModels;
using OrderItems = Voupon.Rewards.WebApp.Services.Cart.Models.OrderItems;

namespace Voupon.Rewards.WebApp.Services.Cart.Queries
{
    public class PendingPaymentListQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }
    public class PendingPaymentListQueryHandler : IRequestHandler<PendingPaymentListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public PendingPaymentListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(PendingPaymentListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Orders.AsNoTracking().Include(x => x.OrderItems).Include(x => x.OrderShopExternal).ThenInclude(x => x.OrderItemExternal).Where(x => x.MasterMemberProfileId == request.Id && x.OrderStatus == 1).OrderByDescending(x => x.CreatedAt).ToListAsync();

                List<OrderHistoryModel> list = new List<OrderHistoryModel>();
                var currentSortId = 0;
                foreach (var item in items)
                {
                    currentSortId++;
                    OrderHistoryModel newItem = new OrderHistoryModel();
                    newItem.SortId = currentSortId;
                    newItem.Id = item.Id;
                    newItem.ShortId = item.ShortId;
                    newItem.MasterMemberProfileId = item.MasterMemberProfileId;
                    newItem.Email = item.Email;
                    newItem.TotalPrice = item.TotalPrice;
                    newItem.TotalPoints = item.TotalPoints;
                    newItem.TotalItems = item.TotalItems;
                    newItem.OrderStatus = (byte)item.OrderStatus;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.BillingPersonFirstName = item.BillingPersonFirstName;
                    newItem.BillingPersonLastName = item.BillingPersonLastName;
                    newItem.BillingEmail = item.BillingEmail;
                    newItem.BillingMobileNumber = item.BillingMobileNumber;
                    newItem.BillingMobileCountryCode = item.BillingMobileCountryCode;
                    newItem.BillingAddressLine1 = item.BillingAddressLine1;
                    newItem.BillingAddressLine2 = item.BillingAddressLine2;
                    newItem.BillingPostcode = item.BillingPostcode;
                    newItem.BillingCity = item.BillingCity;
                    newItem.BillingState = item.BillingState;
                    newItem.BillingCountry = item.BillingCountry;
                    newItem.ShippingPersonFirstName = item.ShippingPersonFirstName;
                    newItem.ShippingPersonLastName = item.ShippingPersonLastName;
                    newItem.ShippingEmail = item.ShippingEmail;
                    newItem.ShippingMobileNumber = item.ShippingMobileNumber;
                    newItem.ShippingMobileCountryCode = item.ShippingMobileCountryCode;
                    newItem.ShippingAddressLine1 = item.ShippingAddressLine1;
                    newItem.ShippingAddressLine2 = item.ShippingAddressLine2;
                    newItem.ShippingPostcode = item.ShippingPostcode;
                    newItem.ShippingCity = item.ShippingCity;
                    newItem.ShippingState = item.ShippingState;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.ShippingCountry = item.ShippingCountry;
                    newItem.ShippingCost = item.TotalShippingCost;
                    newItem.OrderItems = new List<OrderItems>();
                    foreach (var orderItemDb in item.OrderItems)
                    {
                        OrderItems orderItem = new Models.OrderItems();
                        orderItem.Id = orderItemDb.Id;
                        orderItem.OrderId = orderItemDb.OrderId;
                        orderItem.ProductId = orderItemDb.ProductId;
                        orderItem.MerchantId = orderItemDb.MerchantId;
                        orderItem.MerchantDisplayName = orderItemDb.MerchantDisplayName;
                        orderItem.Commision = orderItemDb.Commision;
                        orderItem.Price = orderItemDb.Price;
                        orderItem.Points = orderItemDb.Points;
                        orderItem.ProductDetail = orderItemDb.ProductDetail;
                        orderItem.Product = Newtonsoft.Json.JsonConvert.DeserializeObject<Commands.ProductList>(orderItemDb.ProductDetail);
                        orderItem.ExpirationTypeId = orderItemDb.ExpirationTypeId;
                        orderItem.ProductTitle = orderItemDb.ProductTitle;
                        orderItem.IsVariationProduct = orderItemDb.IsVariationProduct;
                        orderItem.VariationText = orderItemDb.VariationText;
                        orderItem.ProductImageFolderUrl = orderItemDb.ProductImageFolderUrl;
                        orderItem.Status = orderItemDb.Status;
                        newItem.OrderItems.Add(orderItem);
                    }

                    if (item.OrderShopExternal != null && item.OrderShopExternal.Any())
                    {
                        var externalItemList = item.OrderShopExternal.SelectMany(x => x.OrderItemExternal).ToList();
                        foreach (var externalItem in externalItemList)
                        {
                            currentSortId++;
                            OrderItems orderItem = new Models.OrderItems();
                            newItem.SortId = currentSortId;
                            orderItem.Id = externalItem.Id;
                            orderItem.OrderId = externalItem.OrderShopExternal.OrderId;
                            orderItem.ProductId = 0;
                            orderItem.MerchantId = 0;
                            orderItem.MerchantDisplayName = externalItem.OrderShopExternal.ExternalShopName;
                            orderItem.Commision = 0;
                            orderItem.Price = externalItem.Price;
                            orderItem.Points = externalItem.Points;
                            orderItem.Product = new Commands.ProductList
                            {
                                Title = externalItem.ProductTitle,
                                Price = (double)externalItem.OriginalPrice,
                                DiscountedPrice = (double)externalItem.Price,
                                TotalPrice = externalItem.TotalPrice.ToString(),
                                AdditionalDiscount = new Commands.AdditionalDiscount
                                {
                                    PointsRequired = externalItem.Points
                                }

                            };
                            orderItem.ExpirationTypeId = externalItem.OrderShopExternal.OrderItemExternal.First().DealExpirationId;
                            orderItem.ProductTitle = externalItem.ProductTitle;
                            orderItem.IsVariationProduct = externalItem.IsVariationProduct;
                            orderItem.VariationText = externalItem.VariationText;
                            orderItem.ProductImageFolderUrl = externalItem.ProductCartPreviewSmallImageURL;
                            orderItem.Status = 1;
                            orderItem.ExternalItemId = externalItem.ExternalItemId;
                            orderItem.ExternalShopId = externalItem.OrderShopExternal.ExternalShopId;
                            orderItem.ExternalTypeId = externalItem.OrderShopExternal.ExternalTypeId;
                            newItem.OrderItems.Add(orderItem);
                        }
                    }

                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Pending Payment List Successfully";
                response.Data = list;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
