using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Services.Identity.Commands;
using System.Security.Claims;
using Voupon.Common.BaseTypes;
using Voupon.Merchant.WebApp.Areas.App.Services.IdentityExtension;
using Voupon.Merchant.WebApp.Areas.App.Services.Business.ViewModels;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Merchant.WebApp.Common.Services.Banks.Queries;
using Voupon.Merchant.WebApp.Common.Services.BusinessTypes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Queries;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Queries;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Models;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Models;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Commands;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Commands;
using Voupon.Merchant.WebApp.Common.Services.BankAccounts.Commands;
using Voupon.Common.Enum;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Create;
using Voupon.Common.Azure.Blob;
using Voupon.Merchant.WebApp.Common.Services.Blob.Queries;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant")]
    public class BusinessController : BaseAppController
    {
        public async Task<IActionResult> Index()
        {
            var userid = User.Identity.GetUserId();
            var merchantId = int.Parse(User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value);

            if (merchantId == 0)
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
            }

            var merchantBusinessDetails = new BusinessDetailsViewModel();

            var bankAccountDetalsResponse = await Mediator.Send(new MerchantBankAccountPendingChangesQuery() { MerchantId = merchantId });
            if (bankAccountDetalsResponse.Successful)
            {
                merchantBusinessDetails.BankAccount = (BankAccountModel)bankAccountDetalsResponse.Data;
            }
            var personInChargeDetalsResponse = await Mediator.Send(new MerchantPersonInChargePendingChangesQuery() { MerchantId = merchantId });
            if (personInChargeDetalsResponse.Successful)
            {
                merchantBusinessDetails.PersonInCharge = (PersonInChargeModel)personInChargeDetalsResponse.Data;
            }
            var merchantDetailsResponse = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = merchantId });
            if (merchantDetailsResponse.Successful)
            {
                merchantBusinessDetails.Merchant = (MerchantModel)merchantDetailsResponse.Data;
            }

            var sasTokenResult = await Mediator.Send(new SASTokenQuery());
            if (sasTokenResult.Successful)
            {
                if(merchantBusinessDetails.Merchant != null)
                {
                    if (merchantBusinessDetails.Merchant.BIDDocumentUrl != "" && merchantBusinessDetails.Merchant.BIDDocumentUrl != null)
                        merchantBusinessDetails.Merchant.BIDDocumentUrl = merchantBusinessDetails.Merchant.BIDDocumentUrl;
                }
                
                if(merchantBusinessDetails.BankAccount != null)
                {
                    if (merchantBusinessDetails.BankAccount.DocumentUrl != "" && merchantBusinessDetails.BankAccount.DocumentUrl != null)
                        merchantBusinessDetails.BankAccount.DocumentUrl = merchantBusinessDetails.BankAccount.DocumentUrl;
                }
                
                if(merchantBusinessDetails.PersonInCharge != null)
                {
                    if (merchantBusinessDetails.PersonInCharge.DocumentUrl != "" && merchantBusinessDetails.PersonInCharge.DocumentUrl != null)
                        merchantBusinessDetails.PersonInCharge.DocumentUrl = merchantBusinessDetails.PersonInCharge.DocumentUrl;
                }
                
            }

            return View(merchantBusinessDetails);
        }


        [HttpGet]
        [Route("GetMerchantPendingChanges/{merchantId}")]
        public async Task<ApiResponseViewModel> GetMerchantDetails(int merchantId)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            if (!VerifyUserAccess(merchantId))
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid credentials";
                return apiResponseViewModel;
            }
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;

            var result = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = merchantId });
            var sasTokenResult = await Mediator.Send(new SASTokenQuery());
            if (sasTokenResult.Successful)
            {
                var model = (MerchantModel)result.Data;
                model.BIDDocumentUrl = model.BIDDocumentUrl + sasTokenResult.Data.ToString();
            }

            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("GetPersonInChargePendingChange/{merchantId}")]
        public async Task<ApiResponseViewModel> GetPersonInChargeDetails(int merchantId)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            if (!VerifyUserAccess(merchantId))
            {

                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid credentials";
                return apiResponseViewModel;
            }
            var result = await Mediator.Send(new MerchantPersonInChargePendingChangesQuery() { MerchantId = merchantId });

            var sasTokenResult = await Mediator.Send(new SASTokenQuery());
            if (sasTokenResult.Successful)
            {
                var model = (PersonInChargeModel)result.Data;
                model.DocumentUrl = model.DocumentUrl + sasTokenResult.Data.ToString();
            }


            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("GetBankAccountPendingChanges/{merchantId}")]
        public async Task<ApiResponseViewModel> GetBankAccountDetails(int merchantId)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            if (!VerifyUserAccess(merchantId))
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid credentials";
                return apiResponseViewModel;
            }
            var result = await Mediator.Send(new MerchantBankAccountPendingChangesQuery() { MerchantId = merchantId });

            var model = (BankAccountModel)result.Data;
            var sasTokenResult = await Mediator.Send(new SASTokenQuery());
            if (sasTokenResult.Successful)
            {
                model.DocumentUrl = model.DocumentUrl + sasTokenResult.Data.ToString();
            }

            apiResponseViewModel.Data = (BankAccountModel)result.Data;
            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("GetBankList")]
        public async Task<ApiResponseViewModel> GetBankList()
        {
            ApiResponseViewModel response = await Mediator.Send(new BankListQuery());
            return response;
        }

        [HttpGet]
        [Route("GetBusinessTypeList")]
        public async Task<ApiResponseViewModel> GetBusinessTypeList()
        {
            ApiResponseViewModel response = await Mediator.Send(new BusinessTypeListQuery());
            return response;
        }

        [HttpPost]
        [Route("UploadMerchantPersonInChargeDocument/{merchantId}")]
        public async Task<ApiResponseViewModel> UploadMerchantPersonInChargeDocument(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContent = HttpContext.Request.Form.Files["Document"];
            if (fileContent == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            if (fileContent.FileName.Substring(fileContent.FileName.LastIndexOf(".")).ToLower() == ".pdf" && fileContent.Length / 1000000 < 4)
            {
                CreateFileCommand command = new CreateFileCommand()
                {
                    Id = merchantId,
                    File = fileContent,
                    ContainerName = ContainerNameEnum.Merchants,
                    FilePath = FilePathEnum.Temporary + "/" + FilePathEnum.Merchants_PersonInCharge_Documents,
                    IsPublic = false
                };
                var response = await Mediator.Send(command);

                var sasTokenResult = await Mediator.Send(new SASTokenQuery());
                if (sasTokenResult.Successful)
                {
                    response.Data = response.Data + sasTokenResult.Data.ToString();
                }

                return response;
            }
            else
            {
                ApiResponseViewModel respons = new ApiResponseViewModel();
                respons.Successful = false;
                respons.Message = "Please upload files having .pdf extension";
                return respons;
            }
        }

        [HttpPost]
        [Route("UploadMerchantDocument/{merchantId}")]
        public async Task<ApiResponseViewModel> UploadMerchantDocument(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContent = HttpContext.Request.Form.Files["Document"];
            if (fileContent == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            if (fileContent.FileName.Substring(fileContent.FileName.LastIndexOf(".")).ToLower() == ".pdf" && fileContent.Length / 1000000 < 4)
            {
                CreateFileCommand command = new CreateFileCommand()
                {
                    Id = merchantId,
                    File = fileContent,
                    ContainerName = ContainerNameEnum.Merchants,
                    FilePath = FilePathEnum.Temporary + "/" + FilePathEnum.Merchants_BusinessInfo_Documents,
                    IsPublic = false
                };
                var response = await Mediator.Send(command);
                var sasTokenResult = await Mediator.Send(new SASTokenQuery());
                if (sasTokenResult.Successful)
                {
                    response.Data = response.Data + sasTokenResult.Data.ToString();
                }
                return response;
            }
            else
            {
                ApiResponseViewModel respons = new ApiResponseViewModel();
                respons.Successful = false;
                respons.Message = "Please upload files having .pdf extension";
                return respons;
            }
        }

        [HttpPost]
        [Route("UploadMerchantLogo/{merchantId}")]
        public async Task<ApiResponseViewModel> UploadMerchantLogo(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
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
                ContainerName = ContainerNameEnum.MerchantsLogos,
                FilePath = FilePathEnum.Temporary + "/" + FilePathEnum.Merchants_Logo,
                IsPublic = true
            };
            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UploadMerchantBankAccountDocument/{merchantId}")]
        public async Task<ApiResponseViewModel> UploadMerchantBankAccountDocument(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var fileContent = HttpContext.Request.Form.Files["Document"];
            if (fileContent == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return apiResponseViewModel;
            }
            if (fileContent.FileName.Substring(fileContent.FileName.LastIndexOf(".")).ToLower() == ".pdf" && fileContent.Length / 1000000 < 4)
            {
                CreateFileCommand command = new CreateFileCommand()
                {
                    Id = merchantId,
                    File = fileContent,
                    ContainerName = ContainerNameEnum.Merchants,
                    FilePath = FilePathEnum.Temporary + "/" + FilePathEnum.Merchants_BankAccount_Documents,
                    IsPublic = false
                };
                var response = await Mediator.Send(command);

                var sasTokenResult = await Mediator.Send(new SASTokenQuery());
                if (sasTokenResult.Successful)
                {
                    response.Data = response.Data + sasTokenResult.Data.ToString();
                }

                return response;
            }
            else
            {
                ApiResponseViewModel respons = new ApiResponseViewModel();
                respons.Successful = false;
                respons.Message = "Please upload files having .pdf extension";
                return respons;
            }
        }

        [HttpPost]
        [Route("UpdateMerchant")]
        public async Task<ApiResponseViewModel> UpdateMerchant([FromForm] UpdateMerchantPendingChangesCommand command)
        {
            if (!VerifyUserAccess(command.MerchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            #region Verify Input
            //if(String.IsNullOrEmpty(command.Address_1) || String.IsNullOrEmpty(command.BIDDocumentUrl) 
            //    || String.IsNullOrEmpty(command.CompanyContact) || String.IsNullOrEmpty(command.CompanyName)
            //    || !command.CompanyTypeId.HasValue || String.IsNullOrEmpty(command.CompanyWebsite)
            //    || !command.CountryId.HasValue  || String.IsNullOrEmpty(command.Description)
            //    || String.IsNullOrEmpty(command.DisplayName) || !command.DistrictId.HasValue
            //    || String.IsNullOrEmpty(command.LogoUrl)  || String.IsNullOrEmpty(command.MerchantId.ToString())
            //    || !command.PostcodeId.HasValue || !command.ProvinceId.HasValue 
            //    || String.IsNullOrEmpty(command.RegistrationNumber) || String.IsNullOrEmpty(command.StatusTypeId.ToString())
            //    )
            //{
            //    ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
            //    InvalidResponse.Successful = false;
            //    InvalidResponse.Message = "Information not completed";
            //    return InvalidResponse;
            //}
            #endregion

            command.StatusTypeId = StatusTypeEnum.PENDING_REVIEW;
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (response.Successful)
            {
                ApiResponseViewModel merchantResponse = await Mediator.Send(new MerchantQuery() { MerchantId = command.MerchantId });
                if (merchantResponse.Successful)
                {
                    var merchant = (MerchantModel)merchantResponse.Data;
                    if (merchant.StatusTypeId != StatusTypeEnum.APPROVED)
                    {
                        ApiResponseViewModel merchantStatusResponse = await Mediator.Send(new UpdateMerchantStatusCommand()
                        {
                            MerchantId = command.MerchantId,
                            StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                            Remarks = "",
                            LastUpdatedAt = command.LastUpdatedAt,
                            LastUpdatedByUserId = command.LastUpdatedByUserId
                        });
                        if (!merchantStatusResponse.Successful)
                            return merchantStatusResponse;
                    }
                    await Mediator.Send(new CheckMerchantInfoStatusesForSendingEmail() { MerchantId = command.MerchantId });

                }
            }
            return response;
        }

        [HttpPost]
        [Route("SubmitReviewMerchant")]
        public async Task<ApiResponseViewModel> SubmitReviewMerchant([FromForm] UpdateMerchantPendingChangesStatusCommand command)
        {
            if (!VerifyUserAccess(command.MerchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            #region Verify Input
            ApiResponseViewModel merchantQueryResponse = await Mediator.Send(new MerchantPendingChangesQuery() { MerchantId = command.MerchantId });
            if (merchantQueryResponse.Successful)
            {
                var merchant = (MerchantModel)merchantQueryResponse.Data;
                if (String.IsNullOrEmpty(merchant.Address_1) /*|| String.IsNullOrEmpty(merchant.BIDDocumentUrl)*/
                    || String.IsNullOrEmpty(merchant.CompanyContact) || String.IsNullOrEmpty(merchant.CompanyName)
                    || !merchant.BusinessTypeId.HasValue || String.IsNullOrEmpty(merchant.WebsiteSiteUrl)
                    || !merchant.CountryId.HasValue || String.IsNullOrEmpty(merchant.Description)
                    || String.IsNullOrEmpty(merchant.DisplayName) || !merchant.DistritId.HasValue
                    || String.IsNullOrEmpty(merchant.LogoUrl) || String.IsNullOrEmpty(merchant.Id.ToString())
                    || !merchant.PostCodeId.HasValue || !merchant.ProvinceId.HasValue
                    || String.IsNullOrEmpty(merchant.RegistrationNumber) || String.IsNullOrEmpty(merchant.StatusTypeId.ToString())
                    )
                {
                    ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
                    InvalidResponse.Successful = false;
                    InvalidResponse.Message = "Information not completed";
                    return InvalidResponse;
                }
            }
            else
            {
                ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
                InvalidResponse.Successful = false;
                InvalidResponse.Message = "Information not completed";
                return InvalidResponse;
            }
            #endregion

            command.StatusTypeId = StatusTypeEnum.PENDING_REVIEW;
            command.Remarks = command.Remarks;
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (response.Successful)
            {
                ApiResponseViewModel merchantResponse = await Mediator.Send(new MerchantQuery() { MerchantId = command.MerchantId });
                if (merchantResponse.Successful)
                {
                    var merchant = (MerchantModel)merchantResponse.Data;
                    if (merchant.StatusTypeId != StatusTypeEnum.APPROVED)
                    {
                        ApiResponseViewModel merchantStatusResponse = await Mediator.Send(new UpdateMerchantStatusCommand()
                        {
                            MerchantId = command.MerchantId,
                            StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                            Remarks = command.Remarks,
                            LastUpdatedAt = command.LastUpdatedAt,
                            LastUpdatedByUserId = command.LastUpdatedByUserId
                        });
                        if (!merchantStatusResponse.Successful)
                            return merchantStatusResponse;
                    }
                }
            }
            return response;
        }


        [HttpPost]
        [Route("UpdatePersonInCharge")]
        public async Task<ApiResponseViewModel> UpdatePersonInCharge([FromForm] UpdatePersonInChargePendingChangesCommand command)
        {
            #region Verify Input
            //if (String.IsNullOrEmpty(command.Contact) || String.IsNullOrEmpty(command.DocumentUrl)
            //    || String.IsNullOrEmpty(command.Id.ToString()) || String.IsNullOrEmpty(command.IdentityNumber)           
            //    || String.IsNullOrEmpty(command.Name) || String.IsNullOrEmpty(command.Position)
            //    || String.IsNullOrEmpty(command.StatusTypeId.ToString())
            //    )
            //{
            //    ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
            //    InvalidResponse.Successful = false;
            //    InvalidResponse.Message = "Information not completed";
            //    return InvalidResponse;
            //}
            #endregion

            command.StatusTypeId = StatusTypeEnum.PENDING_REVIEW;
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (response.Successful)
            {
                ApiResponseViewModel personInChargeResponse = await Mediator.Send(new PersonInChargeQuery() { PersonInChargeId = command.Id });

                if (personInChargeResponse.Successful)
                {
                    var personInCharge = (PersonInChargeModel)personInChargeResponse.Data;
                    if (!VerifyUserAccess(personInCharge.MerchantId))
                    {
                        ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                        badRequestResponse.Successful = false;
                        badRequestResponse.Message = "Invalid credentials";
                        return badRequestResponse;
                    }
                    if (personInCharge.StatusTypeId != StatusTypeEnum.APPROVED)
                    {
                        ApiResponseViewModel personInChargeStatusResponse = await Mediator.Send(new UpdatePersonInChargeStatusCommand()
                        {
                            Id = command.Id,
                            StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                            Remarks = "",
                            LastUpdatedAt = command.LastUpdatedAt,
                            LastUpdatedByUserId = command.LastUpdatedByUserId
                        });
                        if (!personInChargeStatusResponse.Successful)
                            return personInChargeStatusResponse;
                    }
                    await Mediator.Send(new CheckMerchantInfoStatusesForSendingEmail() { MerchantId = personInCharge.MerchantId });

                }
            }
            return response;
        }

        [HttpPost]
        [Route("SubmitReviewPersonInCharge")]
        public async Task<ApiResponseViewModel> SubmitReviewPersonInCharge([FromForm] UpdatePersonInChargePendingChangesStatusCommand command)
        {
            #region Verify Input
            ApiResponseViewModel personInChargeQueryResponse = await Mediator.Send(new PersonInChargePendingChangesQuery() { PersonInChargeId = command.PersonInChargeId });
            if (personInChargeQueryResponse.Successful)
            {
                var personInCharge = (PersonInChargeModel)personInChargeQueryResponse.Data;
                if (String.IsNullOrEmpty(personInCharge.Contact) /*|| String.IsNullOrEmpty(personInCharge.DocumentUrl)*/
                    || String.IsNullOrEmpty(personInCharge.Id.ToString()) || String.IsNullOrEmpty(personInCharge.IdentityNumber)
                    || String.IsNullOrEmpty(personInCharge.Name) || String.IsNullOrEmpty(personInCharge.Position)
                    || String.IsNullOrEmpty(personInCharge.StatusTypeId.ToString())
                    )
                {
                    ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
                    InvalidResponse.Successful = false;
                    InvalidResponse.Message = "Information not completed";
                    return InvalidResponse;
                }
            }
            else
            {
                ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
                InvalidResponse.Successful = false;
                InvalidResponse.Message = "Information not completed";
                return InvalidResponse;
            }
            #endregion
            command.Remarks = command.Remarks;
            command.StatusTypeId = StatusTypeEnum.PENDING_REVIEW;
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (response.Successful)
            {
                ApiResponseViewModel personInChargeResponse = await Mediator.Send(new PersonInChargeQuery() { PersonInChargeId = command.PersonInChargeId });
                if (personInChargeResponse.Successful)
                {
                    var personInCharge = (PersonInChargeModel)personInChargeResponse.Data;
                    if (!VerifyUserAccess(personInCharge.MerchantId))
                    {
                        ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                        badRequestResponse.Successful = false;
                        badRequestResponse.Message = "Invalid credentials";
                        return badRequestResponse;
                    }
                    if (personInCharge.StatusTypeId != StatusTypeEnum.APPROVED)
                    {
                        ApiResponseViewModel personInChargeStatusResponse = await Mediator.Send(new UpdatePersonInChargeStatusCommand()
                        {
                            Id = command.PersonInChargeId,
                            StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                            Remarks = command.Remarks,
                            LastUpdatedAt = command.LastUpdatedAt,
                            LastUpdatedByUserId = command.LastUpdatedByUserId
                        });
                        if (!personInChargeStatusResponse.Successful)
                            return personInChargeStatusResponse;
                    }
                }
            }
            return response;
        }

        [HttpPost]
        [Route("UpdateBankAccount")]
        public async Task<ApiResponseViewModel> UpdateBankAccount([FromForm] UpdateBankAccountPendingChangesCommand command)
        {
            #region Verify Input
            //if (String.IsNullOrEmpty(command.Id.ToString()) || String.IsNullOrEmpty(command.Name)
            //    || String.IsNullOrEmpty(command.AccountNumber) || !command.BankId.HasValue
            //    || String.IsNullOrEmpty(command.DocumentUrl)  || String.IsNullOrEmpty(command.StatusTypeId.ToString())
            //    )
            //{
            //    ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
            //    InvalidResponse.Successful = false;
            //    InvalidResponse.Message = "Information not completed";
            //    return InvalidResponse;
            //}
            #endregion
            command.StatusTypeId = StatusTypeEnum.PENDING_REVIEW;
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (response.Successful)
            {
                ApiResponseViewModel bankAccountResponse = await Mediator.Send(new BankAccountQuery() { BankAccountId = command.Id });
                if (bankAccountResponse.Successful)
                {
                    var bankAccount = (BankAccountModel)bankAccountResponse.Data;
                    if (!VerifyUserAccess(bankAccount.MerchantId))
                    {
                        ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                        badRequestResponse.Successful = false;
                        badRequestResponse.Message = "Invalid credentials";
                        return badRequestResponse;
                    }
                    if (bankAccount.StatusTypeId != StatusTypeEnum.APPROVED)
                    {
                        ApiResponseViewModel bankAccountStatusResponse = await Mediator.Send(new UpdateBankAccountStatusCommand()
                        {
                            Id = command.Id,
                            StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                            Remarks = "",
                            LastUpdatedAt = command.LastUpdatedAt,
                            LastUpdatedByUserId = command.LastUpdatedByUserId
                        });
                        if (!bankAccountStatusResponse.Successful)
                            return bankAccountStatusResponse;
                    }
                    await Mediator.Send(new CheckMerchantInfoStatusesForSendingEmail() { MerchantId = bankAccount.MerchantId });

                }
            }
            return response;
        }

        [HttpPost]
        [Route("SubmitReviewBankAccount")]
        public async Task<ApiResponseViewModel> SubmitReviewBankAccount([FromForm] UpdateBankAccountPendingChangesStatusCommand command)
        {
            #region Verify Input
            ApiResponseViewModel bankAccountQueryResponse = await Mediator.Send(new BankAccountPendingChangesQuery() { Id = command.BankAccountId });
            if (bankAccountQueryResponse.Successful)
            {
                var bankAccount = (BankAccountModel)bankAccountQueryResponse.Data;
                if (String.IsNullOrEmpty(bankAccount.Id.ToString()) || String.IsNullOrEmpty(bankAccount.Name)
               || String.IsNullOrEmpty(bankAccount.AccountNumber) || !bankAccount.BankId.HasValue
               /*|| String.IsNullOrEmpty(bankAccount.DocumentUrl)*/ || String.IsNullOrEmpty(bankAccount.StatusTypeId.ToString())
               )
                {
                    ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
                    InvalidResponse.Successful = false;
                    InvalidResponse.Message = "Information not completed";
                    return InvalidResponse;
                }
            }
            else
            {
                ApiResponseViewModel InvalidResponse = new ApiResponseViewModel();
                InvalidResponse.Successful = false;
                InvalidResponse.Message = "Information not completed";
                return InvalidResponse;
            }
            #endregion
            command.Remarks = command.Remarks;
            command.StatusTypeId = StatusTypeEnum.PENDING_REVIEW;
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (response.Successful)
            {
                ApiResponseViewModel bankAccountResponse = await Mediator.Send(new BankAccountQuery() { BankAccountId = command.BankAccountId });
                if (bankAccountResponse.Successful)
                {
                    var bankAccount = (BankAccountModel)bankAccountResponse.Data;
                    if (!VerifyUserAccess(bankAccount.MerchantId))
                    {
                        ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                        badRequestResponse.Successful = false;
                        badRequestResponse.Message = "Invalid credentials";
                        return badRequestResponse;
                    }
                    if (bankAccount.StatusTypeId != StatusTypeEnum.APPROVED)
                    {
                        ApiResponseViewModel bankAccountStatusResponse = await Mediator.Send(new UpdateBankAccountStatusCommand()
                        {
                            Id = command.BankAccountId,
                            StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                            Remarks = command.Remarks,
                            LastUpdatedAt = command.LastUpdatedAt,
                            LastUpdatedByUserId = command.LastUpdatedByUserId
                        });
                        if (!bankAccountStatusResponse.Successful)
                            return bankAccountStatusResponse;
                    }
                }
            }
            return response;
        }

        [HttpPost]
        [Route("SendMessageToAdmin")]
        public async Task<ApiResponseViewModel> SendMessageToAdmin([FromForm] SendMessageToAdminCommand command)
        {
            command.Remarks = command.Remarks;
            command.StatusTypeId = StatusTypeEnum.PENDING_REVIEW;
            command.LastUpdatedAt = DateTime.Now;
            command.LastUpdatedByUserId = new Guid(User.Identity.GetUserId());
            ApiResponseViewModel response = await Mediator.Send(command);
            if (response.Successful)
            {
                ApiResponseViewModel updateStatusResponse = await UpdateMerchantInfoStatuses(command);
                if(!updateStatusResponse.Successful)
                {
                    return updateStatusResponse;
                }
            }
            return response;
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<ApiResponseViewModel> UpdateMerchantInfoStatuses(SendMessageToAdminCommand command)
        {
            ApiResponseViewModel bankAccountResponse = await Mediator.Send(new BankAccountQuery() { BankAccountId = command.BankAccountId });
            if (bankAccountResponse.Successful)
            {
                var bankAccount = (BankAccountModel)bankAccountResponse.Data;
                if (!VerifyUserAccess(bankAccount.MerchantId))
                {
                    ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                    badRequestResponse.Successful = false;
                    badRequestResponse.Message = "Invalid credentials";
                    return badRequestResponse;
                }
                if (bankAccount.StatusTypeId != StatusTypeEnum.APPROVED)
                {
                    ApiResponseViewModel bankAccountStatusResponse = await Mediator.Send(new UpdateBankAccountStatusCommand()
                    {
                        Id = command.BankAccountId,
                        StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                        Remarks = command.Remarks,
                        LastUpdatedAt = command.LastUpdatedAt,
                        LastUpdatedByUserId = command.LastUpdatedByUserId
                    });
                    if (!bankAccountStatusResponse.Successful)
                        return bankAccountStatusResponse;
                }
            }

            ApiResponseViewModel personInChargeResponse = await Mediator.Send(new PersonInChargeQuery() { PersonInChargeId = command.PersonInChargeId });

            if (personInChargeResponse.Successful)
            {
                var personInCharge = (PersonInChargeModel)personInChargeResponse.Data;
                if (!VerifyUserAccess(personInCharge.MerchantId))
                {
                    ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                    badRequestResponse.Successful = false;
                    badRequestResponse.Message = "Invalid credentials";
                    return badRequestResponse;
                }
                if (personInCharge.StatusTypeId != StatusTypeEnum.APPROVED)
                {
                    ApiResponseViewModel personInChargeStatusResponse = await Mediator.Send(new UpdatePersonInChargeStatusCommand()
                    {
                        Id = command.PersonInChargeId,
                        StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                        Remarks = "",
                        LastUpdatedAt = command.LastUpdatedAt,
                        LastUpdatedByUserId = command.LastUpdatedByUserId
                    });
                    if (!personInChargeStatusResponse.Successful)
                        return personInChargeStatusResponse;
                }
            }

            ApiResponseViewModel merchantResponse = await Mediator.Send(new MerchantQuery() { MerchantId = command.MerchantId });
            if (merchantResponse.Successful)
            {
                var merchant = (MerchantModel)merchantResponse.Data;
                if (merchant.StatusTypeId != StatusTypeEnum.APPROVED)
                {
                    ApiResponseViewModel merchantStatusResponse = await Mediator.Send(new UpdateMerchantStatusCommand()
                    {
                        MerchantId = command.MerchantId,
                        StatusTypeId = StatusTypeEnum.PENDING_REVIEW,
                        Remarks = command.Remarks,
                        LastUpdatedAt = command.LastUpdatedAt,
                        LastUpdatedByUserId = command.LastUpdatedByUserId
                    });
                    if (!merchantStatusResponse.Successful)
                        return merchantStatusResponse;
                }
            }
            return merchantResponse;
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
    }
}