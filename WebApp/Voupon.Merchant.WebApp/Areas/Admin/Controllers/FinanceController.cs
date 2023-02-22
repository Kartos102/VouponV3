using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using Voupon.Common.Azure.Blob;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Create;
using Voupon.Merchant.WebApp.Common.Services.FinanceSummary.Command;
using Voupon.Merchant.WebApp.Common.Services.FinanceSummary.Queries;
using Voupon.Merchant.WebApp.Common.Services.Users.Queries;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.ViewModels;
using System.Text;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    public class FinanceSummaryViewModel
    {
        public List<Voupon.Database.Postgres.RewardsEntities.FinanceSummary> SummaryList { get; set; }
    }

    public class FinanceSummaryDetailViewModel
    {
        public Voupon.Database.Postgres.RewardsEntities.FinanceSummary Summary { get; set; }
    }

    public class PayoutDetailViewModel
    {
        public string MerchantDisplayName { get; set; }
        public int MerchantId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<OutletRedemptionModel> Outlet { get; set; }
        public List<DigitalRedemptionModel> Digital { get; set; }
        public List<DeliveryRedemptionModel> Delivery { get; set; }
    }

    public class OutletRedemptionModel
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public decimal Revenue { get; set; }
        public string Transaction { get; set; }
    }
    public class DigitalRedemptionModel
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public decimal Revenue { get; set; }
        public string Transaction { get; set; }
    }

    public class DeliveryRedemptionModel
    {
        public int ProductId { get; set; }
        public string ProductTitle { get; set; }
        public decimal Revenue { get; set; }
        public string Transaction { get; set; }
    }


    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class FinanceController : BaseAdminController
    {
        public async Task<IActionResult> Index()
        {
            ApiResponseViewModel response = await Mediator.Send(new FinanceSummaryListQuery());
            FinanceSummaryViewModel vm = new FinanceSummaryViewModel() { SummaryList = new List<Voupon.Database.Postgres.RewardsEntities.FinanceSummary>() };
            if (response.Successful)
            {
                vm.SummaryList = (List<Voupon.Database.Postgres.RewardsEntities.FinanceSummary>)response.Data;
            }
            return View(vm);
        }

        [Route("PayoutDetail/{MerchantFinanceId}")]
        public async Task<IActionResult> PayoutDetail(int MerchantFinanceId)
        {
            ApiResponseViewModel response = await Mediator.Send(new GetPayoutQuery() { MerchantFinanceId = MerchantFinanceId });
            PayoutDetailViewModel vm = new PayoutDetailViewModel() { Delivery = new List<DeliveryRedemptionModel>(), Digital = new List<DigitalRedemptionModel>(), Outlet = new List<OutletRedemptionModel>() };
            if (response.Successful)
            {
                var data = (Voupon.Database.Postgres.RewardsEntities.MerchantFinance)response.Data;
                vm.MerchantId = data.MerchantId;
                vm.MerchantDisplayName = data.MerchantDisplayName;
                vm.StartDate = data.FinanceSummary.StartDate.ToDateTime(TimeOnly.Parse("10:00PM"));
                vm.EndDate = data.FinanceSummary.EndDate.ToDateTime(TimeOnly.Parse("10:00PM"));
                foreach (var item in data.FinanceTransaction)
                {
                    if (item.OrderItem.ExpirationTypeId == 1 || item.OrderItem.ExpirationTypeId == 2)
                    {
                        OutletRedemptionModel model = new OutletRedemptionModel();
                        model.ProductId = item.ProductId;
                        model.ProductTitle = item.ProductTitle;
                        model.Revenue = item.TotalPrice;
                        model.Transaction = item.OrderItemId.ToString();
                        vm.Outlet.Add(model);

                    }
                    else if (item.OrderItem.ExpirationTypeId == 4)
                    {
                        DigitalRedemptionModel model = new DigitalRedemptionModel();
                        model.ProductId = item.ProductId;
                        model.ProductTitle = item.ProductTitle;
                        model.Revenue = item.TotalPrice;
                        model.Transaction = item.OrderItemId.ToString();
                        vm.Digital.Add(model);
                    }
                    else
                    {
                        DeliveryRedemptionModel model = new DeliveryRedemptionModel();
                        model.ProductId = item.ProductId;
                        model.ProductTitle = item.ProductTitle;
                        model.Revenue = item.TotalPrice;
                        model.Transaction = item.OrderItemId.ToString();
                        vm.Delivery.Add(model);
                    }
                }
            }
            return View(vm);
            // return View(vm);
        }

        [Route("Payout/{Id}")]
        public async Task<IActionResult> Payout(int Id)
        {
            ApiResponseViewModel response = await Mediator.Send(new FinanceSummaryQuery() { FinanceSummaryId = Id });
            FinanceSummaryDetailViewModel vm = new FinanceSummaryDetailViewModel() { Summary = new Voupon.Database.Postgres.RewardsEntities.FinanceSummary() { MerchantFinance = new List<Voupon.Database.Postgres.RewardsEntities.MerchantFinance>() } };
            if (response.Successful)
            {
                vm.Summary = (Voupon.Database.Postgres.RewardsEntities.FinanceSummary)response.Data;
            }
            return View(vm);
        }

        [Route("Statement/{Id}")]
        public async Task<IActionResult> Statement(int Id)
        {
            ApiResponseViewModel response = await Mediator.Send(new GetPayoutQuery() { MerchantFinanceId = Id });
            if (response.Successful)
            {
                var data = (Voupon.Database.Postgres.RewardsEntities.MerchantFinance)response.Data;        
            }

            return new Rotativa.AspNetCore.ViewAsPdf("Statement", response.Data);
            // return View();
        }

        [HttpPost]
        [Route("UpdatePayout/{Id}")]
        public async Task<ApiResponseViewModel> UpdatePayout(int Id, string Remark)
        {
            bool IsStatementCreated = false;
            ApiResponseViewModel payoutResponse = await Mediator.Send(new GetPayoutQuery() { MerchantFinanceId = Id });
            if (payoutResponse.Successful)
            {
                var data = (Voupon.Database.Postgres.RewardsEntities.MerchantFinance)payoutResponse.Data;
                IsStatementCreated = data.IsPaid;
            }

            ApiResponseViewModel response = await Mediator.Send(new UpdatePayoutCommand() { MerchantFinanceId = Id, PaidOutDate = DateTime.Now, Remarks = Remark });
            if (response.Successful)
            {
                var data = (Voupon.Database.Postgres.RewardsEntities.MerchantFinance)response.Data;
                response.Data = data.PayoutDate.Value;
                if (!IsStatementCreated)
                {
                    payoutResponse = await Mediator.Send(new GetPayoutQuery() { MerchantFinanceId = Id });
                    if (payoutResponse.Successful)
                    {
                        data = (Voupon.Database.Postgres.RewardsEntities.MerchantFinance)payoutResponse.Data;
                    }

                    var pdf = new Rotativa.AspNetCore.ViewAsPdf("Statement", data);
                    byte[] applicationPDFData = await pdf.BuildFile(this.ControllerContext);
                    ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                    apiResponseViewModel = await Mediator.Send(new CreateStatementFileCommand()
                    {
                        ContainerName = ContainerNameEnum.Merchants,
                        Data = applicationPDFData,
                        FileName = "Statement_" + data.FinanceSummary.StartDate.ToString("ddMMyyyy") + "_" + data.FinanceSummary.EndDate.ToString("ddMMyyyy"),
                        MerchantId = data.MerchantId
                    });
                    if (apiResponseViewModel.Successful)
                    {
                        var url = apiResponseViewModel.Data.ToString();
                        var UpdateResponse = await Mediator.Send(new UpdateStatementUrlCommand() { MerchantFinanceId = Id, Url = url });

                        string email = "";
                        var userResponse = await Mediator.Send(new UserListWithMerchantIdQuery() { MerchantId = data.MerchantId });
                        if (userResponse.Successful)
                        {
                            var users = (List<Voupon.Database.Postgres.RewardsEntities.Users>)userResponse.Data;
                            users = users.OrderBy(x => x.CreatedAt).ToList();
                            //Guid role = new Guid("1A436B3D-15A0-4F03-8E4E-0022A5DD5736");
                            var role = "1A436B3D-15A0-4F03-8E4E-0022A5DD5736";
                            var user = users.FirstOrDefault(x => x.UserRoles != null && x.UserRoles.Count > 0 && x.UserRoles.First().RoleId == new Guid(role));
                            email = user.Email;
                        }

                        var apiKey = "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";
                        string templateId = "62b9d832-12e1-48e7-92ee-6e26be0a4867";
                        string subject = "Statement Of Account ( " + data.FinanceSummary.StartDate.ToString("dd MMMM yyyy") + " - " + data.FinanceSummary.EndDate.ToString("dd MMMM yyyy") + " )";
                        var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus Merchants");
                        var to = new SendGrid.Helpers.Mail.EmailAddress(email, data.Merchant.DisplayName);
                        var client = new SendGridClient(apiKey);
                        var msg = new SendGridMessage();
                        msg.From = from;
                        msg.TemplateId = templateId;
                        msg.Personalizations = new System.Collections.Generic.List<Personalization>();
                        var personalization = new Personalization();
                        personalization.Substitutions = new Dictionary<string, string>();
                        personalization.Substitutions.Add("-url-", url);
                        personalization.Substitutions.Add("-date-", data.FinanceSummary.StartDate.ToString("dd MMMM yyyy") + " - " + data.FinanceSummary.EndDate.ToString("dd MMMM yyyy"));
                        personalization.Subject = subject;
                        personalization.Tos = new List<EmailAddress>();
                        personalization.Tos.Add(to);
                        msg.Personalizations.Add(personalization);
                        byte[] content;
                        using (var wc = new System.Net.WebClient())
                        {
                            wc.Encoding = Encoding.UTF8;
                            content = wc.DownloadData(url);
                        }
                        msg.AddAttachment(new Attachment
                        {
                            Content = Convert.ToBase64String(content),
                            Filename = "Statement" + ".pdf",
                            Type = "application/pdf",
                            Disposition = "attachment"
                        });

                        var sendEmailResponse = await client.SendEmailAsync(msg).ConfigureAwait(false);

                    }
                }
            }
            return response;
        }

        [HttpGet]
        [Route("GetPayout/{Id}")]
        public async Task<ApiResponseViewModel> GetPayout(int Id)
        {
            ApiResponseViewModel response = await Mediator.Send(new GetPayoutQuery() { MerchantFinanceId = Id });
            if (response.Successful)
            {
                var data = (Voupon.Database.Postgres.RewardsEntities.MerchantFinance)response.Data;
                data.Merchant = null;
                data.FinanceTransaction = null;
                data.FinanceSummary = null;
                response.Data = data;
            }
            return response;
        }
    }
}