using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Areas.Admin.Services.EmailBlast.ViewModels;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.SendInvouicesEmailCommand.Commands
{
    public class SendInvouicesEmailCommand : IRequest<ApiResponseViewModel>
    {
       public SendEmailViewModel EmailViewModel { get; set; }
       public int StartMonth { get; set; }
       public int EndMonth{ get; set; }

        public class SendInvouicesEmailCommandHandler : IRequestHandler<SendInvouicesEmailCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            private readonly VodusV2Context vodusV2Context;
            private readonly IOptions<AppSettings> appSettings;


            public SendInvouicesEmailCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(SendInvouicesEmailCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    //var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);

                    var orders = rewardsDBContext.Orders.AsNoTracking().Include(x => x.OrderItems).Include(x => x.OrderShopExternal).ThenInclude(x => x.OrderItemExternal).Where(x => x.OrderStatus == 2 && x.CreatedAt >= new DateTime(2021, request.StartMonth, 1) && x.CreatedAt < new DateTime(2021, request.EndMonth, 1)).OrderBy(x=> x.CreatedAt).ToListAsync();

                    //if (order == null)
                    //{
                    //    apiResponseViewModel.Message = "Invalid order";
                    //    apiResponseViewModel.Successful = false;

                    //    var errorLogs = new Database.Postgres.RewardsEntities.ErrorLogs
                    //    {
                    //        ActionName = "CreatePaymentCommand",
                    //        Errors = "Invalid order. Order is null",
                    //        TypeId = CreateErrorLogCommand.Type.Service,
                    //        ActionRequest = JsonConvert.SerializeObject(request),
                    //        CreatedAt = DateTime.Now
                    //    };

                    //    await rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                    //    await rewardsDBContext.SaveChangesAsync();
                    //    rewardsDBContext.Database.CommitTransaction();
                    //    return apiResponseViewModel;
                    //}

                    //vodusV2Context.Database.BeginTransaction();
                    //var masterMemberProfile = vodusV2Context.MasterMemberProfiles.Include(x => x.User).Where(x => x.Id == order.MasterMemberProfileId).FirstOrDefault();
                    //if (masterMemberProfile == null)
                    //{
                    //    await new Logs
                    //    {
                    //        Description = "Failed to create payment, invalid master memberprofile",
                    //        Email = "",
                    //        JsonData = JsonConvert.SerializeObject(request),
                    //        ActionName = "CreatePaymentCommand",
                    //        TypeId = CreateErrorLogCommand.Type.Service,
                    //        SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                    //        RewardsDBContext = rewardsDBContext
                    //    }.Error();

                    //    rewardsDBContext.Database.CommitTransaction();
                    //    vodusV2Context.Database.RollbackTransaction();

                    //    apiResponseViewModel.Message = "Invalid request";
                    //    apiResponseViewModel.Successful = false;
                    //    return apiResponseViewModel;
                    //}

                    //firstName = masterMemberProfile.User.FirstName;
                    //lastName = masterMemberProfile.User.LastName;
                    //_email = masterMemberProfile.User.Email;

                    var itemPrice = "";
                    var discountedAmount = "0";
                    var subTotal = "";

                    //  Create Order Payments
                    //rewardsDBContext.OrderPayments.Add(new OrderPayments
                    //{
                    //    Id = Guid.NewGuid(),
                    //    Amount = request.Amount,
                    //    AuthCode = request.AuthCode,
                    //    CreatedAt = DateTime.Now,
                    //    CreditCardBankName = (request.CreditCardBankName != null ? request.CreditCardBankName : null),
                    //    CreditCardName = (request.CreditCardName != null ? request.CreditCardName : null),
                    //    CreditCardNumber = (request.CreditCardNumber != null ? request.CreditCardNumber : null),
                    //    CreditCardIssuingCountry = (request.CreditCardIssuingCountry != null ? request.CreditCardIssuingCountry : null),
                    //    Currency = request.Currency,
                    //    MerchantCode = request.MerchantCode,
                    //    PaymentId = request.PaymentId,
                    //    PaymentProviderId = request.PaymentProviderId,
                    //    RefNo = request.RefNo,
                    //    Remark = (request.Remark != null ? request.Remark : null),
                    //    Signature = request.Signature,
                    //    Status = request.Status,
                    //    TransactionId = request.TransactionId,
                    //    ErrorDescription = (request.ErrorDescription != null ? request.ErrorDescription : null),
                    //    JsonResponse = request.JsonResponse,
                    //});

                    //itemPrice = request.Amount.ToString();

                    //order.OrderStatus = 2;

                    //  Deduct points if any
                    //  Points is already deducted during order creation. Will refund points back to user when they cancel
                    //  or during order cancellation scheduler 
                    //if (order.TotalPoints > 0)
                    //{
                    //    if (masterMemberProfile.AvailablePoints < order.TotalPoints)
                    //    {
                    //        apiResponseViewModel.Message = "Not enough VPoints to complete transactions";
                    //        apiResponseViewModel.Successful = false;

                    //        var errorLogs = new Database.Postgres.RewardsEntities.ErrorLogs
                    //        {
                    //            ActionName = "CreatePaymentCommand",
                    //            Errors = "Not enough VPoints to complete transactions",
                    //            TypeId = CreateErrorLogCommand.Type.Service,
                    //            ActionRequest = JsonConvert.SerializeObject(request),
                    //            CreatedAt = DateTime.Now,
                    //            MasterProfileId = order.MasterMemberProfileId
                    //        };

                    //        await rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                    //        await rewardsDBContext.SaveChangesAsync();
                    //        rewardsDBContext.Database.CommitTransaction();
                    //        vodusV2Context.Database.RollbackTransaction();

                    //        return apiResponseViewModel;
                    //    }

                    //    masterMemberProfile.AvailablePoints = masterMemberProfile.AvailablePoints - order.TotalPoints;
                    //    vodusV2Context.MasterMemberProfiles.Update(masterMemberProfile);
                    //    await vodusV2Context.SaveChangesAsync();

                    //    viewModel.UpdatePoints = true;
                    //    viewModel.AvailablePoints = masterMemberProfile.AvailablePoints;
                    //}

                    //rewardsDBContext.Orders.Update(order);
                    List<int[]> orderArraysQuantitiesList = new List<int[]>();

                    foreach (var order in orders.Result)
                    {
                        var orderItemsList = order.OrderItems.ToList();
                        int[] productsQuantities = new int[orderItemsList.Count];

                        for (int i = 0; i < orderItemsList.Count; i++)
                        {
                            productsQuantities[i]++;
                            if (i+1 != orderItemsList.Count)
                            {
                                if (orderItemsList[i + 1].ProductId == orderItemsList[i].ProductId && orderItemsList[i + 1].Price == orderItemsList[i].Price && orderItemsList[i + 1].DiscountedAmount == orderItemsList[i].DiscountedAmount)
                                {
                                    orderItemsList.RemoveAt(i + 1);
                                    //productsQuantities[i]++;
                                    i--;
                                }
                            }
                        }
                        order.OrderItems = orderItemsList;
                        orderArraysQuantitiesList.Add(productsQuantities);
                    }
                    //  Generate Token
                    var orderReceiptView = "";
                    int k = 0;

                    foreach (var order in orders.Result)
                    {
                        int j = 0;
                        foreach (var item in order.OrderItems)
                        {
                            int quantity = orderArraysQuantitiesList[k][j];
                            var address = order.ShippingAddressLine1 + ", ";
                            if (order.ShippingAddressLine2 != null && order.ShippingAddressLine2 != "")
                                address += order.ShippingAddressLine2 + ", ";
                            address += order.ShippingPostcode + " ";
                            address += order.ShippingCity + " ";
                            address += order.ShippingState + ", ";
                            address += order.ShippingCountry;

                            //orderReceiptView += "<h3>Your Order #" + order.ShortId + " has been placed on " + order.CreatedAt.ToString("dd/MM/yyyy");

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

                            //  masterMemberProfile.User.Email
                            //var tokenResult = await CreateToken(item.Id, item.ProductId, masterMemberProfile.Id, masterMemberProfile.User.Email, masterMemberProfile.User.FirstName + " " + masterMemberProfile.User.LastName);
                            //if (tokenResult != null)
                            //{
                            //    sendgridMessageList.Add(tokenResult);
                            //}
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
                                if (isShippingCostIncludedForShop)
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
                        string firstName = "";
                        string lastName = "";
                        var masterMemberProfile = await vodusV2Context.MasterMemberProfiles.AsNoTracking().Include(x => x.User).Where(x => x.Id == order.MasterMemberProfileId).FirstOrDefaultAsync();
                        if (masterMemberProfile != null)
                        {
                            firstName = masterMemberProfile.User.FirstName;
                            lastName = masterMemberProfile.User.LastName;
                        }

                        var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                        var msg = new SendGridMessage();
                        msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.OrderConfirmation);
                        msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                        msg.SetSubject("Order Invoice " + order.CreatedAt.ToString("dd/MM/yyyy"));
                        msg.AddTo(new EmailAddress(appSettings.Value.Emails.Receipt));
                        msg.AddSubstitution("-customerName-", firstName + " " + lastName);
                        msg.AddSubstitution("-orderNumber-", "#" + order.ShortId);
                        msg.AddSubstitution("-orderDate-", order.CreatedAt.ToString("dd/MM/yyyy"));
                        msg.AddSubstitution("-orderTable-", orderReceiptView);
                        var response = sendGridClient.SendEmailAsync(msg).Result;
                        orderReceiptView = "";
                        k++;
                    }

                    //await rewardsDBContext.SaveChangesAsync();

                    //// Update product quantity
                    //foreach (var item in order.OrderItems)
                    //{
                    //    var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == item.ProductId);

                    //    if (!item.IsVariationProduct)
                    //    {
                    //        product.AvailableQuantity--;
                    //        //if(product.AvailableQuantity == 0)
                    //        //{
                    //        //   //Update all cart products to OutOfStock
                    //        //}
                    //    }
                    //    else
                    //    {
                    //        var productVariation = await rewardsDBContext.ProductVariation.FirstOrDefaultAsync(x => x.Id == item.VariationId);
                    //        productVariation.AvailableQuantity--;
                    //        //if(productVariation.AvailableQuantity == 0)
                    //        //{
                    //        //   //Update all cart products to OutOfStock
                    //        //}
                    //        rewardsDBContext.ProductVariation.Update(productVariation);
                    //    }
                    //    product.TotalBought++;
                    //    rewardsDBContext.Products.Update(product);
                    //}

                    //await rewardsDBContext.SaveChangesAsync();
                    //rewardsDBContext.Database.CommitTransaction();
                    //vodusV2Context.Database.CommitTransaction();

                    //foreach (var item in order.OrderItems)
                    //{
                    //    //  Generate notification email to merchant 
                    //    var merchantEmail = rewardsDBContext.UserRoles.Include(x => x.User).Where(x => x.MerchantId == item.MerchantId && x.RoleId == new Guid("1A436B3D-15A0-4F03-8E4E-0022A5DD5736")).FirstOrDefault().User.Email;
                    //    var sendGridClientForMerchant = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                    //    var msgForMerchant = new SendGridMessage();
                    //    //  In Store
                    //    if (item.ExpirationTypeId == 2)
                    //    {
                    //        msgForMerchant.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.InStoreMerchantSalesNotificationEmail);
                    //    }
                    //    // Digital
                    //    else if (item.ExpirationTypeId == 4)
                    //    {
                    //        msgForMerchant.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.DigitalMerchantSalesNotificationEmail);
                    //    }
                    //    // Delivery
                    //    else if (item.ExpirationTypeId == 5)
                    //    {
                    //        msgForMerchant.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.DeliveryMerchantSalesNotificationEmail);
                    //        msgForMerchant.AddSubstitution("-name-", masterMemberProfile.User.FirstName + " " + masterMemberProfile.User.LastName);
                    //        if (order.ShippingAddressLine2 != null || order.ShippingAddressLine2 != "")
                    //        {
                    //            msgForMerchant.AddSubstitution("-address-", order.ShippingAddressLine1 + ", " + order.ShippingAddressLine2 + ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry);
                    //        }
                    //        else
                    //        {
                    //            msgForMerchant.AddSubstitution("-address-", order.ShippingAddressLine1 + ", " + order.ShippingCity + ", " + order.ShippingPostcode + ", " + order.ShippingState + ", " + order.ShippingCountry);
                    //        }
                    //        msgForMerchant.AddSubstitution("-phone-", "+(" + order.ShippingMobileCountryCode + ")" + order.ShippingMobileNumber);
                    //    }

                    //    msgForMerchant.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                    //    msgForMerchant.SetSubject("Vodus - New order");
                    //    msgForMerchant.AddTo(new EmailAddress(merchantEmail));
                    //    msgForMerchant.AddSubstitution("-orderTable-", orderReceiptView);
                    //    var responseMerchant = sendGridClientForMerchant.SendEmailAsync(msgForMerchant).Result;
                    //}

                    //  Once transaction is completed. Send email
                    //  Generate Receipt
                    //var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                    //var msg = new SendGridMessage();
                    //msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.OrderConfirmation);
                    //msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                    //msg.SetSubject("Orders Invoices");
                    //msg.AddTo(new EmailAddress("belal@vodus.my"));
                    ////msg.AddSubstitution("-customerName-", firstName + " " + lastName);
                    ////msg.AddSubstitution("-orderNumber-", "#" + order.ShortId);
                    ////msg.AddSubstitution("-orderDate-", order.CreatedAt.ToString("dd/MM/yyyy"));
                    //msg.AddSubstitution("-orderTable-", orderReceiptView);
                    //var response = sendGridClient.SendEmailAsync(msg).Result;

                    //  Send itemized 
                    //if (sendgridMessageList != null && sendgridMessageList.Any())
                    //{
                    //    foreach (var tokenMessage in sendgridMessageList)
                    //    {
                    //        msg = tokenMessage;
                    //        var resp = sendGridClient.SendEmailAsync(msg).Result;
                    //    }
                    //}

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Emails sent successfully";
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to send Emails [0001]";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }

    }
}
