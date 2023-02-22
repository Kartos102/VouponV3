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
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Logger;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Products.Command
{
    public class UpdateProductPendingChangesStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public string Remarks { get; set; }
        public string UserEmail { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateProductPendingChangesStatusCommandHandler : IRequestHandler<UpdateProductPendingChangesStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public UpdateProductPendingChangesStatusCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductPendingChangesStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                if (!string.IsNullOrEmpty(request.Remarks))
                    request.Remarks = request.Remarks + "<i class='meesage-time'>" + request.LastUpdatedAt.ToString() + "</i>";
                var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
                if (product != null)
                {
                    var jsonString = "";
                    if (String.IsNullOrEmpty(product.PendingChanges))
                    {
                        jsonString = JsonConvert.SerializeObject(product);
                    }
                    else
                    {
                        jsonString = product.PendingChanges;

                        jsonString = product.PendingChanges;
                        jsonString = jsonString.Replace("DealExpirations\":[]:", "DealExpirations\":null");
                        jsonString = jsonString.Replace("\"DealExpirations\":[],", "");
                        jsonString = jsonString.Replace("DealExpirations:[],", "");
                        jsonString = jsonString.Replace("DealExpirations", "DealExpiration");

                        var newItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Products>(jsonString);
                        newItem.PendingChanges = "";
                        if (!string.IsNullOrEmpty(newItem.Remarks) && !string.IsNullOrEmpty(request.Remarks))
                            newItem.Remarks = newItem.Remarks + "<br>" + request.Remarks;
                        else if (!string.IsNullOrEmpty(request.Remarks))
                            newItem.Remarks = request.Remarks;

                        if (request.StatusTypeId == 4)
                        {
                            newItem.Remarks = "";
                        }
                        else
                        {
                            newItem.StatusTypeId = 2;
                        }

                        newItem.StatusTypeId = request.StatusTypeId;
                        product.PendingChanges = "";
                        product.PendingChanges = JsonConvert.SerializeObject(newItem);
                        rewardsDBContext.SaveChanges();
                        response.Successful = true;
                        response.Message = "Update Product Status Successfully";
                        response.Data = newItem.Remarks;

                    }

                    if (request.StatusTypeId == 2 || request.StatusTypeId == 3)
                    {
                        var currentUserRole = await rewardsDBContext.UserRoles.Include(x => x.Role).Where(x => x.UserId == request.LastUpdatedByUserId && x.Role.Name == "Admin").FirstOrDefaultAsync();

                        //  If user is Admin
                        if (currentUserRole != null)
                        {
                            var userRole = await rewardsDBContext.UserRoles.Include(x => x.Role).Where(x => x.MerchantId == product.MerchantId && x.Role.Name == "Merchant").FirstOrDefaultAsync();

                            if (userRole != null)
                            {
                                var user = await rewardsDBContext.Users.Where(x => x.Id == userRole.UserId).FirstOrDefaultAsync();
                                if (user != null)
                                {
                                    var url = appSettings.Value.App.BaseUrl + "/App/Products/Edit/" + request.ProductId;
                                    var sendGridClient = new SendGridClient(appSettings.Value.Mailer.Sendgrid.APIKey);
                                    var msg = new SendGridMessage();
                                    msg.SetTemplateId(appSettings.Value.Mailer.Sendgrid.Templates.AdminReviewResponse);
                                    msg.SetFrom(new EmailAddress(appSettings.Value.Emails.Noreply, "Vodus No-Reply"));
                                    msg.SetSubject("Your Vodus Product Listing has been Reviewed");
                                    msg.AddTo(new EmailAddress(user.Email));
                                    msg.AddSubstitution("-name-", user.UserName);
                                    msg.AddSubstitution("-message-", "Your product ( " + product.Title + " ) is pending revision from your end based on the following comments from our admin:");
                                    msg.AddSubstitution("-adminRemark-", request.Remarks);
                                    msg.AddSubstitution("-url-", url);
                                    var responsess = sendGridClient.SendEmailAsync(msg).Result;
                                }
                            }
                        }
                    }
                }
                else
                {
                    response.Message = "Product not found";
                }
            }
            catch (Exception ex)
            {
                var errorLogs = new ErrorLogs
                {
                    ActionName = "UpdateProductPendingChangesStatusCommand",
                    ActionRequest = JsonConvert.SerializeObject(request),
                    CreatedAt = DateTime.Now,
                    Errors = ex.ToString(),
                    Email = request.UserEmail,
                    TypeId = CreateErrorLogCommand.Type.Service
                };

                rewardsDBContext.ErrorLogs.Add(errorLogs);
                await rewardsDBContext.SaveChangesAsync();
                response.Successful = false;
                response.Message = "Fail to Update Product status";
            }
            return response;
        }
    }
}
