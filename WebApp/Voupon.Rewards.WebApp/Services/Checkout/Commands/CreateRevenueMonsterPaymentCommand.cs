using MediatR;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Infrastructures.Helpers.Crypto;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Text;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using Voupon.Common.Services.ActivityLogs.Commands;

namespace Voupon.Rewards.WebApp.Services.Checkout.Commands
{
    public class CreateRevenueMonsterPaymentCommand : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
        public Guid Id { get; set; }
        public byte PaymentProviderId { get; set; }
        public Guid RefNo { get; set; }
        public string MerchantCode { get; set; }
        public int PaymentId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Remark { get; set; }
        public string TransactionId { get; set; }
        public string AuthCode { get; set; }
        public string Status { get; set; }
        public string ErrorDescription { get; set; }
        public string Signature { get; set; }
        public string CreditCardName { get; set; }
        public string CreditCardNumber { get; set; }
        public string CreditCardBankName { get; set; }
        public string CreditCardIssuingCountry { get; set; }
        public string JsonResponse { get; set; }
        public DateTime CreatedAt { get; set; }

        public string ShortOrderId { get; set; }

        public string Username { get; set; }

        public bool IsRegenFromAdmin { get; set; }

        public class CreateRevenueMonsterPaymentCommandHandler : IRequestHandler<CreateRevenueMonsterPaymentCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext _rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            private string _email;

            public CreateRevenueMonsterPaymentCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                _rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(CreateRevenueMonsterPaymentCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var viewModel = new PaymentResponseViewModel
                {
                    UpdatePoints = false
                };
                var sendgridMessageList = new List<SendGridMessage>();
                try
                {
                    var firstName = "";
                    var lastName = "";

                    //  Wait x seconds incase user close browser too fast. This will sleep for 3 seconds check if payment details already created
                    Thread.Sleep(1500);

                    var orderPayment = _rewardsDBContext.OrderPayments.Where(x => x.PaymentProviderId == request.PaymentProviderId && x.TransactionId == request.TransactionId).FirstOrDefault();

                    if (orderPayment != null)
                    {

                        apiResponseViewModel.Message = "Payment details already exists";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    var orderPaymentCheck = _rewardsDBContext.OrderPayments.Where(x => x.RefNo == request.RefNo).FirstOrDefault();
                    if (orderPaymentCheck != null)
                    {
                        apiResponseViewModel.Message = "Payment details already exists";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    var setOrder = _rewardsDBContext.Orders.Where(x => x.ShortId == request.ShortOrderId).FirstOrDefault();
                    if(setOrder == null)
                    {
                        apiResponseViewModel.Message = "Invalid order";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }
                    
                    
                    if(setOrder.OrderStatus == 100)
                    {
                        apiResponseViewModel.Message = "Order is in processing";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }
                    
                    setOrder.OrderStatus = 100;
                    _rewardsDBContext.Orders.Update(setOrder);
                    await _rewardsDBContext.SaveChangesAsync();

                    _rewardsDBContext.Database.BeginTransaction();

                    //  Update Order Status
                    var order = _rewardsDBContext.Orders.Include(x => x.OrderItems).Include(x=> x.OrderShopExternal).ThenInclude(x=> x.OrderItemExternal).Where(x => x.ShortId == request.ShortOrderId).FirstOrDefault();

                    if (order == null)
                    {
                        apiResponseViewModel.Message = "Invalid order";
                        apiResponseViewModel.Successful = false;

                        var errorLogs = new ErrorLogs
                        {
                            ActionName = "CreateRevenueMonsterPaymentCommand",
                            Errors = "Invalid order. Order is null",
                            TypeId = CreateErrorLogCommand.Type.Service,
                            ActionRequest = JsonConvert.SerializeObject(request),
                            CreatedAt = DateTime.Now
                        };

                        await _rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                        await _rewardsDBContext.SaveChangesAsync();
                        _rewardsDBContext.Database.CommitTransaction();
                        return apiResponseViewModel;
                    }

                    vodusV2Context.Database.BeginTransaction();
                    var masterMemberProfile = vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.Id == order.MasterMemberProfileId).FirstOrDefault();
                    if (masterMemberProfile == null)
                    {
                        await new Logs
                        {
                            Description = "Failed to create payment, invalid master memberprofile",
                            Email = "",
                            JsonData = JsonConvert.SerializeObject(request),
                            ActionName = "CreateRevenueMonsterPaymentCommand",
                            TypeId = CreateErrorLogCommand.Type.Service,
                            SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                            RewardsDBContext = _rewardsDBContext
                        }.Error();

                        _rewardsDBContext.Database.CommitTransaction();
                        vodusV2Context.Database.RollbackTransaction();

                        apiResponseViewModel.Message = "Invalid request";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    firstName = masterMemberProfile.User.FirstName;
                    lastName = masterMemberProfile.User.LastName;
                    _email = masterMemberProfile.User.Email;

                    var itemPrice = "";
                    var discountedAmount = "0";
                    var subTotal = "";

                    //  Create Order Payments
                    _rewardsDBContext.OrderPayments.Add(new OrderPayments
                    {
                        Id = Guid.NewGuid(),
                        Amount = request.Amount,
                        AuthCode = request.AuthCode,
                        CreatedAt = DateTime.Now,
                        CreditCardBankName = (request.CreditCardBankName != null ? request.CreditCardBankName : null),
                        CreditCardName = (request.CreditCardName != null ? request.CreditCardName : null),
                        CreditCardNumber = (request.CreditCardNumber != null ? request.CreditCardNumber : null),
                        CreditCardIssuingCountry = (request.CreditCardIssuingCountry != null ? request.CreditCardIssuingCountry : null),
                        Currency = request.Currency,
                        MerchantCode = request.MerchantCode,
                        PaymentId = request.PaymentId,
                        PaymentProviderId = request.PaymentProviderId,
                        RefNo = order.Id,
                        Remark = (request.Remark != null ? request.Remark : null),
                        Signature = request.Signature,
                        Status = request.Status,
                        TransactionId = request.TransactionId,
                        ErrorDescription = (request.ErrorDescription != null ? request.ErrorDescription : null),
                        JsonResponse = request.JsonResponse,
                    });

                    itemPrice = request.Amount.ToString();

                    order.OrderStatus = 2;

                    //  Deduct points if any
                    //  Points is already deducted during order creation. Will refund points back to user when they cancel
                    //  or during order cancellation scheduler 
                    if (order.TotalPoints > 0)
                    {
                        if (masterMemberProfile.AvailablePoints < order.TotalPoints)
                        {
                            apiResponseViewModel.Message = "Not enough VPoints to complete transactions";
                            apiResponseViewModel.Successful = false;

                            var errorLogs = new ErrorLogs
                            {
                                ActionName = "CreatePaymentCommand",
                                Errors = "Not enough VPoints to complete transactions",
                                TypeId = CreateErrorLogCommand.Type.Service,
                                ActionRequest = JsonConvert.SerializeObject(request),
                                CreatedAt = DateTime.Now,
                                MasterProfileId = order.MasterMemberProfileId
                            };

                            await _rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                            await _rewardsDBContext.SaveChangesAsync();
                            _rewardsDBContext.Database.CommitTransaction();
                            vodusV2Context.Database.RollbackTransaction();

                            return apiResponseViewModel;
                        }

                        masterMemberProfile.AvailablePoints = masterMemberProfile.AvailablePoints - order.TotalPoints;
                        vodusV2Context.MasterMemberProfiles.Update(masterMemberProfile);
                        await vodusV2Context.SaveChangesAsync();

                        viewModel.UpdatePoints = true;
                        viewModel.AvailablePoints = masterMemberProfile.AvailablePoints;
                    }
                    
                    _rewardsDBContext.Orders.Update(order);

                    //  Adding quantity to order items and remove the duplicates
                    var orderItemsList = order.OrderItems.ToList();
                    int[] productsQuantitiies = new int[orderItemsList.Count];

                    for (int i = 0; i < orderItemsList.Count; i++)
                    {
                        productsQuantitiies[i]++;
                        if (i + 1 != orderItemsList.Count)
                        {
                            if (orderItemsList[i + 1].ProductId == orderItemsList[i].ProductId && orderItemsList[i + 1].Price == orderItemsList[i].Price && orderItemsList[i + 1].DiscountedAmount == orderItemsList[i].DiscountedAmount)
                            {
                                orderItemsList.RemoveAt(i + 1);
                                i--;
                            }
                        }
                    }
                    int j = 0;

                    //  Generate Token
                    var orderReceiptView = "";

                    string[] orderReceiptViewList = new string[orderItemsList.Count];
                    foreach (var item in orderItemsList)
                    {
                        int quantity = productsQuantitiies[j];
                        var address = order.ShippingAddressLine1 + ", ";
                        if (order.ShippingAddressLine2 != null && order.ShippingAddressLine2 != "")
                            address += order.ShippingAddressLine2 + ", ";
                        address += order.ShippingPostcode + " ";
                        address += order.ShippingCity + " ";
                        address += order.ShippingState + ", ";
                        address += order.ShippingCountry;

                        orderReceiptViewList[j] = $"<table border='0' cellpadding='0' cellspacing='0' width='100%' class='x_478386741tableButton'>" +
                                     "<tbody style = 'font-family:Calibri'>" +
                                     "<tr>" +
                                     "<td align='center' valign='top' style='padding-top:20px; padding-bottom:20px; width:100%;'>" +
                                     "<table style = 'width:100%; max-width :  800px; '>" +
                                     "<tbody>" +
                               "<tr style = 'background-color:#eee;font-weight:bold;'>" +
                                  "<td style = 'padding: 0 15px'> Item </td>" +
                                   "<td></td>" +
                                   "<td style = 'padding:5px;text-align:center;'> Qty </td>" +
                                    "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;'> Price </td>" +
                                "</tr>" +
                                "<tr>" +
                                "<td width = '15%'>" +
                                    $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductImageFolderUrl.Split(",").Last().Replace("http://", "https://")}'>" + "</td>" +
                                        "<td style = 'padding:5px;font-size:12px;'>" +
                                        $"<a style = 'font-size:14px;color:#666;text-decoration:none;' href ='https://vodus.my/Order/History' target = '_blank'>" +
                                                $"{item.ProductTitle}" +
                                        "</a>" +
                                    "</td> " +
                                    $"<td style = 'padding:5px;text-align:center;'>{quantity}</td>" +
                                    "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;color:#8C37F6;font-weight:bold;'>RM " +
                                            item.SubtotalPrice +
                                    "</td>" +
                                    "</tr>" +
                                "</tbody> " +
                            "</table>" +
                            "<table style='border:1px #ddd solid;border-left:0;border-right:0;'>" +
                                            "<tbody>" +
                                              "<tr>" +
                                            "<td width='50%' style='padding:15px;padding-right:0;'> " +
                                                "<p style = 'color:#000;font-size:16px;font-weight:bold;text-align:left;margin:0'> Shipping Address </p>" +
                                                "<p style = 'text-align:left;font-size:13px;margin:5px 0'>" +
                                                    $"{address}" +
                                                "</p>" +
                                            "</td>" +
                                            "<td width='50%' style='min-width:150px;padding:15px;padding-left:0;'>" +
                                            "<table style='width:100%;'> " +
                                               "<tbody style='padding:0 15px;font-size:16px;'>  " +
                                                   "<tr>" +
                                                       "<td width='50%' style='text-align: right;'> VPoints:</td> " +
                                                      $"<td width='50%' style='text-align: right;'>{ item.Points * quantity} </td> " +
                                                   "</tr>  " +
                                                   "<tr>  " +
                                                       "<td width='50%' style='text-align:right;'> Subtotal: </td> " +
                                                       $"<td width='50%' style='text-align:right;'> RM {item.SubtotalPrice * quantity}</td>" +
                                                   "</tr> " +
                                                   "<tr style='width: 100%; vertical-align: top; '> " +
                                                       "<td width = '50%' style='text-align:right;'> Discount:</td>" +
                                                       $"<td width = '50%' style='text-align:right;'> -RM {item.DiscountedAmount * quantity}</td>" +
                                                   "</tr>  " +
                                                   "<tr style='width: 100%; vertical-align: top; '> " +
                                                       "<td width='50%' style='text-align:right;'> Shipping Cost:</td> " +
                                                        $"<td width='50%' style='text-align:right;'> RM {item.ShippingCost} </td>  " +
                                                   "</tr> " +
                                                   "<tr style='vertical-align:top;font-weight:bold;'> " +
                                                       "<td width='50%' style='text-align:right;'> Total:</td> " +
                                                        $"<td width='50%' style='text-align:right;color:#8C37F6;'> RM {(item.Price * quantity) + item.ShippingCost} </td>  " +
                                                   "</tr> " +
                                               "</tbody> " +
                                           " </table> " +
                                           " </td> " +
                                           " </tr> " +
                                           " </tbody> " +
                                        "</table> " +
                                    "</td> " +
                                "</tr> " +
                            "</tbody> " +
                        "</table> ";

                        orderReceiptView += orderReceiptViewList[j];

                       j++;
                    }
                    foreach (var shop in order.OrderShopExternal)
                    {
                        bool isShippingCostIncludedForShop = false;
                        foreach (var item in shop.OrderItemExternal)
                        {
                            var address = order.ShippingAddressLine1 + ", ";
                            if (order.ShippingAddressLine2 != null && order.ShippingAddressLine2 != "")
                                address += order.ShippingAddressLine2 + ", ";
                            address += order.ShippingPostcode + " ";
                            address += order.ShippingCity + " ";
                            address += order.ShippingState + ", ";
                            address += order.ShippingCountry;
                            decimal shippingCost = shop.ShippingCost;
                            if(isShippingCostIncludedForShop)
                            {
                                shippingCost = 0;
                            }

                            orderReceiptView += $"<table border='0' cellpadding='0' cellspacing='0' width='100%' class='x_478386741tableButton'>" +
                                         "<tbody style = 'font-family:Calibri'>" +
                                         "<tr>" +
                                         "<td align='center' valign='top' style='padding-top:20px; padding-bottom:20px; width:100%;'>" +
                                         "<table style = 'width:100%; max-width :  800px; '>" +
                                         "<tbody>" +
                                   "<tr style = 'background-color:#eee;font-weight:bold;'>" +
                                      "<td style = 'padding: 0 15px'> Item </td>" +
                                       "<td></td>" +
                                       "<td style = 'padding:5px;text-align:center;'> Qty </td>" +
                                        "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;'> Price </td>" +
                                    "</tr>" +
                                    "<tr>" +
                                    "<td width = '15%'>" +
                                        $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductCartPreviewSmallImageURL.Replace("http://", "https://")}'>" + "</td>" +
                                            "<td style = 'padding:5px;font-size:12px;'>" +
                                            $"<a style = 'font-size:14px;color:#666;text-decoration:none;' href ='https://vodus.my/Order/History' target = '_blank'>" +
                                                    $"{item.ProductTitle}" +
                                            "</a>" +
                                        "</td> " +
                                        $"<td style = 'padding:5px;text-align:center;'>{item.Quantity}</td>" +
                                        "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;color:#8C37F6;font-weight:bold;'>RM " +
                                                item.OriginalPrice +
                                        "</td>" +
                                        "</tr>" +
                                    "</tbody> " +
                                "</table>" +
                                "<table style='border:1px #ddd solid;border-left:0;border-right:0;'>" +
                                                "<tbody>" +
                                                  "<tr>" +
                                                "<td width='50%' style='padding:15px;padding-right:0;'> " +
                                                    "<p style = 'color:#000;font-size:16px;font-weight:bold;text-align:left;margin:0'> Shipping Address </p>" +
                                                    "<p style = 'text-align:left;font-size:13px;margin:5px 0'>" +
                                                        $"{address}" +
                                                    "</p>" +
                                                "</td>" +
                                                "<td width='50%' style='min-width:150px;padding:15px;padding-left:0;'>" +
                                                "<table style='width:100%;'> " +
                                                   "<tbody style='padding:0 15px;font-size:16px;'>  " +
                                                       "<tr>" +
                                                           "<td width='50%' style='text-align: right;'> VPoints:</td> " +
                                                          $"<td width='50%' style='text-align: right;'>{ item.Points} </td> " +
                                                       "</tr>  " +
                                                       "<tr>  " +
                                                           "<td width='50%' style='text-align:right;'> Subtotal: </td> " +
                                                           $"<td width='50%' style='text-align:right;'> RM {item.OriginalPrice * item.Quantity}</td>" +
                                                       "</tr> " +
                                                       "<tr style='width: 100%; vertical-align: top; '> " +
                                                           "<td width = '50%' style='text-align:right;'> Discount:</td>" +
                                                           $"<td width = '50%' style='text-align:right;'> -RM {item.DiscountedAmount * item.Quantity}</td>" +
                                                       "</tr>  " +
                                                       "<tr style='width: 100%; vertical-align: top; '> " +
                                                           "<td width='50%' style='text-align:right;'> Shipping Cost:</td> " +
                                                            $"<td width='50%' style='text-align:right;'> RM {shop.ShippingCost} </td>  " +
                                                       "</tr> " +
                                                       "<tr style='vertical-align:top;font-weight:bold;'> " +
                                                           "<td width='50%' style='text-align:right;'> Total:</td> " +
                                                            $"<td width='50%' style='text-align:right;color:#8C37F6;'> RM {(item.OriginalPrice * item.Quantity) - (item.DiscountedAmount * item.Quantity) + shop.ShippingCost} </td>  " +
                                                       "</tr> " +
                                                   "</tbody> " +
                                               " </table> " +
                                               " </td> " +
                                               " </tr> " +
                                               " </tbody> " +
                                            "</table> " +
                                        "</td> " +
                                    "</tr> " +
                                "</tbody> " +
                            "</table> ";
                            if (shop.OrderItemExternal.Count > 1 && shop.ShippingCost > 0)
                            {
                                isShippingCostIncludedForShop = true;
                            }
                        }
                    }


                    await _rewardsDBContext.SaveChangesAsync();

                    // Update product quantity
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == item.ProductId);

                        if (!item.IsVariationProduct) {
                            product.AvailableQuantity--;
                            //if(product.AvailableQuantity == 0)
                            //{
                            //   //Update all cart products to OutOfStock
                            //}
                        }
                        else
                        {
                            var productVariation = await _rewardsDBContext.ProductVariation.FirstOrDefaultAsync(x => x.Id == item.VariationId);
                            productVariation.AvailableQuantity--;
                            //if(productVariation.AvailableQuantity == 0)
                            //{
                            //   //Update all cart products to OutOfStock
                            //}
                            _rewardsDBContext.ProductVariation.Update(productVariation);
                        }
                        product.TotalBought++;
                        _rewardsDBContext.Products.Update(product);
                        //  masterMemberProfile.User.Email
                        var tokenResult = await CreateToken(item.Id, item.ProductId, masterMemberProfile.Id, masterMemberProfile.User.Email, masterMemberProfile.User.FirstName + " " + masterMemberProfile.User.LastName);
                        if (tokenResult != null)
                        {
                            sendgridMessageList.Add(tokenResult);
                        }
                    }

                    await _rewardsDBContext.SaveChangesAsync();
                    _rewardsDBContext.Database.CommitTransaction();
                    vodusV2Context.Database.CommitTransaction();
                    j = 0;
                    foreach(var item in orderItemsList)
                    {
                        //  Generate notification email to merchant 
                        var merchantEmail = _rewardsDBContext.UserRoles.Include(x => x.User).Where(x => x.MerchantId == item.MerchantId && x.RoleId == new Guid("1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefault().User.Email;
                        var sendGridClientForMerchant = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                        var msgForMerchant = new SendGridMessage();
                        //  In Store
                        if (item.ExpirationTypeId == 2)
                        {
                            msgForMerchant.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.InStoreMerchantSalesNotificationEmail);
                        }
                        // Digital
                        else if (item.ExpirationTypeId == 4)
                        {
                            msgForMerchant.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.DigitalMerchantSalesNotificationEmail);
                        }
                        // Delivery
                        else if (item.ExpirationTypeId == 5)
                        {
                            msgForMerchant.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.DeliveryMerchantSalesNotificationEmail);
                            msgForMerchant.AddSubstitution("-name-", masterMemberProfile.User.FirstName + " " + masterMemberProfile.User.LastName);
                            if (order.ShippingAddressLine2 != null || order.ShippingAddressLine2 != "")
                            {
                                msgForMerchant.AddSubstitution("-address-", order.ShippingAddressLine1 + ", " + order.ShippingAddressLine2 + ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry);
                            }
                            else
                            {
                                msgForMerchant.AddSubstitution("-address-", order.ShippingAddressLine1 + ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry);
                            }
                            msgForMerchant.AddSubstitution("-phone-", "+(" + order.ShippingMobileCountryCode + ")" + order.ShippingMobileNumber);
                        }

                        msgForMerchant.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                        msgForMerchant.SetSubject("Vodus - New order");
                        msgForMerchant.AddTo(new EmailAddress(merchantEmail));
                        msgForMerchant.AddSubstitution("-orderTable-", orderReceiptViewList[j]);
                        var responseMerchant = sendGridClientForMerchant.SendEmailAsync(msgForMerchant).Result;
                        //  Add to activity log
                        await new CreateActivityLogCommand(_rewardsDBContext).Create(new CreateActivityLogCommand.CreateActivityRequest
                        {
                            ActionName = "CreateRevenueMonsterPaymentCommand",
                            ActionFunction = "MerchantEmail",
                            ActionData = JsonConvert.SerializeObject(request),
                            ActionId = request.RefNo.ToString(),
                            TriggerBy = _email,
                            Message = await responseMerchant.Body.ReadAsStringAsync(),
                            TriggerFor = merchantEmail,
                            IsSuccessful = true

                        });

                        j++;
                    }

                    //  Once transaction is completed. Send email
                    //  Generate Receipt
                    var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                    var msg = new SendGridMessage();
                    msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.OrderConfirmation);
                    msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                    msg.SetSubject("Thank you for your order");
                    msg.AddTo(new EmailAddress(masterMemberProfile.User.Email));
                    msg.AddSubstitution("-customerName-", firstName + " " + lastName);
                    msg.AddSubstitution("-orderNumber-", "#" + order.ShortId);
                    msg.AddSubstitution("-orderDate-", order.CreatedAt.ToString("dd/MM/yyyy"));
                    msg.AddSubstitution("-orderTable-", orderReceiptView);

                    //  Send invoice email to vodus.receipt@gmail.com
                    msg.SetSubject("Order Invoice " + order.CreatedAt.ToString("dd/MM/yyyy"), 1);
                    msg.AddTo(new EmailAddress("vodus.receipt@gmail.com"), 1);
                    msg.AddSubstitution("-customerName-", firstName + " " + lastName, 1);
                    msg.AddSubstitution("-orderNumber-", "#" + order.ShortId, 1);
                    msg.AddSubstitution("-orderDate-", order.CreatedAt.ToString("dd/MM/yyyy"), 1);
                    msg.AddSubstitution("-orderTable-", orderReceiptView, 1);
                    var response = sendGridClient.SendEmailAsync(msg).Result;

                    //  Send itemized 
                    if (sendgridMessageList != null && sendgridMessageList.Any())
                    {
                        foreach (var tokenMessage in sendgridMessageList)
                        {
                            msg = tokenMessage;
                            var resp = sendGridClient.SendEmailAsync(msg).Result;
                        }
                    }

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = viewModel;
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    _rewardsDBContext.Database.RollbackTransaction();
                    vodusV2Context.Database.RollbackTransaction();

                    await new Logs
                    {
                        Description = ex.ToString(),
                        Email = "",
                        JsonData = JsonConvert.SerializeObject(request),
                        ActionName = "CreateRevenueMonsterPaymentCommand",
                        TypeId = CreateErrorLogCommand.Type.Service,
                        SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                        RewardsDBContext = _rewardsDBContext
                    }.Error();

                    //  Send error email to tech team.
                    apiResponseViewModel.Message = "Fail to process request: " + ex.ToString();
                    apiResponseViewModel.Successful = false;
                    return apiResponseViewModel;
                }
            }
            private async Task<SendGridMessage> CreateToken(Guid orderItemId, int productId, int masterMemberProfile, string email, string fullName)
            {
                var product = _rewardsDBContext.Products.Include(x => x.DealExpirations).FirstOrDefault(x => x.Id == productId);
                if (product != null)
                {
                    var exist = await _rewardsDBContext.DigitalRedemptionTokens.Where(x => x.OrderItemId == orderItemId).FirstOrDefaultAsync();
                    if (exist != null)
                    {
                        return null;
                    }

                    var deallExpiration = _rewardsDBContext.DealExpirations.FirstOrDefault(x => x.Id == product.DealExpirationId);// product.DealExpiration;
                    var itemPrice = product.DiscountedPrice.HasValue ? product.DiscountedPrice.Value : (product.Price.HasValue ? product.Price.Value : 0);

                    var msg = new SendGridMessage();

                    if (deallExpiration.ExpirationTypeId == 1)
                    {
                        //instore redemption 
                        var item = new InStoreRedemptionTokens();
                        item.StartDate = DateTime.Now;
                        item.ExpiredDate = DateTime.Now.AddDays(deallExpiration.TotalValidDays.Value);
                        item.ProductId = productId;
                        item.IsActivated = true;
                        item.IsRedeemed = false;
                        item.MasterMemberProfileId = masterMemberProfile;
                        item.MerchantId = product.MerchantId;
                        item.OutletId = null;
                        item.ProductTitle = product.Title;
                        item.OrderItemId = orderItemId;
                        item.RedeemedAt = null;
                        item.CreatedAt = DateTime.Now;
                        item.RedemptionInfo = product.RedemptionInfo;
                        item.Revenue = itemPrice;// revenue;
                        item.Token = "";
                        item.Email = email;
                        _rewardsDBContext.InStoreRedemptionTokens.Add(item);
                        _rewardsDBContext.SaveChanges();

                        if (product.ThirdPartyTypeId != null)
                        {
                            //var result = await ThirdPartyRedemption(product);

                            //if (result != null)
                            //{
                            //    if (result.IsSuccessful)
                            //    {
                            //        item.Token = result.Url;
                            //        item.TokenType = 2;
                            //        _rewardsDBContext.SaveChanges();

                            //        var sendGridClient2 = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                            //        var msg2 = new SendGridMessage();
                            //        msg2.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.ThirdPartyRedemption);
                            //        msg2.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                            //        msg2.SetSubject("Vodus Redemption Url");
                            //        msg2.AddTo(new EmailAddress(email));

                            //        msg2.AddSubstitution("-customerName-", fullName);
                            //        msg2.AddSubstitution("-item-", product.Title);
                            //        msg2.AddSubstitution("-redemptionUrl-", result.Url);
                            //        msg2.AddSubstitution("-imageUrl-", product.ImageFolderUrl);
                            //        //var response2 = sendGridClient2.SendEmailAsync(msg2).Result;
                            //        return msg2;
                            //    }
                            //}
                            //return null;
                        }
                        else
                        {
                            item.Token = Voupon.Common.RedemptionTokenGenerator.InStoreRedemptionToken.GenerateToken(item.Id);
                            _rewardsDBContext.SaveChanges();

                            var url = appSettings.Value.App.BaseUrl + "/order/store-redemption/" + item.Id;

                            var image = GenerateQRCodeHtmlImageSrc(appSettings.Value.App.VouponMerchantAppBaseUrl + "/qr/v/" + item.Token, 200, 200, 0);

                            msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.InStoreRedemption);
                            msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                            msg.SetSubject("Vodus InStore Redemption");
                            msg.AddTo(new EmailAddress(email));
                            msg.AddTo("merchant@vodus.my");
                            msg.AddSubstitution("-productTitle-", product.Title);
                            msg.AddSubstitution("-voucherCode-", item.Token);
                            msg.AddSubstitution("-voucherUrl-", "");
                            msg.AddSubstitution("-redemptionFlow-", product.RedemptionInfo);
                            return msg;
                        }
                    }
                    else if (deallExpiration.ExpirationTypeId == 2)
                    {
                        //instore redemption 
                        var item = new InStoreRedemptionTokens();
                        item.StartDate = deallExpiration.StartDate.Value;
                        item.ExpiredDate = deallExpiration.ExpiredDate.Value;
                        item.ProductId = productId;
                        item.IsActivated = true;
                        item.IsRedeemed = false;
                        item.MasterMemberProfileId = masterMemberProfile;
                        item.MerchantId = product.MerchantId;
                        item.OutletId = null;
                        item.ProductTitle = product.Title;
                        item.OrderItemId = orderItemId;
                        item.RedeemedAt = null;
                        item.CreatedAt = DateTime.Now;
                        item.RedemptionInfo = product.RedemptionInfo;
                        item.Revenue = itemPrice;// revenue;
                        item.Token = "";
                        item.Email = email;
                        _rewardsDBContext.InStoreRedemptionTokens.Add(item);
                        _rewardsDBContext.SaveChanges();


                        if (product.ThirdPartyTypeId != null)
                        {
                            //var result = await ThirdPartyRedemption(product);

                            //if (result != null)
                            //{
                            //    if (result.IsSuccessful)
                            //    {
                            //        item.Token = result.Url;
                            //        item.TokenType = 2;
                            //        _rewardsDBContext.SaveChanges();

                            //        var sendGridClient2 = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                            //        var msg2 = new SendGridMessage();
                            //        msg2.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.ThirdPartyRedemption);
                            //        msg2.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                            //        msg2.SetSubject("Vodus Redemption Url");
                            //        msg2.AddTo(new EmailAddress(email));

                            //        msg2.AddSubstitution("-customerName-", fullName);
                            //        msg2.AddSubstitution("-item-", product.Title);
                            //        msg2.AddSubstitution("-redemptionUrl-", result.Url);
                            //        msg2.AddSubstitution("-imageUrl-", product.ImageFolderUrl);
                            //        //var response2 = sendGridClient2.SendEmailAsync(msg2).Result;
                            //        return msg2;
                            //    }
                            //}
                            //return null;
                        }
                        else
                        {
                            item.Token = Voupon.Common.RedemptionTokenGenerator.InStoreRedemptionToken.GenerateToken(item.Id);
                            _rewardsDBContext.SaveChanges();


                            var url = appSettings.Value.App.BaseUrl + "/order/store-redemption/" + item.Id;

                            var image = GenerateQRCodeHtmlImageSrc(appSettings.Value.App.VouponMerchantAppBaseUrl + "/qr/v/" + item.Token, 200, 200, 0);

                            msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.InStoreRedemption);
                            msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                            msg.SetSubject("Vodus InStore Redemption");
                            msg.AddTo(new EmailAddress(email));
                            msg.AddTo("merchant@vodus.my");
                            msg.AddSubstitution("-productTitle-", product.Title);
                            msg.AddSubstitution("-voucherCode-", item.Token);
                            msg.AddSubstitution("-voucherUrl-", "");
                            msg.AddSubstitution("-redemptionFlow-", product.RedemptionInfo);
                            return msg;
                        }

                    }
                    else if (deallExpiration.ExpirationTypeId == 4)
                    {
                        //digital redemption
                        var item = new DigitalRedemptionTokens();
                        item.CreatedAt = DateTime.Now;
                        item.ProductId = productId;
                        item.StartDate = deallExpiration.StartDate.Value;
                        item.ExpiredDate = deallExpiration.ExpiredDate.Value;
                        item.IsActivated = true;
                        item.IsRedeemed = false;
                        item.MasterMemberProfileId = masterMemberProfile;
                        item.MerchantId = product.MerchantId;
                        item.ProductTitle = product.Title;
                        item.OrderItemId = orderItemId;
                        item.RedeemedAt = null;
                        item.RedemptionInfo = product.RedemptionInfo;
                        item.Revenue = itemPrice;// revenue;                       
                        item.Token = "";
                        item.Email = email;

                        _rewardsDBContext.DigitalRedemptionTokens.Add(item);
                        _rewardsDBContext.SaveChanges();

                        return null;
                    }
                    else if (deallExpiration.ExpirationTypeId == 5)
                    {
                        //delivery redemption
                        var item = new DeliveryRedemptionTokens();
                        item.CreatedAt = DateTime.Now;
                        item.ProductId = productId;
                        item.ExpiredDate = null;
                        item.IsActivated = true;
                        item.IsRedeemed = false;
                        item.MasterMemberProfileId = masterMemberProfile;
                        item.MerchantId = product.MerchantId;
                        item.ProductTitle = product.Title;
                        item.OrderItemId = orderItemId;
                        item.RedeemedAt = null;
                        item.RedemptionInfo = product.RedemptionInfo;
                        item.Revenue = itemPrice;// revenue;
                        item.StartDate = null;
                        item.CourierProvider = "";
                        item.Token = "";
                        item.Email = email;
                        _rewardsDBContext.DeliveryRedemptionTokens.Add(item);
                        _rewardsDBContext.SaveChanges();
                        return null;
                    }
                }
                return null;
            }

            private bool IsResponseSignatureValid(string signature, string merchantCode, int paymentId, string refNo, string amount, string currency, string status)
            {
                string inputValue = appSettings.Value.PaymentGateways.Ipay88.MerchantKey
                                  + merchantCode
                                  + paymentId
                                  + refNo
                                  + decimal.Parse(amount).ToString("F").Replace(".", "").Replace(",", "")
                                  + "MYR"
                                  + status;

                if (signature == SHA_256.GenerateSHA256String(inputValue))
                {
                    return true;
                }
                return false;
            }

            private async Task<GifteeResponseModel> ThirdPartyRedemption(Voupon.Database.Postgres.RewardsEntities.Products product)
            {
                var type = await _rewardsDBContext.ThirdPartyTypes.Where(x => x.Id == product.ThirdPartyTypeId).FirstOrDefaultAsync();

                if (type.Name.ToUpper() == "GIFTEE")
                {
                    var thirdPartyProduct = await _rewardsDBContext.ThirdPartyProducts.Where(x => x.Id == product.ThirdPartyProductId).FirstOrDefaultAsync();
                    if (thirdPartyProduct != null)
                    {
                        using (var client = new HttpClient())
                        {
                            client.BaseAddress = new Uri(appSettings.Value.ThirdPartyRedemptions.Giftee.Url);
                            client.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer", appSettings.Value.ThirdPartyRedemptions.Giftee.Token);

                            var request = new GifteeRequestModel
                            {
                                campaign = appSettings.Value.ThirdPartyRedemptions.Giftee.CampaignName,
                                distributor = appSettings.Value.ThirdPartyRedemptions.Giftee.Distributor,
                                item_id = int.Parse(thirdPartyProduct.ExternalId),
                                request_code = DateTime.Now.Ticks.ToString()
                            };

                            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                            var result = client.PostAsync(appSettings.Value.ThirdPartyRedemptions.Giftee.Url, content).Result;
                            string resultContent = result.Content.ReadAsStringAsync().Result;

                            dynamic testResult = JsonConvert.DeserializeObject(resultContent);

                            var gifteeResponseModel = new GifteeResponseModel();

                            if (testResult["egift_url"] != null)
                            {
                                var totalUrl = testResult["egift_url"].Count;
                                if (totalUrl > 0)
                                {
                                    gifteeResponseModel.Url = testResult["egift_url"][0];
                                    gifteeResponseModel.IsSuccessful = true;
                                }
                                gifteeResponseModel.RawResponse = testResult["egift_url"].ToString();
                                return gifteeResponseModel;
                            }
                            else
                            {
                                gifteeResponseModel.IsSuccessful = false;
                                gifteeResponseModel.Error = testResult["error_code"];
                                return gifteeResponseModel;
                            }
                        }
                    }
                }
                return null;
            }

        }

        public class GifteeResponseModel
        {
            public bool IsSuccessful { get; set; }
            public string Url { get; set; }

            public string RawResponse { get; set; }
            public string Error { get; set; }
        }

        public class GifteeRequestModel
        {
            public string campaign { get; set; }
            public string distributor { get; set; }

            public int item_id { get; set; }

            public string request_code { get; set; }
        }


        private static string GenerateQRCodeHtmlImageSrc(string token, int height, int width, int margin)
        {
            string imageSrc = "";
            var qrCodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = margin
                }
            };
            var pixelData = qrCodeWriter.Write(token);

            using (var bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            using (var ms = new MemoryStream())
            {
                var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                try
                {
                    // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image    
                    System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                // save to stream as PNG    
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                imageSrc = String.Format("data:image/png;base64,{0}", Convert.ToBase64String(ms.ToArray()));
            }
            return imageSrc;

        }


        public class PaymentResponseViewModel
        {
            public int AvailablePoints { get; set; }
            public bool UpdatePoints { get; set; }

        }
    }
}
