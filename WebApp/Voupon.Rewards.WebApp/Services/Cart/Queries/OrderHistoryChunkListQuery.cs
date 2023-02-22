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
    public class OrderHistoryChunkListQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int PageNumber{ get; set; }
    }
    public class OrderHistoryChunkListQueryHandler : IRequestHandler<OrderHistoryChunkListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        IAzureBlobStorage azureBlobStorage;
        public OrderHistoryChunkListQueryHandler(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(OrderHistoryChunkListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Orders.Include(x => x.OrderItems).Where(x => x.MasterMemberProfileId == request.Id && x.OrderStatus == 2).OrderByDescending(x => x.CreatedAt).AsNoTracking().ToListAsync();

                List<OrderHistoryModel> list = new List<OrderHistoryModel>();

                for (int i = 0; i < items.Count(); i++)
                {
                    if(!(i < (request.PageNumber * 10) && ((request.PageNumber * 10) - i) <= 10))
                    {
                        continue;
                    }
                    OrderHistoryModel newItem = new OrderHistoryModel();
                    newItem.Id = items[i].Id;
                    newItem.ShortId = items[i].ShortId;
                    newItem.MasterMemberProfileId = items[i].MasterMemberProfileId;
                    newItem.Email = items[i].Email;
                    newItem.TotalPrice = items[i].TotalPrice;
                    newItem.TotalPoints = items[i].TotalPoints;
                    newItem.TotalItems = items[i].TotalItems;
                    newItem.OrderStatus = (byte)items[i].OrderStatus;
                    newItem.CreatedAt = items[i].CreatedAt;
                    newItem.BillingPersonFirstName = items[i].BillingPersonFirstName;
                    newItem.BillingPersonLastName = items[i].BillingPersonLastName;
                    newItem.BillingEmail = items[i].BillingEmail;
                    newItem.BillingMobileNumber = items[i].BillingMobileNumber;
                    newItem.BillingMobileCountryCode = items[i].BillingMobileCountryCode;
                    newItem.BillingAddressLine1 = items[i].BillingAddressLine1;
                    newItem.BillingAddressLine2 = items[i].BillingAddressLine2;
                    newItem.BillingPostcode = items[i].BillingPostcode;
                    newItem.BillingCity = items[i].BillingCity;
                    newItem.BillingState = items[i].BillingState;
                    newItem.BillingCountry = items[i].BillingCountry;
                    newItem.ShippingPersonFirstName = items[i].ShippingPersonFirstName;
                    newItem.ShippingPersonLastName = items[i].ShippingPersonLastName;
                    newItem.ShippingEmail = items[i].ShippingEmail;
                    newItem.ShippingMobileNumber = items[i].ShippingMobileNumber;
                    newItem.ShippingMobileCountryCode = items[i].ShippingMobileCountryCode;
                    newItem.ShippingAddressLine1 = items[i].ShippingAddressLine1;
                    newItem.ShippingAddressLine2 = items[i].ShippingAddressLine2;
                    newItem.ShippingPostcode = items[i].ShippingPostcode;
                    newItem.ShippingCity = items[i].ShippingCity;
                    newItem.ShippingState = items[i].ShippingState;
                    newItem.CreatedAt = items[i].CreatedAt;
                    newItem.ShippingCountry = items[i].ShippingCountry;
                    newItem.OrderItems = new List<OrderItems>();
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
                        orderItem.Product = Newtonsoft.Json.JsonConvert.DeserializeObject<Commands.ProductList>(orderItemDb.ProductDetail);
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
                            var redemption = await rewardsDBContext.InStoreRedemptionTokens.AsNoTracking().FirstOrDefaultAsync(x => x.OrderItemId == orderItem.Id
                            );
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

                        newItem.OrderItems.Add(orderItem);
                    }

                    list.Add(newItem);
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