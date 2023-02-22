using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Orders.Queries
{
    public class ItemRedemptionDetail
    {
        public int RedemptionType { get; set; }
        public string ProductTitle { get; set; }
        public string ProductImageUrl { get; set; }
        public string Token { get; set; }
        public string TokenImageSrc { get; set; }
        public string CreatedAt { get; set; }
        public string OrderedAt { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? Price { get; set; }
        public decimal? quantity { get; set; }
       
        public string ValidFrom { get; set; }
        public string ValidEnded { get; set; }
        public string RedeemedAt { get; set; }
        public bool IsRedeemed { get; set; }
        public string Courier { get; set; }



        public string ExpectedDeliveryDate { get; set; }

        public string DeliveryAddress { get; set; }
        public string DeliveryuserName { get; set; }

        public short TokenType { get; set; }
        public string Url { get; set; }

        public MerchantDetails MerchantInfo { get; set; }

        //For 1749

        public string ShortId { get; set; }
        public int? ProductId { get; set; }
        public int? Status { get; set; }

        public int? Points { get; set; }
        public decimal? DiscountedAmount { get; set; }
        public decimal? SubtotalPrice { get; set; }
        public int? ProductCategory { get; set; }

        public string OrderId { get; set; }
        public string OrderItemId { get; set; }
        public int OrderTypeId { get; set; }
        public int LatestStatusId { get; set; }
        public string LatestStatus { get; set; }
        public string LatestUpdatedAt { get; set; }
        public List<TrackingDetailsModel> TrackingList { get; set; }
    }

    public class MerchantDetails
    {
        public string Name { get; set; }
        public int? Id { get; set; }

        public string ImageUrl { get; set; }
        public string Email { get; set; }
    }

    public class ShippingInternalProdJsonObj
    {
        public string OrderId { get; set; }
        public string OrderItemId { get; set; }
        public int OrderTypeId { get; set; }
        public int LatestStatusId { get; set; }
        public string LatestStatus { get; set; }
        public string LatestUpdatedAt { get; set; }
        public List<ShippingInfoJsonObj> Shipping { get; set; }

    }

    public class ShippingInfoJsonObj
    {
        public string Date { get; set; }
        public string status { get; set; }
        public int StatusId { get; set; }
        public string Content { get; set; }
        public string Location { get; set; }
    }

    public class TrackingDetailsModel
    {
        public DateTime Date { get; set; }
        public string status { get; set; }
        public int StatusId { get; set; }
        public string Content { get; set; }
        public string Location { get; set; }
    }

    public class OrderItemRedemptionTokenQuery : IRequest<ApiResponseViewModel>
    {
        public Guid OrderItemId { get; set; }
    }
    public class ProductQueryOrderItemRedemptionTokenQueryHandler : IRequestHandler<OrderItemRedemptionTokenQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public ProductQueryOrderItemRedemptionTokenQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(OrderItemRedemptionTokenQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.OrderItems.FirstOrDefaultAsync(x => x.Id == request.OrderItemId);
                if (item != null)
                {
                    WebApp.Services.Cart.Commands.ProductList product = Newtonsoft.Json.JsonConvert.DeserializeObject<WebApp.Services.Cart.Commands.ProductList>(item.ProductDetail);
                    ItemRedemptionDetail detail = new ItemRedemptionDetail();
                    detail.ProductTitle = item.ProductTitle;
                    detail.Price = item.Price;
                    detail.ProductImageUrl = item.ProductImageFolderUrl;
                    detail.ProductCategory = Convert.ToInt32(product.ProductCategory); 
                    detail.RedemptionType = product.DealExpiration.Type;
                    detail.ProductId = Convert.ToInt32(product.ProductId);
                  



                    //Missing fields for 1749 



                    switch (detail.RedemptionType)
                    {
                        case 1:
                        case 2:
                            //Days, Date
                            var redemption = await rewardsDBContext.InStoreRedemptionTokens.FirstOrDefaultAsync(x => x.OrderItemId == item.Id);
                            if (redemption != null)
                            {
                                var order = await rewardsDBContext.OrderItems.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == item.Id);
                                detail.DeliveryuserName = order.Order.ShippingPersonFirstName + " " + order.Order.ShippingPersonLastName;
                                detail.DeliveryAddress = "(+" + order.Order.ShippingMobileCountryCode + ") " + order.Order.ShippingMobileNumber + "<br>" +
                                   order.Order.ShippingAddressLine1;
                                if (order.Order.ShippingAddressLine2 != "" && order.Order.ShippingAddressLine2 != null)
                                {
                                    detail.DeliveryAddress += ", " + order.Order.ShippingAddressLine2 + ", " + order.Order.ShippingCity + ", " + order.Order.ShippingPostcode + ", " + order.Order.ShippingState + ", " + order.Order.ShippingCountry;
                                }
                                else
                                {
                                    detail.DeliveryAddress += ", " + order.Order.ShippingCity + ", " + order.Order.ShippingPostcode + ", " + order.Order.ShippingState + ", " + order.Order.ShippingCountry;
                                }

                                //For Request 1749
                                detail.ShortId = order.ShortId;
                            
                                detail.Points = order.Points;
                                detail.Price = order.Price;
                                detail.DiscountedAmount = order.DiscountedAmount;
                                detail.SubtotalPrice = order.SubtotalPrice;
                                detail.Status = order.Status;
                               

                                detail.ProductId = Convert.ToInt32(product.ProductId);

                                detail.Courier = "";
                                detail.CreatedAt = redemption.CreatedAt.ToString("dd-M-yyyy HH:mm");
                                detail.IsRedeemed = redemption.IsRedeemed;
                                detail.RedeemedAt = redemption.RedeemedAt.HasValue ? redemption.RedeemedAt.Value.ToString("dd-M-yyyy HH:mm") : "";
                                detail.Token = redemption.Token;
                                detail.TokenType = redemption.TokenType;
                                detail.ValidFrom = redemption.StartDate.ToString("dd-M-yyyy HH:mm");
                                detail.ValidEnded = redemption.ExpiredDate.ToString("dd-M-yyyy HH:mm");
                                detail.PaidAmount = redemption.Revenue.Value;
                                detail.OrderedAt = order.Order.CreatedAt.ToString("dd-M-yyyy HH:mm");
                                if (redemption.TokenType == 1)
                                {
                                    detail.TokenImageSrc = Voupon.Common.QRGenerator.QRCodeGenerator.GenerateQRCodeHtmlImageSrc(appSettings.Value.App.VouponMerchantAppBaseUrl + "/qr/v/" + redemption.Token, 200, 200, 0);
                                }
                                detail.MerchantInfo = new MerchantDetails();
                                detail.MerchantInfo.Name = order.MerchantDisplayName;
                                var merchant = rewardsDBContext.Merchants.Where(x => x.Id == order.MerchantId).FirstOrDefault();
                                if (merchant != null)
                                {
                                    detail.MerchantInfo.ImageUrl = merchant.LogoUrl;

                                    var merchantManagerUser = await rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.UserRoles.Any(y => y.MerchantId == merchant.Id && y.RoleId.ToString() == "1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefaultAsync();
                                    if (merchantManagerUser != null)
                                    {
                                        detail.MerchantInfo.Email = merchantManagerUser.Email;
                                        detail.MerchantInfo.Id = merchant.Id;
                                    }
                                }

                            }
                            break;
                        case 4:
                            //Digital Redemption
                            var digitalRedemption = await rewardsDBContext.DigitalRedemptionTokens.FirstOrDefaultAsync(x => x.OrderItemId == item.Id);
                            if (digitalRedemption != null)
                            {
                                var order = await rewardsDBContext.OrderItems.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == item.Id);
                                detail.DeliveryuserName = order.Order.ShippingPersonFirstName + " " + order.Order.ShippingPersonLastName;
                                detail.DeliveryAddress = "(+" + order.Order.ShippingMobileCountryCode + ") " + order.Order.ShippingMobileNumber + "<br>" +
                                   order.Order.ShippingAddressLine1;
                                if (order.Order.ShippingAddressLine2 != "" && order.Order.ShippingAddressLine2 != null)
                                {
                                    detail.DeliveryAddress += ", " + order.Order.ShippingAddressLine2 + ", " + order.Order.ShippingCity + ", " + order.Order.ShippingPostcode + ", " + order.Order.ShippingState + ", " + order.Order.ShippingCountry;
                                }
                                else
                                {
                                    detail.DeliveryAddress += ", " + order.Order.ShippingCity + ", " + order.Order.ShippingPostcode + ", " + order.Order.ShippingState + ", " + order.Order.ShippingCountry;
                                }
                                detail.Courier = "";
                                detail.CreatedAt = digitalRedemption.CreatedAt.ToString("dd-M-yyyy HH:mm");
                                detail.IsRedeemed = digitalRedemption.IsRedeemed;
                                detail.RedeemedAt = digitalRedemption.RedeemedAt.HasValue ? digitalRedemption.RedeemedAt.Value.ToString("dd-M-yyyy HH:mm") : "";
                                detail.TokenType = digitalRedemption.TokenType;
                                detail.Token = digitalRedemption.Token;
                                detail.ValidFrom = digitalRedemption.StartDate.ToString("dd-M-yyyy HH:mm");
                                detail.ValidEnded = digitalRedemption.ExpiredDate.ToString("dd-M-yyyy HH:mm");
                                detail.PaidAmount = digitalRedemption.Revenue.Value;
                                detail.OrderedAt = order.Order.CreatedAt.ToString("dd-M-yyyy HH:mm");

                                detail.MerchantInfo = new MerchantDetails();
                                detail.MerchantInfo.Name = order.MerchantDisplayName;

                                //For Request 1749
                                detail.ShortId = order.ShortId;
                                detail.Points = order.Points;
                                detail.Price = order.Price;
                                detail.DiscountedAmount = order.DiscountedAmount;
                                detail.SubtotalPrice = order.SubtotalPrice;
                                detail.Status = order.Status;

                                detail.ProductId = Convert.ToInt32(product.ProductId);


                                var merchant = rewardsDBContext.Merchants.Where(x => x.Id == order.MerchantId).FirstOrDefault();
                                if (merchant != null)
                                {
                                    detail.MerchantInfo.ImageUrl = merchant.LogoUrl;

                                    var merchantManagerUser = await rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.UserRoles.Any(y => y.MerchantId == merchant.Id && y.RoleId.ToString() == "1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefaultAsync();
                                    if (merchantManagerUser != null)
                                    {
                                        detail.MerchantInfo.Email = merchantManagerUser.Email;
                                        detail.MerchantInfo.Id = merchant.Id;
                                    }
                                }
                            }
                            break;
                        case 5:
                            //Delivery
                            var deliveryRedemption = await rewardsDBContext.DeliveryRedemptionTokens.FirstOrDefaultAsync(x => x.OrderItemId == item.Id);
                            if (deliveryRedemption != null)
                            {
                                var order = await rewardsDBContext.OrderItems.Include(x => x.Order).FirstOrDefaultAsync(x => x.Id == item.Id);


                                //For Request 1749
                                detail.ShortId = order.ShortId;
                                detail.Points = order.Points;
                                detail.Price = order.Price;
                                detail.DiscountedAmount = order.DiscountedAmount;
                                detail.SubtotalPrice = order.SubtotalPrice;
                                detail.Status = order.Status;


                                detail.ProductId = Convert.ToInt32(product.ProductId);

                                detail.DeliveryuserName = order.Order.ShippingPersonFirstName + " " + order.Order.ShippingPersonLastName;
                                detail.DeliveryAddress = "(+" + order.Order.ShippingMobileCountryCode + ") " + order.Order.ShippingMobileNumber + "<br>" +
                                   order.Order.ShippingAddressLine1;
                                if (order.Order.ShippingAddressLine2 != "" && order.Order.ShippingAddressLine2 != null)
                                {
                                    detail.DeliveryAddress += ", " + order.Order.ShippingAddressLine2 + ", " + order.Order.ShippingCity + ", " + order.Order.ShippingPostcode + ", " + order.Order.ShippingState + ", " + order.Order.ShippingCountry;
                                }
                                else
                                {
                                    detail.DeliveryAddress += ", " + order.Order.ShippingCity + ", " + order.Order.ShippingPostcode + ", " + order.Order.ShippingState + ", " + order.Order.ShippingCountry;
                                }
                                detail.PaidAmount = deliveryRedemption.Revenue.Value;
                                detail.OrderedAt = order.Order.CreatedAt.ToString("dd-M-yyyy HH:mm");
                                detail.Courier = deliveryRedemption.CourierProvider;
                                detail.CreatedAt = deliveryRedemption.CreatedAt.ToString("dd-M-yyyy HH:mm");
                                detail.IsRedeemed = deliveryRedemption.IsRedeemed;
                                detail.RedeemedAt = deliveryRedemption.RedeemedAt.HasValue ? deliveryRedemption.RedeemedAt.Value.ToString("dd-M-yyyy HH:mm") : "";
                                detail.Token = deliveryRedemption.Token;
                                detail.ValidFrom = deliveryRedemption.StartDate.HasValue ? deliveryRedemption.StartDate.Value.ToString("dd-M-yyyy HH:mm") : "";
                                detail.ValidEnded = deliveryRedemption.ExpiredDate.HasValue ? deliveryRedemption.ExpiredDate.Value.ToString("dd-M-yyyy HH:mm") : "";
                                detail.ExpectedDeliveryDate = deliveryRedemption.CreatedAt.AddDays(14).ToString("dd-M-yyyy HH:mm");

                                detail.MerchantInfo = new MerchantDetails();
                                detail.MerchantInfo.Name = order.MerchantDisplayName;
                                var merchant = rewardsDBContext.Merchants.Where(x => x.Id == order.MerchantId).FirstOrDefault();
                                if (merchant != null)
                                {
                                    detail.MerchantInfo.ImageUrl = merchant.LogoUrl;

                                    var merchantManagerUser = await rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.UserRoles.Any(y => y.MerchantId == merchant.Id && y.RoleId.ToString() == "1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefaultAsync();
                                    if (merchantManagerUser != null)
                                    {
                                        detail.MerchantInfo.Email = merchantManagerUser.Email;
                                        detail.MerchantInfo.Id = merchant.Id;
                                    }
                                }

                                var jsonStr = "";
                                if (deliveryRedemption.ShippingDetailsJSON != null)
                                {
                                    jsonStr = deliveryRedemption.ShippingDetailsJSON.Substring(1, deliveryRedemption.ShippingDetailsJSON.Length - 2);
                                }

                                var jsonShippingDetails = JsonConvert.DeserializeObject<ShippingInternalProdJsonObj>(jsonStr);

                                if (jsonShippingDetails != null)
                                {
                                    detail.OrderId = jsonShippingDetails.OrderId;
                                    detail.OrderItemId = jsonShippingDetails.OrderItemId;
                                    detail.OrderTypeId = jsonShippingDetails.OrderTypeId;
                                    detail.LatestStatusId = jsonShippingDetails.LatestStatusId;
                                    detail.LatestStatus = jsonShippingDetails.LatestStatus;
                                    detail.LatestUpdatedAt = jsonShippingDetails.LatestUpdatedAt;

                                    List<TrackingDetailsModel> trackingDetails = new List<TrackingDetailsModel>();

                                    foreach (var trackingRecord in jsonShippingDetails.Shipping)
                                    {

                                        TrackingDetailsModel trackingDetailsModel = new TrackingDetailsModel();

                                        trackingDetailsModel.Date = Convert.ToDateTime(trackingRecord.Date);
                                        trackingDetailsModel.status = trackingRecord.status;
                                        trackingDetailsModel.StatusId = trackingRecord.StatusId;
                                        trackingDetailsModel.Content = trackingRecord.Content;
                                        trackingDetailsModel.Location = trackingRecord.Location;
                                        
                                        trackingDetails.Add(trackingDetailsModel);

                                    }

                                    detail.TrackingList = trackingDetails;
                                }

                            }
                            break;
                    }
                    response.Successful = true;
                    response.Message = "Get Order Item Successfully";
                    response.Data = detail;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "No Order Item";

                }


            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
