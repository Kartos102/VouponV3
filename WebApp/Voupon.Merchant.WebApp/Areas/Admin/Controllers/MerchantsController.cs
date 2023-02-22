using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Common.Azure.Blob;
using Voupon.Common.Enum;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Merchants.Commands;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Merchants.ViewModels;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Commands;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Models;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Queries;
using Voupon.Merchant.WebApp.Common.Services.Banks.Queries;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Create;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Delete;
using Voupon.Merchant.WebApp.Common.Services.Blob.Queries;
using Voupon.Merchant.WebApp.Common.Services.BusinessTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Countries.Queries;
using Voupon.Merchant.WebApp.Common.Services.Districts.Queries;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Commands;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Commands;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Queries;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Commands;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Models;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Queries;
using Voupon.Merchant.WebApp.Common.Services.Postcodes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Provinces.Queries;
using Voupon.Merchant.WebApp.Common.Services.StatusTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Users.Queries;
using Voupon.Merchant.WebApp.ViewModels;
using System.Linq.Dynamic.Core;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    public class MerchantViewModel : MerchantModel
    {
        public int JsonStatusTypeId { get; set; }
        public string JsonStatus { get; set; }
        public int JsonPICStatusTypeId { get; set; }
        public string JsonPICStatus { get; set; }
        public int JsonBankStatusTypeId { get; set; }
        public string JsonBankStatus { get; set; }
    }

    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MerchantsController : BaseAdminController
    {
        public async Task<IActionResult> Index()
        {
            //MerchantViewModel a = new MerchantViewModel();           
            List<MerchantViewModel> model = null;
            var response = await Mediator.Send(new MerchantListQuery());
            if (response.Successful)
            {
                var list1 = (IEnumerable<MerchantModel>)response.Data;
                List<MerchantViewModel> list = new List<MerchantViewModel>();
                List<MerchantViewModel> pendingreviewlist = new List<MerchantViewModel>();
                List<MerchantViewModel> pendingrevisionlist = new List<MerchantViewModel>();
                List<MerchantViewModel> approvedlist = new List<MerchantViewModel>();
                List<MerchantViewModel> draftlist = new List<MerchantViewModel>();
                List<MerchantViewModel> sortedList = new List<MerchantViewModel>();
                foreach (var item in list1)
                {
                    MerchantViewModel temp = new MerchantViewModel();
                    temp.Id = item.Id;
                    temp.Code = item.Code;
                    temp.CountryId = item.CountryId;
                    temp.CountryName = item.CountryName;
                    temp.ProvinceId = item.ProvinceId;
                    temp.ProvinceName = item.ProvinceName;
                    temp.DistritId = item.DistritId;
                    temp.DistritName = item.DistritName;
                    temp.PostCodeId = item.PostCodeId;
                    temp.PostCodeName = item.PostCodeName;
                    temp.Address_1 = item.Address_1;
                    temp.Address_2 = item.Address_2;
                    temp.RegistrationNumber = item.RegistrationNumber;
                    temp.BIDDocumentUrl = item.BIDDocumentUrl;
                    temp.BusinessTypeId = item.BusinessTypeId;
                    temp.CompanyContact = item.CompanyContact;
                    temp.CompanyName = item.CompanyName;
                    temp.Description = item.Description;
                    temp.DisplayName = item.DisplayName;
                    temp.LogoUrl = item.LogoUrl;
                    temp.WebsiteSiteUrl = item.WebsiteSiteUrl;
                    temp.CreatedAt = item.CreatedAt;
                    temp.CreatedByUserId = item.CreatedByUserId;
                    temp.LastUpdatedAt = item.LastUpdatedAt;
                    temp.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    temp.StatusTypeId = item.StatusTypeId;
                    temp.Status = item.Status;
                    temp.Remarks = item.Remarks;
                    temp.TotalOutlets = item.TotalOutlets;
                    temp.IsPublished = item.IsPublished;
                    temp.IsBrandShownInHomePage = item.IsBrandShownInHomePage;
                    temp.PICStatusTypeId = item.PICStatusTypeId;
                    temp.PICRemarks = item.PICRemarks;
                    temp.PICStatus = item.PICStatus;
                    temp.BankRemarks = item.BankRemarks;
                    temp.BankStatus = item.BankStatus;
                    temp.BankStatusTypeId = item.BankStatusTypeId;
                    temp.DefaultCommission = item.DefaultCommission;
                    temp.IsTestAccount = item.IsTestAccount;
                    list.Add(temp);
                }
                foreach (var item in list)
                {
                    var merchantId = item.Id;
                    var merchantResponse = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = merchantId });
                    if (merchantResponse.Successful)
                    {
                        var merchant = (MerchantModel)merchantResponse.Data;                           
                        item.JsonStatus = merchant.Status;
                        item.JsonStatusTypeId = merchant.StatusTypeId;
                    }

                    merchantResponse = await Mediator.Send(new MerchantBankAccountPendingChangesQuery() { MerchantId = merchantId });
                    if (merchantResponse.Successful)
                    {
                        var merchant = (BankAccountModel)merchantResponse.Data;
                        item.JsonBankStatus = merchant.Status;
                        item.JsonBankStatusTypeId = merchant.StatusTypeId;
                    }

                    merchantResponse = await Mediator.Send(new MerchantPersonInChargePendingChangesQuery() { MerchantId = merchantId });
                    if (merchantResponse.Successful)
                    {
                        var merchant = (PersonInChargeModel)merchantResponse.Data;
                        item.JsonPICStatus = merchant.Status;
                        item.JsonPICStatusTypeId = merchant.StatusTypeId;
                    }

                    if (item.JsonStatusTypeId == StatusTypeEnum.PENDING_REVIEW)
                    {
                        item.StatusTypeId = item.JsonStatusTypeId;
                        item.Status = item.JsonStatus;
                    }

                    if (item.JsonBankStatusTypeId == StatusTypeEnum.PENDING_REVIEW)
                    {
                        item.BankStatusTypeId = item.JsonBankStatusTypeId;
                        item.BankStatus = item.JsonBankStatus;
                    }

                    if (item.JsonPICStatusTypeId == StatusTypeEnum.PENDING_REVIEW)
                    {
                        item.PICStatusTypeId = item.JsonPICStatusTypeId;
                        item.PICStatus = item.JsonPICStatus;
                    }
                    if (item.StatusTypeId == StatusTypeEnum.PENDING_REVIEW || item.BankStatusTypeId == StatusTypeEnum.PENDING_REVIEW || item.PICStatusTypeId == StatusTypeEnum.PENDING_REVIEW)
                    {
                        pendingreviewlist.Add(item);
                        continue;
                    }
                    if (item.StatusTypeId == StatusTypeEnum.PENDING_REVISION || item.BankStatusTypeId == StatusTypeEnum.PENDING_REVISION || item.PICStatusTypeId == StatusTypeEnum.PENDING_REVISION)
                    {
                        pendingrevisionlist.Add(item);
                        continue;
                    }
                    if (item.StatusTypeId == StatusTypeEnum.DRAFT || item.BankStatusTypeId == StatusTypeEnum.DRAFT || item.PICStatusTypeId == StatusTypeEnum.DRAFT)
                    {
                        draftlist.Add(item);
                        continue;
                    }
                    if (item.StatusTypeId == StatusTypeEnum.APPROVED || item.BankStatusTypeId == StatusTypeEnum.APPROVED || item.PICStatusTypeId == StatusTypeEnum.APPROVED)
                    {
                        approvedlist.Add(item);
                        continue;
                    }
                }

                sortedList.AddRange(pendingreviewlist.OrderByDescending(x=>x.CreatedAt));
                sortedList.AddRange(pendingrevisionlist.OrderByDescending(x => x.CreatedAt));
                sortedList.AddRange(approvedlist.OrderByDescending(x => x.CreatedAt));
                sortedList.AddRange(draftlist.OrderByDescending(x => x.CreatedAt));
                model = sortedList;// list;

            }


            return View(model);
        }

        [HttpGet]
        [Route("Get-Merchants-List")]
        public async Task<ApiResponseViewModel> GetMerchantsList(int merchantId, string start, string length, string sortColumn, string searchValue)
        {
            try
            {
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                ApiResponseViewModel response = await GetMercahnts();
                if (response.Successful)
                {
                    var merchantsData = (List<MerchantViewModel>)response.Data;
                    if (!(string.IsNullOrEmpty(sortColumn)))
                    {
                        merchantsData = merchantsData.AsQueryable().OrderBy(sortColumn).ToList();
                    }
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        merchantsData = merchantsData.Where(m => m.DisplayName.ToLower().Contains(searchValue) || m.Code.Contains(searchValue)).ToList();
                    }
                    recordsTotal = merchantsData.Count();
                    var data = merchantsData.Skip(skip).Take(pageSize).ToList();
                    var jsonData = new { recordsFiltered = data.Count(), recordsTotal = recordsTotal, data = data };
                    response.Data = jsonData;
                    return response;
                }
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<ApiResponseViewModel> GetMercahnts()
        {
            List<MerchantViewModel> model = null;
            var response = await Mediator.Send(new MerchantListQuery());
            if (response.Successful)
            {
                var list1 = (IEnumerable<MerchantModel>)response.Data;
                List<MerchantViewModel> list = new List<MerchantViewModel>();
                List<MerchantViewModel> pendingreviewlist = new List<MerchantViewModel>();
                List<MerchantViewModel> pendingrevisionlist = new List<MerchantViewModel>();
                List<MerchantViewModel> approvedlist = new List<MerchantViewModel>();
                List<MerchantViewModel> draftlist = new List<MerchantViewModel>();
                List<MerchantViewModel> sortedList = new List<MerchantViewModel>();
                foreach (var item in list1)
                {
                    MerchantViewModel temp = new MerchantViewModel();
                    temp.Id = item.Id;
                    temp.Code = item.Code;
                    temp.CountryId = item.CountryId;
                    temp.CountryName = item.CountryName;
                    temp.ProvinceId = item.ProvinceId;
                    temp.ProvinceName = item.ProvinceName;
                    temp.DistritId = item.DistritId;
                    temp.DistritName = item.DistritName;
                    temp.PostCodeId = item.PostCodeId;
                    temp.PostCodeName = item.PostCodeName;
                    temp.Address_1 = item.Address_1;
                    temp.Address_2 = item.Address_2;
                    temp.RegistrationNumber = item.RegistrationNumber;
                    temp.BIDDocumentUrl = item.BIDDocumentUrl;
                    temp.BusinessTypeId = item.BusinessTypeId;
                    temp.CompanyContact = item.CompanyContact;
                    temp.CompanyName = item.CompanyName;
                    temp.Description = item.Description;
                    temp.DisplayName = item.DisplayName;
                    temp.LogoUrl = item.LogoUrl;
                    temp.WebsiteSiteUrl = item.WebsiteSiteUrl;
                    temp.CreatedAt = item.CreatedAt;
                    temp.CreatedByUserId = item.CreatedByUserId;
                    temp.LastUpdatedAt = item.LastUpdatedAt;
                    temp.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    temp.StatusTypeId = item.StatusTypeId;
                    temp.Status = item.Status;
                    temp.Remarks = item.Remarks;
                    temp.TotalOutlets = item.TotalOutlets;
                    temp.IsPublished = item.IsPublished;
                    temp.IsBrandShownInHomePage = item.IsBrandShownInHomePage;
                    temp.PICStatusTypeId = item.PICStatusTypeId;
                    temp.PICRemarks = item.PICRemarks;
                    temp.PICStatus = item.PICStatus;
                    temp.BankRemarks = item.BankRemarks;
                    temp.BankStatus = item.BankStatus;
                    temp.BankStatusTypeId = item.BankStatusTypeId;
                    temp.DefaultCommission = item.DefaultCommission;
                    temp.IsTestAccount = item.IsTestAccount;
                    list.Add(temp);
                }
                foreach (var item in list)
                {
                    var merchantId = item.Id;
                    var merchantResponse = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = merchantId });
                    if (merchantResponse.Successful)
                    {
                        var merchant = (MerchantModel)merchantResponse.Data;
                        item.JsonStatus = merchant.Status;
                        item.JsonStatusTypeId = merchant.StatusTypeId;
                    }

                    merchantResponse = await Mediator.Send(new MerchantBankAccountPendingChangesQuery() { MerchantId = merchantId });
                    if (merchantResponse.Successful)
                    {
                        var merchant = (BankAccountModel)merchantResponse.Data;
                        item.JsonBankStatus = merchant.Status;
                        item.JsonBankStatusTypeId = merchant.StatusTypeId;
                    }

                    merchantResponse = await Mediator.Send(new MerchantPersonInChargePendingChangesQuery() { MerchantId = merchantId });
                    if (merchantResponse.Successful)
                    {
                        var merchant = (PersonInChargeModel)merchantResponse.Data;
                        item.JsonPICStatus = merchant.Status;
                        item.JsonPICStatusTypeId = merchant.StatusTypeId;
                    }

                    if (item.JsonStatusTypeId == StatusTypeEnum.PENDING_REVIEW)
                    {
                        item.StatusTypeId = item.JsonStatusTypeId;
                        item.Status = item.JsonStatus;
                    }

                    if (item.JsonBankStatusTypeId == StatusTypeEnum.PENDING_REVIEW)
                    {
                        item.BankStatusTypeId = item.JsonBankStatusTypeId;
                        item.BankStatus = item.JsonBankStatus;
                    }

                    if (item.JsonPICStatusTypeId == StatusTypeEnum.PENDING_REVIEW)
                    {
                        item.PICStatusTypeId = item.JsonPICStatusTypeId;
                        item.PICStatus = item.JsonPICStatus;
                    }
                    if (item.StatusTypeId == StatusTypeEnum.PENDING_REVIEW || item.BankStatusTypeId == StatusTypeEnum.PENDING_REVIEW || item.PICStatusTypeId == StatusTypeEnum.PENDING_REVIEW)
                    {
                        pendingreviewlist.Add(item);
                        continue;
                    }
                    if (item.StatusTypeId == StatusTypeEnum.PENDING_REVISION || item.BankStatusTypeId == StatusTypeEnum.PENDING_REVISION || item.PICStatusTypeId == StatusTypeEnum.PENDING_REVISION)
                    {
                        pendingrevisionlist.Add(item);
                        continue;
                    }
                    if (item.StatusTypeId == StatusTypeEnum.DRAFT || item.BankStatusTypeId == StatusTypeEnum.DRAFT || item.PICStatusTypeId == StatusTypeEnum.DRAFT)
                    {
                        draftlist.Add(item);
                        continue;
                    }
                    if (item.StatusTypeId == StatusTypeEnum.APPROVED || item.BankStatusTypeId == StatusTypeEnum.APPROVED || item.PICStatusTypeId == StatusTypeEnum.APPROVED)
                    {
                        approvedlist.Add(item);
                        continue;
                    }
                }

                sortedList.AddRange(pendingreviewlist.OrderByDescending(x => x.CreatedAt));
                sortedList.AddRange(pendingrevisionlist.OrderByDescending(x => x.CreatedAt));
                sortedList.AddRange(approvedlist.OrderByDescending(x => x.CreatedAt));
                sortedList.AddRange(draftlist.OrderByDescending(x => x.CreatedAt));
                model = sortedList;

            }
            response.Data = model;

            return response;
        }

        [Route("Details/{merchantId}")]
        public async Task<IActionResult> Details(int merchantId)
        {
            DetailsViewModel details = new DetailsViewModel();

            var sasToken = await Mediator.Send(new SASTokenQuery());

            var bankAccountResponse = await Mediator.Send(new MerchantBankAccountQuery() { MerchantId = merchantId });
            if (bankAccountResponse.Successful)
            {
                details.BankAccount = (BankAccountModel)bankAccountResponse.Data;
                details.BankAccount.DocumentUrl = details.BankAccount.DocumentUrl + sasToken.Data.ToString();
            }
            var personInChargeResponse = await Mediator.Send(new MerchantPersonInChargeQuery() { MerchantId = merchantId });
            if (personInChargeResponse.Successful)
            {
                details.PersonInCharge = (PersonInChargeModel)personInChargeResponse.Data;
                details.PersonInCharge.DocumentUrl = details.PersonInCharge.DocumentUrl + sasToken.Data.ToString();
            }
            var merchantResponse = await Mediator.Send(new MerchantQuery() { MerchantId = merchantId });
            if (merchantResponse.Successful)
            {
                details.Merchant = (MerchantModel)merchantResponse.Data;
                details.Merchant.BIDDocumentUrl = details.Merchant.BIDDocumentUrl + sasToken.Data.ToString();
            }

            details.RegisteredEmail= "";
            var userResponse = await Mediator.Send(new UserListWithMerchantIdQuery() { MerchantId = merchantId });
            if (userResponse.Successful)
            {
                var users = (List<Voupon.Database.Postgres.RewardsEntities.Users>)userResponse.Data;
                users = users.OrderBy(x => x.CreatedAt).ToList();
                Guid role = new Guid("1A436B3D-15A0-4F03-8E4E-0022A5DD5736");
                var user = users.FirstOrDefault(x => x.UserRoles != null && x.UserRoles.Count > 0 && x.UserRoles.First().RoleId == role);
                details.RegisteredEmail = user.Email;
            }
            return View(details);
        }
        #region Outlet
        [HttpPost]
        [Route("UpdateOutlet")]
        public async Task<IActionResult> UpdateOutlet(UpdateOutletCommand command)
        {
            if (!await VerifyUserAccessWithOutletId(command.Id))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return BadRequest(badRequestResponse);
            }
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost]
        [Route("UploadOutletImages/{outletId}")]
        public async Task<ApiResponseViewModel> UploadOutletImages(int outletId)
        {
            if (!await VerifyUserAccessWithOutletId(outletId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContents = HttpContext.Request.Form.Files;

            if (fileContents == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }

            var passValidation = true;
            foreach (var fileContent in fileContents)
            {
                var fileExtention = fileContent.FileName.Substring(fileContent.FileName.LastIndexOf(".")).ToLower();
                if (fileExtention != ".jpg" && fileExtention != ".jpeg" && fileExtention != ".png" && fileExtention != ".jfif")
                {
                    passValidation = false;
                }
            }

            if (passValidation)
            {
                CreateImagesCommand command = new CreateImagesCommand()
                {
                    Id = outletId,
                    Files = fileContents,
                    ContainerName = ContainerNameEnum.Outlets,
                    FilePath = FilePathEnum.Outlets_Images
                };
                ApiResponseViewModel response = await Mediator.Send(command);
                return response;
            }
            else
            {
                ApiResponseViewModel respons = new ApiResponseViewModel();
                respons.Successful = false;
                respons.Message = "Please upload Images having extensions: png, jpeg, jpg, jfif only";
                return respons;
            }

            //CreateFilesCommand command = new CreateFilesCommand()
            //{
            //    Id = outletId,
            //    Files = fileContents,
            //    ContainerName = ContainerNameEnum.Outlets,
            //    FilePath = FilePathEnum.Outlets_Images
            //}
            //;
            //ApiResponseViewModel response = await Mediator.Send(command);
            //return response;
        }


        [HttpPost]
        [Route("DeleteOutlet")]
        public async Task<ApiResponseViewModel> DeleteOutlet([FromForm] DeleteOutletCommand command)
        {
            if (!await VerifyUserAccessWithOutletId(command.OutletId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("UpdateOutletStatus")]
        public async Task<ApiResponseViewModel> UpdateOutletStatus([FromForm] UpdateOutletStatusCommand command)
        {
            if (!await VerifyUserAccessWithOutletId(command.OutletId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpGet]
        [Route("GetOutlet/{outletId}")]
        public async Task<ApiResponseViewModel> GetOutlet(int outletId)
        {
            if (!await VerifyUserAccessWithOutletId(outletId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = await Mediator.Send(new OutletQuery() { OutletId = outletId });
            return response;
        }

        [HttpGet]
        [Route("GetOutletImages/{outletId}")]
        public async Task<ApiResponseViewModel> GetOutletImages(int outletId)
        {
            if (!await VerifyUserAccessWithOutletId(outletId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            //ApiResponseViewModel response = await Mediator.Send(new OutletQuery() { OutletId = outletId });
            //if (!response.Successful)
            //{
            //    apiResponseViewModel.Successful = false;
            //    apiResponseViewModel.Message = "Invalid outlet";
            //    return apiResponseViewModel;
            //}
            //var outlet=(Common.Services.Outlets.Models.OutletModel)response.Data;
            //if(String.IsNullOrEmpty(outlet.ImgUrl))
            //{
            //    apiResponseViewModel.Successful = true;
            //    apiResponseViewModel.Message = "No Images";
            //    return apiResponseViewModel;
            //}
            // string[] url=outlet.ImgUrl.Replace(@"http://vodusuat.blob.core.windows.net:80/outlets/","").Split("/");

            BlobSmallImagesListQuery command = new BlobSmallImagesListQuery()
            {
                Id = outletId,
                ContainerName = ContainerNameEnum.Outlets,
                FilePath = FilePathEnum.Outlets_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }
        private async Task<bool> VerifyUserAccessWithOutletId(int outletId)
        {
            int merchantId = await QueriesOutletMerchantId(outletId);
            return VerifyUserAccess(merchantId);
        }

        private async Task<int> QueriesOutletMerchantId(int outletId)
        {
            int merchantId = 0;
            ApiResponseViewModel response = await Mediator.Send(new OutletQuery() { OutletId = outletId });
            if (response.Successful)
            {
                if (response.Data != null)
                {
                    var outlet = (Common.Services.Outlets.Models.OutletModel)response.Data;
                    merchantId = outlet.MerchantId;
                }
            }
            return merchantId;
        }

        private bool VerifyUserAccess(int merchantId)
        {
            bool IsMatch = false;
            try
            {
                var userid = User.Identity.GetUserId();
                if (User.IsInRole("Admin"))
                {
                    IsMatch = true;
                    return IsMatch;
                }
                var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
                if (Int32.Parse(userMerchantId) == merchantId)
                {
                    IsMatch = true;
                }
            }
            catch (Exception ex)
            {
                IsMatch = false;
            }
            return IsMatch;
        }

        #endregion

        #region Post

        [HttpPost]
        [Route("CreateOutlet")]
        public async Task<IActionResult> CreateOutlet(CreateOutletCommand command)
        {
            if (!VerifyUserAccess(command.MerchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return BadRequest(badRequestResponse);
            }
            command.IsActivated = true;
            command.CreatedAt = DateTime.Now;
            command.CreatedByUserId = new Guid(User.Identity.GetUserId());
            var result = await Mediator.Send(command);
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost]
        [Route("DeleteOutletImages/{outletId}")]
        public async Task<ApiResponseViewModel> DeleteOutletImages(int outletId, string[] removeFiles)
        {
            if (!await VerifyUserAccessWithOutletId(outletId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();

            if (removeFiles == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "No files to delete";
                return apiResponseViewModel;
            }
            DeleteFilesCommand command = new DeleteFilesCommand()
            {
                Id = outletId,
                Files = removeFiles,
                ContainerName = ContainerNameEnum.Outlets,
                FilePath = FilePathEnum.Outlets_Images
            }
            ;
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateMerchantDefaultCommission")]
        public async Task<ApiResponseViewModel> UpdateMerchantDefaultCommission([FromForm] UpdateMerchantDefaultCommissionCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UploadMerchantLogo/{merchantId}")]
        public async Task<ApiResponseViewModel> UploadMerchantLogo(int merchantId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContent = HttpContext.Request.Form.Files["EditCompanyLogo"];
            if (fileContent == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            CreateFileCommand command = new CreateFileCommand()
            {
                Id = merchantId,
                File = fileContent,
                ContainerName = ContainerNameEnum.Merchants,
                FilePath = FilePathEnum.Merchants_Logo
            };
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UploadMerchantBusinessDocument/{merchantId}")]
        public async Task<ApiResponseViewModel> UploadMerchantBusinessDocument(int merchantId)
        {

            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContent = HttpContext.Request.Form.Files["BusinessDocument"];
            if (fileContent == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            CreateFileCommand command = new CreateFileCommand()
            {
                Id = merchantId,
                File = fileContent,
                ContainerName = ContainerNameEnum.Merchants,
                FilePath = FilePathEnum.Merchants_BusinessInfo_Documents
            };
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;

            //ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            //var fileContent = HttpContext.Request.Form.Files["BusinessDocument"];
            //if (fileContent == null)
            //{
            //    apiResponseViewModel.Successful = false;
            //    apiResponseViewModel.Message = "Invalid request";
            //    return apiResponseViewModel;
            //}
            //CreateBusinessDocumentCommand command = new CreateBusinessDocumentCommand() { MerchantId = merchantId, File = fileContent };
            //ApiResponseViewModel response = await Mediator.Send(command);
            //return response;
        }

        [HttpPost]
        [Route("UploadMerchantPersonInChargeDocument/{merchantId}")]
        public async Task<ApiResponseViewModel> UploadMerchantPersonInChargeDocument(int merchantId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContent = HttpContext.Request.Form.Files["Document"];
            if (fileContent == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            CreateFileCommand command = new CreateFileCommand()
            {
                Id = merchantId,
                File = fileContent,
                ContainerName = ContainerNameEnum.Merchants,
                FilePath = FilePathEnum.Merchants_PersonInCharge_Documents
            };
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UploadMerchantBankAccountDocument/{merchantId}")]
        public async Task<ApiResponseViewModel> UploadMerchantBankAccountDocument(int merchantId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContent = HttpContext.Request.Form.Files["Document"];
            if (fileContent == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            CreateFileCommand command = new CreateFileCommand()
            {
                Id = merchantId,
                File = fileContent,
                ContainerName = ContainerNameEnum.Merchants,
                FilePath = FilePathEnum.Merchants_BankAccount_Documents
            };
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("UpdateMerchantBrandShownInHomePageStatus")]
        public async Task<ApiResponseViewModel> UpdateMerchantBrandShownInHomePageStatus([FromForm] UpdateMerchantBrandShownInHomePageStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateMerchantPublishedStatus")]
        public async Task<ApiResponseViewModel> UpdateMerchantPublishedStatus([FromForm] UpdateMerchantPublishedStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateMerchantTestAccountStatus")]
        public async Task<ApiResponseViewModel> UpdateMerchantTestAccountStatus([FromForm] UpdateMerchantTestAccountStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateMerchant")]
        public async Task<ApiResponseViewModel> UpdateMerchant([FromForm] UpdateMerchantCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }


        [HttpPost]
        [Route("UpdatePersonInCharge")]
        public async Task<ApiResponseViewModel> UpdatePersonInCharge([FromForm] UpdatePersonInChargeCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateBankAccount")]
        public async Task<ApiResponseViewModel> UpdateBankAccount([FromForm] UpdateBankAccountCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateMerchantStatus")]
        public async Task<ApiResponseViewModel> UpdateMerchantStatus([FromForm] UpdateMerchantPendingChangesStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (command.StatusTypeId == StatusTypeEnum.APPROVED)
            {
                ApiResponseViewModel statusresponse = await Mediator.Send(new UpdateMerchantStatusCommand()
                {
                    MerchantId = command.MerchantId,
                    LastUpdatedAt = command.LastUpdatedAt,
                    LastUpdatedByUserId = command.LastUpdatedByUserId,
                    Remarks = response.Data.ToString(),
                    StatusTypeId = command.StatusTypeId
                });
                if (!statusresponse.Successful)
                {
                    return statusresponse;
                }
                UpdateMerchantPublishedStatusCommand publishStatusCommand = new UpdateMerchantPublishedStatusCommand();
                publishStatusCommand.LastUpdatedAt = command.LastUpdatedAt;
                publishStatusCommand.LastUpdatedByUserId = command.LastUpdatedByUserId;
                publishStatusCommand.MerchantId = command.MerchantId;
                publishStatusCommand.Status = true;                
                ApiResponseViewModel publishStatusResponse = await Mediator.Send(publishStatusCommand);
                if (!publishStatusResponse.Successful)
                {
                    return publishStatusResponse;
                }
                ApiResponseViewModel merchantResponse = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = command.MerchantId });

                var sasToken = await Mediator.Send(new SASTokenQuery());

                if (merchantResponse.Successful)
                {
                    var merchant = (MerchantModel)merchantResponse.Data;
                    var BIDDocumentUrl = merchant.BIDDocumentUrl;
                    var LogoUrl = merchant.LogoUrl;
                    if (!String.IsNullOrEmpty(merchant.BIDDocumentUrl))
                    {
                        ApiResponseViewModel documentResponse = await Mediator.Send(new CreateFileWithUrlCommand()
                        {
                            Id = command.MerchantId,
                            Url = merchant.BIDDocumentUrl.Split("?")[0],
                            ContainerName = ContainerNameEnum.Merchants,
                            FilePath = FilePathEnum.Merchants_BusinessInfo_Documents,
                            SASToken = sasToken.Data.ToString()
                        });
                        if (documentResponse.Successful)
                        {
                            BIDDocumentUrl = documentResponse.Data.ToString();
                        }
                    }

                    if (!String.IsNullOrEmpty(merchant.LogoUrl))
                    {
                        ApiResponseViewModel documentResponse = await Mediator.Send(new CreateFileWithUrlCommand()
                        {
                            Id = command.MerchantId,
                            Url = merchant.LogoUrl,
                            ContainerName = ContainerNameEnum.MerchantsLogos,
                            FilePath = FilePathEnum.Merchants_Logo
                        });
                        if (documentResponse.Successful)
                        {
                            LogoUrl = documentResponse.Data.ToString();
                        }
                    }

                    UpdateMerchantCommand merchantCommand = new UpdateMerchantCommand()
                    {
                        MerchantId = merchant.Id,
                        LogoUrl = LogoUrl,
                        Description = merchant.Description,
                        BIDDocumentUrl = BIDDocumentUrl,
                        Address_1 = merchant.Address_1,
                        Address_2 = merchant.Address_2,
                        CompanyContact = merchant.CompanyContact,
                        CompanyName = merchant.CompanyName,
                        CompanyTypeId = merchant.BusinessTypeId.Value,
                        CompanyWebsite = merchant.WebsiteSiteUrl,
                        CountryId = merchant.CountryId.Value,
                        DisplayName = merchant.DisplayName,
                        DistrictId = merchant.DistritId.Value,
                        PostcodeId = merchant.PostCodeId.Value,
                        ProvinceId = merchant.ProvinceId.Value,
                        RegistrationNumber = merchant.RegistrationNumber,
                        LastUpdatedAt = command.LastUpdatedAt,
                        LastUpdatedByUserId = command.LastUpdatedByUserId
                    };
                    ApiResponseViewModel updateMerchantResponse = await Mediator.Send(merchantCommand);
                    if (!updateMerchantResponse.Successful)
                    {
                        return updateMerchantResponse;
                    }
                }


            }
            return response;
        }

        [HttpPost]
        [Route("UpdatePersonInChargeStatus/{merchantId}")]
        public async Task<ApiResponseViewModel> UpdatePersonInChargeStatus(int merchantId, [FromForm] UpdatePersonInChargePendingChangesStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (command.StatusTypeId == StatusTypeEnum.APPROVED)
            {
                ApiResponseViewModel statusresponse = await Mediator.Send(new UpdatePersonInChargeStatusCommand()
                {
                    Id = command.PersonInChargeId,
                    LastUpdatedAt = command.LastUpdatedAt,
                    LastUpdatedByUserId = command.LastUpdatedByUserId,
                    Remarks = command.Remarks,
                    StatusTypeId = command.StatusTypeId
                });
                if (!statusresponse.Successful)
                {
                    return statusresponse;
                }

                ApiResponseViewModel personInChargeResponse = await Mediator.Send(new PersonInChargePendingChangesQuery() { PersonInChargeId = command.PersonInChargeId });
                if (personInChargeResponse.Successful)
                {
                    var personInCharge = (PersonInChargeModel)personInChargeResponse.Data;
                    var documentUrl = personInCharge.DocumentUrl;
                    var sasToken = await Mediator.Send(new SASTokenQuery());
                    if (!String.IsNullOrEmpty(personInCharge.DocumentUrl))
                    {
                        ApiResponseViewModel documentResponse = await Mediator.Send(new CreateFileWithUrlCommand()
                        {
                            Id = merchantId,
                            Url = personInCharge.DocumentUrl,
                            ContainerName = ContainerNameEnum.Merchants,
                            FilePath = FilePathEnum.Merchants_PersonInCharge_Documents,
                            SASToken = sasToken.Data.ToString()
                        });
                        if (documentResponse.Successful)
                        {
                            documentUrl = documentResponse.Data.ToString();
                        }
                    }

                    UpdatePersonInChargeCommand personInChargeCommand = new UpdatePersonInChargeCommand()
                    {
                        Id = personInCharge.Id,
                        Contact = personInCharge.Contact,
                        DocumentUrl = documentUrl,
                        IdentityNumber = personInCharge.IdentityNumber,
                        LastUpdatedAt = command.LastUpdatedAt,
                        LastUpdatedByUserId = command.LastUpdatedByUserId,
                        Name = personInCharge.Name,
                        Position = personInCharge.Position
                    };
                    ApiResponseViewModel updatePersonInChargeResponse = await Mediator.Send(personInChargeCommand);
                    if (!updatePersonInChargeResponse.Successful)
                    {
                        return updatePersonInChargeResponse;
                    }
                }


            }
            return response;
        }

        [HttpPost]
        [Route("UpdateBankAccountStatus/{merchantId}")]
        public async Task<ApiResponseViewModel> UpdateBankAccountStatus(int merchantId, [FromForm] UpdateBankAccountPendingChangesStatusCommand command)
        {
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (command.StatusTypeId == StatusTypeEnum.APPROVED)
            {
                ApiResponseViewModel statusresponse = await Mediator.Send(new UpdateBankAccountStatusCommand()
                {
                    Id = command.BankAccountId,
                    LastUpdatedAt = command.LastUpdatedAt,
                    LastUpdatedByUserId = command.LastUpdatedByUserId,
                    Remarks = command.Remarks,
                    StatusTypeId = command.StatusTypeId
                });
                if (!statusresponse.Successful)
                {
                    return statusresponse;
                }

                ApiResponseViewModel bankAccountResponse = await Mediator.Send(new BankAccountPendingChangesQuery() { Id = command.BankAccountId });
                if (bankAccountResponse.Successful)
                {
                    var sasToken = await Mediator.Send(new SASTokenQuery());
                    var bankAccount = (BankAccountModel)bankAccountResponse.Data;
                    var documentUrl = bankAccount.DocumentUrl;
                    if (!String.IsNullOrEmpty(bankAccount.DocumentUrl))
                    {
                        ApiResponseViewModel documentResponse = await Mediator.Send(new CreateFileWithUrlCommand()
                        {
                            Id = merchantId,
                            Url = bankAccount.DocumentUrl,
                            ContainerName = ContainerNameEnum.Merchants,
                            FilePath = FilePathEnum.Merchants_BankAccount_Documents,
                            SASToken = sasToken.Data.ToString()
                        });
                        if (documentResponse.Successful)
                        {
                            documentUrl = documentResponse.Data.ToString();
                        }
                    }

                    UpdateBankAccountCommand bankAccountCommand = new UpdateBankAccountCommand()
                    {
                        Id = bankAccount.Id,
                        BankId = bankAccount.BankId.Value,
                        DocumentUrl = documentUrl,
                        Name = bankAccount.Name,
                        AccountNumber = bankAccount.AccountNumber,
                        LastUpdatedAt = command.LastUpdatedAt,
                        LastUpdatedByUserId = command.LastUpdatedByUserId
                    };
                    ApiResponseViewModel updateBankAccountResponse = await Mediator.Send(bankAccountCommand);
                    if (!updateBankAccountResponse.Successful)
                    {
                        return updateBankAccountResponse;
                    }
                }


            }

            return response;
        }
        #endregion

        #region Get
        [HttpGet]
        [Route("GetMerchantOutlets/{merchantId}")]
        public async Task<ApiResponseViewModel> GetMerchantOutlets(int merchantId)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            return await Mediator.Send(new MerchantOutletListQuery() { MerchantId = merchantId });
        }

        [HttpGet]
        [Route("GetMerchantPendingChanges/{merchantId}")]
        public async Task<ApiResponseViewModel> GetMerchantPendingChanges(int merchantId)
        {
            ApiResponseViewModel response = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = merchantId });
            return response;
        }

        [HttpGet]
        [Route("GetPersonInChargePendingChange/{merchantId}")]
        public async Task<ApiResponseViewModel> GetPersonInChargePendingChange(int merchantId)
        {

            ApiResponseViewModel response = await Mediator.Send(new MerchantPersonInChargePendingChangesQuery() { MerchantId = merchantId });
            return response;
        }

        [HttpGet]
        [Route("GetBankAccountPendingChanges/{merchantId}")]
        public async Task<ApiResponseViewModel> GetBankAccountPendingChanges(int merchantId)
        {
            ApiResponseViewModel response = await Mediator.Send(new MerchantBankAccountPendingChangesQuery() { MerchantId = merchantId });
            return response;
        }

        [HttpGet]
        [Route("GetPostcodeList/{districtId}")]
        public async Task<ApiResponseViewModel> GetPostcodeList(int districtId)
        {
            ApiResponseViewModel response = await Mediator.Send(new PostcodeListQuery() { DistrictId = districtId });
            return response;
        }

        [HttpGet]
        [Route("GetDistrictList/{provinceId}")]
        public async Task<ApiResponseViewModel> GetDistrictList(int provinceId)
        {
            ApiResponseViewModel response = await Mediator.Send(new DistrictListQuery() { ProvinceId = provinceId });
            return response;
        }

        [HttpGet]
        [Route("GetProvinceList/{countryId}")]
        public async Task<ApiResponseViewModel> GetProvinceList(int countryId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProvinceListQuery() { CountryId = countryId });
            return response;
        }

        [HttpGet]
        [Route("GetCountryList")]
        public async Task<ApiResponseViewModel> GetCountryList()
        {
            ApiResponseViewModel response = await Mediator.Send(new CountryListQuery());
            return response;
        }


        [HttpGet]
        [Route("GetStatusTypeList")]
        public async Task<ApiResponseViewModel> GetStatusTypeList()
        {
            ApiResponseViewModel response = await Mediator.Send(new StatusTypeListQuery());
            return response;
        }

        [HttpGet]
        [Route("GetBusinessTypeList")]
        public async Task<ApiResponseViewModel> GetBusinessTypeList()
        {
            ApiResponseViewModel response = await Mediator.Send(new BusinessTypeListQuery());
            return response;
        }

        [HttpGet]
        [Route("GetBankList")]
        public async Task<ApiResponseViewModel> GetBankList()
        {
            ApiResponseViewModel response = await Mediator.Send(new BankListQuery());
            return response;
        }

        #endregion
    }
}