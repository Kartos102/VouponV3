using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Common.Services.Utility.Queries;
using Voupon.Merchant.WebApp.Infrastructures.Helpers;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Orders.Commands
{
    public class CreateOrderFullRefundCommand : IRequest<ApiResponseViewModel>
    {
        public Guid UserId { get; set; }
        public int MerchantId { get; set; }
        public bool IsExternal { get; set; }
        public bool IsAdminForMerchantRefund { get; set; }
        public Guid OrderId { get; set; }
        public string Remark { get; set; }

        public short RefundType { get; set; }

        public decimal RefundAmount { get; set; }

        public class CreateOrderFullRefundCommandHandler : IRequestHandler<CreateOrderFullRefundCommand, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            IOptions<AppSettings> appSettings;
            public CreateOrderFullRefundCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(CreateOrderFullRefundCommand request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                var order = await rewardsDBContext.Orders.Where(x => x.Id == request.OrderId).FirstOrDefaultAsync();

                try
                {
                    if (order == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [001]";
                        return apiResponseViewModel;
                    }

                    //  Only check for payment if order amount is more than 0
                    var orderPayment = await rewardsDBContext.OrderPayments.Where(x => x.RefNo == order.Id).FirstOrDefaultAsync();

                    if (orderPayment == null)
                    {
                        if (order.TotalPrice > 0)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Invalid request [002]";
                            return apiResponseViewModel;
                        }
                    }



                    var user = await vodusV2Context.Users.Include(x => x.MasterMemberProfiles).Where(x => x.Email == order.Email).FirstOrDefaultAsync();
                    if (user == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [003]";
                        return apiResponseViewModel;
                    }

                    if (!user.MasterMemberProfiles.Any())
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [004]";
                        return apiResponseViewModel;
                    }

                    var orderItemExternal = await rewardsDBContext.OrderShopExternal.Include(x => x.OrderItemExternal).Where(x => x.OrderId == request.OrderId).ToListAsync();
                    var orderItem = await rewardsDBContext.OrderItems.Where(x => x.OrderId == request.OrderId).ToListAsync();

                    if (orderItemExternal == null && orderItem == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request [005]";
                        return apiResponseViewModel;
                    }


                    var hasError = false;

                    var master = user.MasterMemberProfiles.First();
                    var externalRefundList = new List<RefundsExternalOrderItems>();
                    var internalRefundList = new List<Refunds>();

                    if (orderItemExternal != null && request.IsExternal)
                    {
                        foreach (var item in orderItemExternal.SelectMany(x => x.OrderItemExternal))
                        {
                            var existingRefundItem = await rewardsDBContext.Refunds.Where(x => x.OrderItemId == item.Id).FirstOrDefaultAsync();

                            if (existingRefundItem != null)
                            {
                                hasError = true;
                                break;
                            }

                            item.OrderItemExternalStatus = 5;
                            externalRefundList.Add(new RefundsExternalOrderItems
                            {
                                Id = Guid.NewGuid(),
                                CreatedBy = request.UserId,
                                CreatedAt = DateTime.Now,
                                MasterMemberProfileId = user.MasterMemberProfiles.First().Id,
                                ShortId = DateTime.Now.ToString("dd") + item.Id.ToString().Split("-")[0],
                                Status = 1,
                                Type = request.RefundType,
                                PointsRefunded = item.Points,
                                MoneyRefunded = item.TotalPrice,
                                Remark = (string.IsNullOrEmpty(request.Remark) ? null : ""),
                                RefundMethod = 2,
                                OrderItemId = item.Id,
                            });
                        }
                        if (hasError)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Invalid request. Refund is already made for this item or its in progress [004]";
                            return apiResponseViewModel;
                        }
                    }

                    if (orderItem != null && !request.IsExternal && !request.IsAdminForMerchantRefund)
                    {
                        foreach (var item in orderItem)
                        {
                            var existingRefundItem = await rewardsDBContext.Refunds.Where(x => x.OrderItemId == item.Id).FirstOrDefaultAsync();

                            if (existingRefundItem != null)
                            {
                                hasError = true;
                                break;
                            }

                            item.Status = 8;
                            internalRefundList.Add(new Refunds
                            {
                                Id = Guid.NewGuid(),
                                CreatedBy = request.UserId,
                                CreatedAt = DateTime.Now,
                                MasterMemberProfileId = user.MasterMemberProfiles.First().Id,
                                ShortId = DateTime.Now.ToString("dd") + item.Id.ToString().Split("-")[0],
                                Status = 1,
                                Type = request.RefundType,
                                PointsRefunded = item.Points,
                                MoneyRefunded = item.Price,
                                Remark = (string.IsNullOrEmpty(request.Remark) ? null : ""),
                                RefundMethod = 2,
                                OrderItemId = item.Id,
                            });
                        }
                        if (hasError)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Invalid request. Refund is already made for this item or its in progress [004]";
                            return apiResponseViewModel;
                        }

                    }
                    else if (orderItem != null && !request.IsExternal && request.IsAdminForMerchantRefund)
                    {
                        foreach (var item in orderItem)
                        {
                            var existingRefundItem = await rewardsDBContext.Refunds.Where(x => x.OrderItemId == item.Id).FirstOrDefaultAsync();

                            if (existingRefundItem == null)
                            {
                                hasError = true;
                                break;
                            }

                            //item.Status = 8;
                            //internalRefundList.Add(existingRefundItem);
                        }
                        if (hasError)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Invalid request. Refund data is missing [005]";
                            return apiResponseViewModel;
                        }
                    }
                    rewardsDBContext.Refunds.AddRange(internalRefundList);
                    await rewardsDBContext.SaveChangesAsync();

                    rewardsDBContext.Database.BeginTransaction();
                    vodusV2Context.Database.BeginTransaction();

                    //  Handle points only orders
                    if (order.TotalPrice == 0 && order.TotalPoints > 0)
                    {
                        await UpdatePointsAndEmailUser(order, orderItemExternal, orderItem, master, externalRefundList, internalRefundList, "", "", true);
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Message = "Order have been refunded";
                        return apiResponseViewModel;
                    }

                    if (!request.IsAdminForMerchantRefund && !request.IsExternal)
                    {
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Message = "The refund is in process.";
                        return apiResponseViewModel;
                    }

                    // Checking if not full refund
                    if (request.IsExternal && orderItem != null && orderItem.Count > 0)
                    {
                        await UpdatePointsAndEmailUser(order, orderItemExternal, orderItem, master, externalRefundList, internalRefundList, "", "", false);
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Can not make automatic refund. Please do a manual refund.[1]";
                        return apiResponseViewModel;
                    }

                    if (!request.IsExternal && ((orderItem != null && orderItem.Where(x => x.MerchantId != request.MerchantId).ToList().Count > 0) && (orderItemExternal != null && orderItemExternal.Count > 0)) && request.IsAdminForMerchantRefund)
                    {
                        await UpdatePointsAndEmailUser(order, orderItemExternal, orderItem, master, externalRefundList, internalRefundList, "", "", false);
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Can not make automatic refund. Please do a manual refund.[2]";
                        return apiResponseViewModel;
                    }

                    // To remove when we won't have limited time for full refund
                    if (request.IsExternal && !(order.CreatedAt > DateTime.Now.AddHours(-24) && order.CreatedAt < DateTime.Now))
                    {
                        await UpdatePointsAndEmailUser(order, orderItemExternal, orderItem, master, externalRefundList, internalRefundList, "", "", false);
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Can not make automatic refund. Please do a manual refund.[3]";
                        return apiResponseViewModel;
                    }

                    if (!request.IsExternal && !(order.CreatedAt > DateTime.Now.AddHours(-24) && order.CreatedAt < DateTime.Now) && request.IsAdminForMerchantRefund)
                    {
                        await UpdatePointsAndEmailUser(order, orderItemExternal, orderItem, master, externalRefundList, internalRefundList, "", "", false);
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Can not make automatic refund. Please do a manual refund.[4]";
                        return apiResponseViewModel;
                    }


                    if (orderPayment != null)
                    {
                        //  RevenueMonster refund
                        var grantType = new RevenueMonsterGrantType
                        {
                            GrantType = "client_credentials"
                        };

                        var httpClient = new HttpClient();
                        httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{appSettings.Value.PaymentGateways.RevenueMonster.ClientId}:{appSettings.Value.PaymentGateways.RevenueMonster.ClientSecret}"))}");
                        var tokenData = new StringContent(JsonConvert.SerializeObject(grantType), Encoding.UTF8, "application/json");
                        var tokenResponse = await httpClient.PostAsync($"{appSettings.Value.PaymentGateways.RevenueMonster.AuthUrl}/v1/token", tokenData);
                        string tokenResult = tokenResponse.Content.ReadAsStringAsync().Result;

                        dynamic dynamicTokenResult = JsonConvert.DeserializeObject<ExpandoObject>(tokenResult);
                        var accessToken = dynamicTokenResult.accessToken;

                        var privateKey = appSettings.Value.PaymentGateways.RevenueMonster.PrivateKey;

                        var refundViewModel = new RefundViewModel
                        {
                            Reason = request.Remark,
                            TransactionId = orderPayment.TransactionId,
                            Refund = new Refund
                            {
                                Amount = (int)((order.TotalPrice + order.TotalShippingCost) * 100),
                                Type = "FULL",
                                CurrencyType = "MYR"
                            }
                        };

                        string compactJson = SignatureUtil.GenerateCompactJson(refundViewModel);
                        string method = "post";
                        string nonceStr = RandomString.GenerateRandomString(32);
                        var requestURL = $"{appSettings.Value.PaymentGateways.RevenueMonster.ApiUrl}/v3/payment/refund";
                        string signType = "sha256";
                        string timestamp = Convert.ToString((Int32)(DateTime.Now.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
                        RevenueMonsterSignature signature = new RevenueMonsterSignature();
                        string signatureResult = "";
                        signatureResult = signature.GenerateSignature(compactJson, method, nonceStr, privateKey, requestURL, signType, timestamp);
                        signatureResult = "sha256 " + signatureResult;

                        //RevenueMonsterRefundResponseViewModel
                        //var content = JsonConvert.SerializeObject(data);
                        var buffer = System.Text.Encoding.UTF8.GetBytes(compactJson);
                        var byteContent = new ByteArrayContent(buffer);
                        byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        HttpClient client = new HttpClient();
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        client.DefaultRequestHeaders.Add("X-Nonce-Str", nonceStr);
                        client.DefaultRequestHeaders.Add("X-Signature", signatureResult);
                        client.DefaultRequestHeaders.Add("X-Timestamp", timestamp);

                        var stringContent = new StringContent(JsonConvert.SerializeObject(compactJson), Encoding.UTF8, "application/json");

                        var response = await client.PostAsync(requestURL, byteContent);
                        var responseStr = await response.Content.ReadAsStringAsync();

                        var jsonAsString = await response.Content.ReadAsStringAsync();
                        dynamic refundResult = JsonConvert.DeserializeObject<ExpandoObject>(jsonAsString);

                        if (((IDictionary<string, object>)refundResult).ContainsKey("error"))
                        {
                            rewardsDBContext.Database.RollbackTransaction();
                            vodusV2Context.Database.RollbackTransaction();

                            //apiResponseViewModel.Message = refundResult.error.message;
                            apiResponseViewModel.Message = "Can not make automatic refund. Plrease do a manual refund.[5]";
                            apiResponseViewModel.Successful = false;
                            return apiResponseViewModel;
                        }

                        await UpdatePointsAndEmailUser(order, orderItemExternal, orderItem, master, externalRefundList, internalRefundList, jsonAsString, refundResult, true);
                    }




                    apiResponseViewModel.Message = "Succesfully refunded order item: #" + order.ShortId;
                    apiResponseViewModel.Successful = true;
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    rewardsDBContext.Database.RollbackTransaction();
                    vodusV2Context.Database.RollbackTransaction();

                    var errorLogs = new ErrorLogs
                    {
                        TypeId = 1,
                        ActionRequest = JsonConvert.SerializeObject(request),
                        ActionName = "CreateOrderFullRefundCommand",
                        CreatedAt = DateTime.Now,
                        Errors = ex.ToString(),
                        Email = order.Email,
                        MasterProfileId = order.MasterMemberProfileId
                    };

                    rewardsDBContext.ErrorLogs.Add(errorLogs);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to issue refund [099]";
                    return apiResponseViewModel;
                }

                async Task UpdatePointsAndEmailUser(Voupon.Database.Postgres.RewardsEntities.Orders order, List<OrderShopExternal> orderItemExternal, List<Voupon.Database.Postgres.RewardsEntities.OrderItems> orderItem, MasterMemberProfiles master, List<RefundsExternalOrderItems> externalRefundList, List<Refunds> internalRefundList, string jsonAsString, dynamic refundResult, bool isfullRefund)
                {
                    var totalPointsRefunded = 0;
                    if (externalRefundList != null && externalRefundList.Any())
                    {
                        if (order.TotalPrice > 0)
                        {
                            foreach (var item in externalRefundList)
                            {
                                item.RefundJsonResponse = jsonAsString;
                                item.RefundTransactionId = refundResult.item.transactionId;
                            }


                            totalPointsRefunded += externalRefundList.Sum(x => x.PointsRefunded);
                            rewardsDBContext.RefundsExternalOrderItems.AddRange(externalRefundList);
                            await rewardsDBContext.SaveChangesAsync();
                        }
                    }

                    if (internalRefundList != null && internalRefundList.Any())
                    {
                        if (order.TotalPrice > 0)
                        {
                            foreach (var item in internalRefundList)
                            {
                                item.RefundJsonResponse = jsonAsString;
                                item.RefundTransactionId = refundResult.item.transactionId;
                            }


                            totalPointsRefunded += internalRefundList.Sum(x => x.PointsRefunded);
                            rewardsDBContext.Refunds.AddRange(internalRefundList);
                            await rewardsDBContext.SaveChangesAsync();
                        }
                    }

                    master.AvailablePoints = master.AvailablePoints + (externalRefundList.Sum(x => x.PointsRefunded) + internalRefundList.Sum(x => x.PointsRefunded));
                    vodusV2Context.MasterMemberProfiles.Update(master);
                    await vodusV2Context.SaveChangesAsync();

                    //  Update item status
                    if (orderItem != null && orderItem.Any())
                    {
                        if (!isfullRefund)
                        {
                            foreach (var item in orderItem)
                            {
                                // Pending for refund from admin
                                item.Status = 4;
                            }
                        }
                        else
                        {
                            foreach (var item in orderItem)
                            {
                                // Refunded
                                item.Status = 5;
                            }
                        }
                        rewardsDBContext.OrderItems.UpdateRange(orderItem);
                    }

                    if (orderItemExternal != null && orderItemExternal.Any())
                    {
                        rewardsDBContext.OrderShopExternal.UpdateRange(orderItemExternal);
                    }

                    await rewardsDBContext.SaveChangesAsync();

                    vodusV2Context.Database.CommitTransaction();
                    rewardsDBContext.Database.CommitTransaction();


                    var address = order.ShippingAddressLine1 + ", ";
                    if (order.ShippingAddressLine2 != null && order.ShippingAddressLine2 != "")
                        address += order.ShippingAddressLine2 + ", ";
                    address += order.ShippingPostcode + " ";
                    address += order.ShippingCity + " ";
                    address += order.ShippingState + ", ";
                    address += order.ShippingCountry;

                    var orderTable = $"<table border='0' cellpadding='0' cellspacing='0' width='100%' class='x_478386741tableButton'>" +
                                     "<tbody style = 'font-family:Calibri'>" +
                                     "<tr>" +
                                     "<td align='center' valign='top' style='padding-top:20px; padding-bottom:20px; width:100%;'>" +
                                     "<table style = 'width:100%; max-width :  800px; '>" +
                                     "<tbody>";

                    if (orderItem != null && orderItem.Any())
                    {
                        foreach (var item in orderItem)
                        {
                            orderTable += "<tr style = 'background-color:#eee;font-weight:bold;'>" +
                                 "<td style = 'padding: 0 15px'> Item </td>" +
                                  "<td></td>" +
                                  "<td style = 'padding:5px;text-align:center;'> Qty </td>" +
                                   "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;'> Price </td>" +
                               "</tr>" +
                               "<tr>" +
                               "<td width = '15%'>" +
                                   $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductImageFolderUrl.Replace("http://", "https://")}'>" + "</td>" +
                                       "<td style = 'padding:5px;font-size:12px;'>" +
                                       $"<a style = 'font-size:14px;color:#666;text-decoration:none;' href ='{appSettings.Value.App.VouponUrl}/Order/Refund' target = '_blank'>" +
                                               $"{item.ProductTitle}" +
                                       "</a>" +
                                   "</td> " +
                                   "<td style = 'padding:5px;text-align:center;'>1</td>" +
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
                                                     $"<td width='50%' style='text-align: right;'>{ item.Points} </td> " +
                                                  "</tr>  " +
                                                  "<tr>  " +
                                                      "<td width='50%' style='text-align:right;'> Subtotal: </td> " +
                                                      $"<td width='50%' style='text-align:right;'> RM {item.SubtotalPrice}</td>" +
                                                  "</tr> " +
                                                  "<tr style='width: 100%; vertical-align: top; '> " +
                                                      "<td width = '50%' style='text-align:right;'> Discount:</td>" +
                                                      $"<td width = '50%' style='text-align:right;'> -RM {item.DiscountedAmount}</td>" +
                                                  "</tr>  " +
                                                  "<tr style='vertical-align:top;font-weight:bold;'> " +
                                                      "<td width='50%' style='text-align:right;'> Total:</td> " +
                                                       $"<td width='50%' style='text-align:right;color:#8C37F6;'> RM {item.Price} </td>  " +
                                                  "</tr> " +
                                              "</tbody> " +
                                          " </table> " +
                                          " </td> " +
                                          " </tr> ";
                        }
                    }

                    if (orderItemExternal != null && orderItemExternal.Any())
                    {
                        foreach (var item in orderItemExternal.SelectMany(x => x.OrderItemExternal))
                        {
                            orderTable += "<tr style = 'background-color:#eee;font-weight:bold;'>" +
                                 "<td style = 'padding: 0 15px'> Item </td>" +
                                  "<td></td>" +
                                  "<td style = 'padding:5px;text-align:center;'> Qty </td>" +
                                   "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;'> Price </td>" +
                               "</tr>" +
                               "<tr>" +
                               "<td width = '15%'>" +
                                   $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{item.ProductCartPreviewSmallImageURL.Replace("http://", "https://")}'>" + "</td>" +
                                       "<td style = 'padding:5px;font-size:12px;'>" +
                                       $"<a style = 'font-size:14px;color:#666;text-decoration:none;' href ='{appSettings.Value.App.VouponUrl}/Order/Refund' target = '_blank'>" +
                                               $"{item.ProductTitle}" +
                                       "</a>" +
                                   "</td> " +
                                   "<td style = 'padding:5px;text-align:center;'>1</td>" +
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
                                                     $"<td width='50%' style='text-align: right;'>{ item.Points} </td> " +
                                                  "</tr>  " +
                                                  "<tr>  " +
                                                      "<td width='50%' style='text-align:right;'> Subtotal: </td> " +
                                                      $"<td width='50%' style='text-align:right;'> RM {item.SubtotalPrice}</td>" +
                                                  "</tr> " +
                                                  "<tr style='width: 100%; vertical-align: top; '> " +
                                                      "<td width = '50%' style='text-align:right;'> Discount:</td>" +
                                                      $"<td width = '50%' style='text-align:right;'> -RM {item.DiscountedAmount}</td>" +
                                                  "</tr>  " +
                                                  "<tr style='vertical-align:top;font-weight:bold;'> " +
                                                      "<td width='50%' style='text-align:right;'> Total:</td> " +
                                                       $"<td width='50%' style='text-align:right;color:#8C37F6;'> RM {item.Price} </td>  " +
                                                  "</tr> " +
                                              "</tbody> " +
                                          " </table> " +
                                          " </td> " +
                                          " </tr> ";
                        }

                    }
                    orderTable += " </tbody> " +
         "</table> " +
     "</td> " +
 "</tr> " +
"</tbody> " +
"</table> ";

                    //  Send refund notification to user
                    var from = new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus Support");
                    var to = new EmailAddress(order.Email, order.BillingPersonFirstName + " " + order.BillingPersonLastName);
                    var sendgridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                    var msg = new SendGridMessage();
                    msg.From = from;
                    msg.TemplateId = appSettings.Value.Mailer.Sendgrid.Templates.RefundToBuyer;
                    msg.Personalizations = new System.Collections.Generic.List<Personalization>();
                    var personalization = new Personalization();
                    personalization.Substitutions = new Dictionary<string, string>();
                    personalization.Substitutions.Add("-name-", order.BillingPersonFirstName + " " + order.BillingPersonLastName);
                    personalization.Substitutions.Add("-date-", order.CreatedAt.ToString("dd MMM yyyy"));
                    personalization.Substitutions.Add("-orderTable-", orderTable);
                    personalization.Subject = "Vodus order item refund #" + order.ShortId;
                    personalization.Tos = new List<EmailAddress>();
                    personalization.Tos.Add(to);
                    msg.Personalizations.Add(personalization);
                    var sendEmailREsponse = await sendgridClient.SendEmailAsync(msg);
                }
            }
        }

        public class RefundViewModel
        {
            public string TransactionId { get; set; }
            public string Reason { get; set; }
            public Refund Refund { get; set; }

        }

        public class Refund
        {
            public string Type { get; set; }
            public string CurrencyType { get; set; }
            public int Amount { get; set; }
        }

        private class RevenueMonsterGrantType
        {
            public string GrantType { get; set; }
        }


    }

}
