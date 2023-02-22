using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Common.Services.Utility.Queries;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Orders.Commands
{
    public class CreateOrderItemRefundCommand : IRequest<ApiResponseViewModel>
    {
        public Guid UserId { get; set; }
        public int MerchantId { get; set; }
        public Guid OrderItemId { get; set; }
        public string Remark { get; set; }

        public byte RefundType { get; set; }

        public decimal RefundAmount { get; set; }

        public class CreateOrderItemRefundCommandHandler : IRequestHandler<CreateOrderItemRefundCommand, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            VodusV2Context vodusV2Context;
            IOptions<AppSettings> appSettings;
            public CreateOrderItemRefundCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(CreateOrderItemRefundCommand request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();

                var orderItem = await rewardsDBContext.OrderItems.Include(x => x.Order).Where(x => x.Id == request.OrderItemId).FirstOrDefaultAsync();
                if (orderItem == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request [001]";
                    return apiResponseViewModel;
                }

                var user = await vodusV2Context.Users.Include(x => x.MasterMemberProfiles).Where(x => x.Email == orderItem.Order.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request [002]";
                    return apiResponseViewModel;
                }

                if (!user.MasterMemberProfiles.Any())
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request [003]";
                    return apiResponseViewModel;
                }

                var master = user.MasterMemberProfiles.First();

                var existingRefunds = await rewardsDBContext.Refunds.Where(x => x.OrderItemId == request.OrderItemId).FirstOrDefaultAsync();

                if(existingRefunds != null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request. Refund is already made for this item or its in progress [004]";
                    return apiResponseViewModel;
                }

                if(orderItem.Price> 0)
                {
                    if(request.RefundAmount > orderItem.Price)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Unable to refund money value more than item price.";
                        return apiResponseViewModel;
                    }
                }

                rewardsDBContext.Database.BeginTransaction();
                vodusV2Context.Database.BeginTransaction();

                try
                {
                    var id = Guid.NewGuid();
                    var refunds = new Refunds
                    {
                        Id = id,
                        CreatedBy = request.UserId,
                        CreatedAt = DateTime.Now,
                        OrderItemId = request.OrderItemId,
                        PointsRefunded = orderItem.Points,
                        MoneyRefunded = request.RefundAmount,
                        Status = 1,
                        MasterMemberProfileId = master.Id,
                        Type = request.RefundType,
                        ShortId = DateTime.Now.ToString("dd") + id.ToString().Split("-")[0],
                        RefundMethod = 1
                    };


                    if (!string.IsNullOrEmpty(request.Remark))
                    {
                        refunds.Remark = request.Remark;
                    }

                    //  TODO initiate ipay88 refunds
                    refunds.Status = 2;

                    rewardsDBContext.Refunds.Add(refunds);
                    await rewardsDBContext.SaveChangesAsync();

                    master.AvailablePoints = master.AvailablePoints + refunds.PointsRefunded;
                    vodusV2Context.MasterMemberProfiles.Update(master);
                    await vodusV2Context.SaveChangesAsync();

                    orderItem.Status = 3;
                    rewardsDBContext.OrderItems.Update(orderItem);
                    await rewardsDBContext.SaveChangesAsync();

                    vodusV2Context.Database.CommitTransaction();
                    rewardsDBContext.Database.CommitTransaction();
                    var address = orderItem.Order.ShippingAddressLine1 + ", ";
                    if (orderItem.Order.ShippingAddressLine2 != null && orderItem.Order.ShippingAddressLine2 != "")
                        address += orderItem.Order.ShippingAddressLine2 + ", ";
                    address += orderItem.Order.ShippingPostcode + " ";
                    address += orderItem.Order.ShippingCity + " ";
                    address += orderItem.Order.ShippingState + ", ";
                    address += orderItem.Order.ShippingCountry;

                    var orderTable = $"<table border='0' cellpadding='0' cellspacing='0' width='100%' class='x_478386741tableButton'>" +
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
                                    $"<img style = 'max-height:80px;max-width:80px;' id = '1624611321109100001_imgsrc_url_1' src='{orderItem.ProductImageFolderUrl.Replace("http://","https://")}'>" +                    "</td>" +
                                        "<td style = 'padding:5px;font-size:12px;'>" +
                                        $"<a style = 'font-size:14px;color:#666;text-decoration:none;' href ='{appSettings.Value.App.VouponUrl}/Order/Refund' target = '_blank'>" +
                                                $"{orderItem.ProductTitle}" +
                                        "</a>" +
                                    "</td> " +
                                    "<td style = 'padding:5px;text-align:center;'>1</td>" +
                                    "<td width = '15%' style = 'min-width:80px;padding:5px;text-align:center;color:#8C37F6;font-weight:bold;'>RM " +
                                            orderItem.SubtotalPrice +
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
                                                      $"<td width='50%' style='text-align: right;'>{ orderItem.Points} </td> " +
                                                   "</tr>  " +
                                                   "<tr>  " +
                                                       "<td width='50%' style='text-align:right;'> Subtotal: </td> " +
                                                       $"<td width='50%' style='text-align:right;'> RM {orderItem.SubtotalPrice}</td>" +
                                                   "</tr> " +
                                                   "<tr style='width: 100%; vertical-align: top; '> " +
                                                       "<td width = '50%' style='text-align:right;'> Discount:</td>" +
                                                       $"<td width = '50%' style='text-align:right;'> -RM {orderItem.DiscountedAmount}</td>" +
                                                   "</tr>  " +
                                                   "<tr style='vertical-align:top;font-weight:bold;'> " +
                                                       "<td width='50%' style='text-align:right;'> Total:</td> " +
                                                        $"<td width='50%' style='text-align:right;color:#8C37F6;'> RM {orderItem.Price} </td>  " +
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

                    //  Send refund notification to user
                    var from = new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus Support");
                    var to = new EmailAddress(orderItem.Order.Email, orderItem.Order.BillingPersonFirstName + " " + orderItem.Order.BillingPersonLastName);
                    var client = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                    var msg = new SendGridMessage();
                    msg.From = from;
                    msg.TemplateId = appSettings.Value.Mailer.Sendgrid.Templates.RefundToBuyer;
                    msg.Personalizations = new System.Collections.Generic.List<Personalization>();
                    var personalization = new Personalization();
                    personalization.Substitutions = new Dictionary<string, string>();
                    personalization.Substitutions.Add("-name-", orderItem.Order.BillingPersonFirstName + " " + orderItem.Order.BillingPersonLastName);
                    personalization.Substitutions.Add("-date-", orderItem.Order.CreatedAt.ToString("dd MMM yyyy"));
                    personalization.Substitutions.Add("-orderTable-", orderTable);
                    personalization.Subject = "Vodus order item refund #" + orderItem.ShortId;
                    personalization.Tos = new List<EmailAddress>();
                    personalization.Tos.Add(to);
                    msg.Personalizations.Add(personalization);
                    var response = await client.SendEmailAsync(msg);

                    apiResponseViewModel.Message = "Succesfully refunded order item: #" + orderItem.ShortId;
                    apiResponseViewModel.Successful = true;
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    rewardsDBContext.Database.RollbackTransaction();
                    vodusV2Context.Database.RollbackTransaction();

                    var errorLogs = new Voupon.Database.Postgres.RewardsEntities.ErrorLogs
                    {
                        TypeId = 1,
                        ActionRequest = JsonConvert.SerializeObject(request),
                        ActionName = "CreateOrderItemRefundCommand",
                        CreatedAt = DateTime.Now,
                        Errors = ex.ToString(),
                        Email = orderItem.Order.Email,
                        MasterProfileId = orderItem.Order.MasterMemberProfileId
                    };

                    rewardsDBContext.ErrorLogs.Add(errorLogs);
                    await rewardsDBContext.SaveChangesAsync();

                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to issue refund [099]";
                    return apiResponseViewModel;
                }

            }
        }

    }

}
