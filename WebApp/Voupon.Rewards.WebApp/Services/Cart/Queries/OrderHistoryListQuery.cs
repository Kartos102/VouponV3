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
    public class OrderHistoryListQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }
    public class OrderHistoryListQueryHandler : IRequestHandler<OrderHistoryListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        IAzureBlobStorage azureBlobStorage;
        public OrderHistoryListQueryHandler(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(OrderHistoryListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Orders.Include(x => x.OrderItems).Include(x => x.OrderShopExternal).ThenInclude(x => x.OrderItemExternal).Where(x => x.MasterMemberProfileId == request.Id && x.OrderStatus == 2).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

                List<OrderItems> list = new List<OrderItems>();
                for (int i = 0; i < items.Count(); i++)
                {
                    if (items[i].OrderItems != null && items[i].OrderItems.Count > 0)
                    {
                        foreach (var orderItemDb in items[i].OrderItems)
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
                            try
                            {
                                orderItem.Product = Newtonsoft.Json.JsonConvert.DeserializeObject<Commands.ProductList>(orderItemDb.ProductDetail);
                            }
                            catch(Exception ex)
                            {
                                //  Log
                            }
                            orderItem.ExpirationTypeId = orderItemDb.ExpirationTypeId;
                            orderItem.ProductTitle = orderItemDb.ProductTitle;
                            orderItem.ProductImageFolderUrl = orderItemDb.ProductImageFolderUrl;
                            orderItem.Status = orderItemDb.Status;
                            orderItem.IsVariationProduct = orderItemDb.IsVariationProduct;
                            orderItem.VariationId = orderItemDb.VariationId;
                            orderItem.VariationText = orderItemDb.VariationText;
                            orderItem.Status = orderItemDb.Status;
                            orderItem.IsRedeemed = false;
                            orderItem.IsReviewed = false;
                            orderItem.CreatedAt = items[i].CreatedAt;
                            orderItem.ShortId = items[i].ShortId;
                            orderItem.OrderStatus = items[i].OrderStatus;
                            if (orderItem.ExpirationTypeId == 1)
                            {
                                var redemption = await rewardsDBContext.InStoreRedemptionTokens.AsNoTracking().FirstOrDefaultAsync(x => x.OrderItemId == orderItem.Id);
                                if (redemption == null)
                                {
                                    orderItem.IsRedeemed = false;
                                }
                                else
                                {
                                    orderItem.IsRedeemed = redemption.IsRedeemed;
                                }
                                var review = await rewardsDBContext.ProductReview.AsNoTracking().FirstOrDefaultAsync(x => x.OrderItemId == orderItem.Id);
                                if (review == null)
                                {
                                    orderItem.IsReviewed = false;
                                }
                                else
                                {
                                    orderItem.IsReviewed = true;
                                }
                            }
                            else if (orderItem.ExpirationTypeId == 2)
                            {
                                var redemption = await rewardsDBContext.InStoreRedemptionTokens.AsNoTracking().FirstOrDefaultAsync(x => x.OrderItemId == orderItem.Id);
                                if (redemption == null)
                                {
                                    orderItem.IsRedeemed = false;
                                }
                                else
                                {
                                    orderItem.IsRedeemed = redemption.IsRedeemed;
                                }
                                var review = await rewardsDBContext.ProductReview.AsNoTracking().FirstOrDefaultAsync(x => x.OrderItemId == orderItem.Id);
                                if (review == null)
                                {
                                    orderItem.IsReviewed = false;
                                }
                                else
                                {
                                    orderItem.IsReviewed = true;
                                }
                            }
                            else if (orderItem.ExpirationTypeId == 4)
                            {
                                var redemption = await rewardsDBContext.DigitalRedemptionTokens.AsNoTracking().FirstOrDefaultAsync(x => x.OrderItemId == orderItem.Id);
                                if (redemption == null)
                                {
                                    orderItem.IsRedeemed = false;
                                }
                                else
                                {
                                    orderItem.IsRedeemed = redemption.IsRedeemed;
                                }
                                var review = await rewardsDBContext.ProductReview.AsNoTracking().FirstOrDefaultAsync(x => x.OrderItemId == orderItem.Id);
                                if (review == null)
                                {
                                    orderItem.IsReviewed = false;
                                }
                                else
                                {
                                    orderItem.IsReviewed = true;
                                }
                            }
                            else
                            {
                                var redemption = await rewardsDBContext.DeliveryRedemptionTokens.AsNoTracking().FirstOrDefaultAsync(x => x.OrderItemId == orderItem.Id);
                                if (redemption == null)
                                {
                                    orderItem.IsRedeemed = false;
                                }
                                else
                                {
                                    orderItem.IsRedeemed = redemption.IsRedeemed;
                                }
                                var review = await rewardsDBContext.ProductReview.AsNoTracking().FirstOrDefaultAsync(x => x.OrderItemId == orderItem.Id);
                                if (review == null)
                                {
                                    orderItem.IsReviewed = false;
                                }
                                else
                                {
                                    orderItem.IsReviewed = true;
                                }
                            }


                            var azureBlobResult = await azureBlobStorage.ListBlobsAsync(ContainerNameEnum.Products, orderItemDb.ProductId + "/" + FilePathEnum.Products_Images);
                            if (azureBlobResult != null && azureBlobResult.Any())
                            {
                                var fileList = new List<string>();

                                foreach (var file in azureBlobResult)
                                {
                                    if (file.StorageUri.PrimaryUri.OriginalString.Contains("big") || file.StorageUri.PrimaryUri.OriginalString.Contains("normal") || file.StorageUri.PrimaryUri.OriginalString.Contains("org"))
                                    {
                                        continue;
                                    }
                                    fileList.Add(file.StorageUri.PrimaryUri.OriginalString);

                                }
                                orderItem.ProductImageFolderUrl = fileList.FirstOrDefault();

                                if (orderItem.ProductImageFolderUrl == "" || orderItem.ProductImageFolderUrl == null)
                                {
                                    orderItem.ProductImageFolderUrl = azureBlobResult.FirstOrDefault().StorageUri.PrimaryUri.OriginalString;

                                }
                            }
                            list.Add(orderItem);
                        }
                    }
                    else if (items[i].OrderShopExternal != null && items[i].OrderShopExternal.Count > 0)
                    {
                        foreach (var orderItemShopExternal in items[i].OrderShopExternal)
                        {
                            if (orderItemShopExternal.OrderItemExternal != null && orderItemShopExternal.OrderItemExternal.Count > 0)
                            {
                                foreach (var orderExternalItemDb in orderItemShopExternal.OrderItemExternal)
                                {
                                    OrderItems orderItem = new Models.OrderItems();
                                    orderItem.Id = orderExternalItemDb.Id;
                                    orderItem.OrderId = items[i].Id;
                                    orderItem.ProductExternalId = orderExternalItemDb.ExternalItemId;
                                    orderItem.MerchantExternalId = orderItemShopExternal.ExternalShopId;
                                    orderItem.MerchantExternalTypeId = orderItemShopExternal.ExternalTypeId;
                                    orderItem.MerchantDisplayName = orderItemShopExternal.ExternalShopName;
                                    //orderItem.Commision = orderExternalItemDb.Commision;
                                    orderItem.Price = orderExternalItemDb.Price;
                                    orderItem.Points = orderExternalItemDb.Points;
                                    //orderItem.ProductDetail = orderExternalItemDb.ProductDetail;
                                    orderItem.Product = new Commands.ProductList();
                                    orderItem.Product.DiscountedPrice = orderExternalItemDb.DiscountPriceValue != null ? (double)orderExternalItemDb.DiscountPriceValue : (double)orderExternalItemDb.Price;
                                    orderItem.Product.Price = (double)orderExternalItemDb.OriginalPrice;
                                    orderItem.Product.ExternalShopId = orderItemShopExternal.Id.ToString();
                                    orderItem.Product.PointsRequired = orderExternalItemDb.Points;
                                    if (orderItem.Product.Price != orderItem.Product.DiscountedPrice)
                                    {
                                        orderItem.Product.DiscountRate = (long)((orderExternalItemDb.OriginalPrice - (decimal)orderItem.Product.DiscountedPrice) / orderExternalItemDb.OriginalPrice * 100);
                                    }
                                    orderItem.Product.OrderQuantity = (long)orderExternalItemDb.Quantity;
                                    //orderItem.ExpirationTypeId = orderExternalItemDb.ExpirationTypeId;
                                    orderItem.ProductTitle = orderExternalItemDb.ProductTitle;
                                    orderItem.ProductImageFolderUrl = orderExternalItemDb.ProductCartPreviewSmallImageURL;
                                    orderItem.Status = orderExternalItemDb.OrderItemExternalStatus;
                                    orderItem.IsVariationProduct = orderExternalItemDb.IsVariationProduct;
                                    orderItem.ExpirationTypeId = orderExternalItemDb.DealExpirationId;
                                    //orderItem.VariationId = orderExternalItemDb.VariationId;
                                    orderItem.VariationText = orderExternalItemDb.VariationText;
                                    orderItem.IsRedeemed = false;
                                    orderItem.IsReviewed = false;
                                    orderItem.CreatedAt = items[i].CreatedAt;
                                    orderItem.ShortId = items[i].ShortId;
                                    orderItem.OrderStatus = items[i].OrderStatus;


                                    list.Add(orderItem);
                                }
                            }
                        }
                    }
                }
                response.Successful = true;
                response.Message = "Get Order History List Successfully";
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