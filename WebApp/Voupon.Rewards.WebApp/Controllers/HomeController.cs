using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Merchants.Models;
using Voupon.Rewards.WebApp.Common.Merchants.Queries;
using Voupon.Rewards.WebApp.Common.ProductCategories.Models;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.Common.Products.Queries;
using Voupon.Rewards.WebApp.Common.ProductSubcategories.Models;
using Voupon.Rewards.WebApp.Common.ProductSubcategories.Queries;
using Voupon.Rewards.WebApp.Models;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Rewards.WebApp.Common.ProductCategories.Queries;
using Voupon.Rewards.WebApp.Common.ProductDiscounts.Queries;
using Voupon.Rewards.WebApp.Infrastructures.Enums;
using Voupon.Rewards.WebApp.Services.Deal.Page;
using static Voupon.Rewards.WebApp.Services.Deal.Page.DetailPage;
using Voupon.Rewards.WebApp.Common.Services.Districts.Queries;
using Voupon.Rewards.WebApp.Common.Services.Provinces.Queries;
using Voupon.Rewards.WebApp.Services.Home.Models;
using Voupon.Rewards.WebApp.Common.Services.Provinces.Models;
using Voupon.Rewards.WebApp.Services.Profile.Page;
using Voupon.Rewards.WebApp.Services.Identity.Commands;
using Voupon.Rewards.WebApp.Common.Redis.Queries;
using SendGrid;
using SendGrid.Helpers.Mail;
using Voupon.Rewards.WebApp.Services.Home.Queries;
using Voupon.Rewards.WebApp.Services.Cart.Models;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Services.Home.Commands;
using Voupon.Rewards.WebApp.Services.Profile.Queries;
using Voupon.Rewards.WebApp.Services.Aggregators.Queries;
using Voupon.Rewards.WebApp.Services.Ads.Queries;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Voupon.Rewards.WebApp.Services.Ads.Commands;
using RewardsAdsProduct = Voupon.Rewards.WebApp.Services.Ads.Queries.RewardsAdsProduct;
using Voupon.Rewards.WebApp.Common.ShippingCost.Models;
using Voupon.Rewards.WebApp.Common.Services.Chats.Queries;
using Microsoft.AspNetCore.Identity;
using static Voupon.Rewards.WebApp.Common.Services.Chats.Queries.GetChatMessagesByGroupIdHandler;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.Services.Chats.Commands;
using Voupon.Rewards.WebApp.Services.Chats.Queries;
using System.Globalization;
using Voupon.Rewards.WebApp.Infrastructures.Extensions;

namespace Voupon.Rewards.WebApp.Controllers
{
    public class HomeController : BaseController
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        //[Route("Chat")]
        //public IActionResult chat()
        //{
        //    return View();
        //}
        [HttpGet]
        [Route("chat/GetChatUsers")]
        public async Task<ApiResponseViewModel> GetChatUsers()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetChatUsers() { UserId = User.Identity.GetUsername() });

            return apiResponseViewModel;
        }

        [HttpPost]
        [Route("chat/DeleteChat")]
        public async Task<ApiResponseViewModel> DeleteChat(string chatGroupId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new DeleteChatCommand() { ChatGroupId = chatGroupId });

            return apiResponseViewModel;
        }
        [HttpGet]
        [Route("chat/GetChatMessagesByGroupId")]
        public async Task<ApiResponseViewModel> GetChatMessagesByGroupId(string chatGroupId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetChatMessagesByGroupId() { GroupChatId = new Guid(chatGroupId) });
            var messagesList = (List<ChatMessagesViewModel>)apiResponseViewModel.Data;
            var sasTokenResult = await Mediator.Send(new SASTokenQuery());
            foreach (var message in messagesList)
            {
                if (message.IsFileAttached && sasTokenResult.Successful)
                {
                    for (int i = 0; i < message.FilesList.Count; i++)
                    {

                        if (message.FilesList[i] != "" && message.FilesList[i] != null)
                        {
                            message.FilesList[i] = message.FilesList[i] + (string)sasTokenResult.Data;
                        }

                    }
                }
            }
            return apiResponseViewModel;
        }


        [HttpGet]
        [Route("GetUnreadedMessagesCountFromUsers")]
        public async Task<ApiResponseViewModel> GetUnreadedMessagesCountFromUsers()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetUnreadedMessagesCount() { UserId = User.Identity.GetUsername().ToLower() });

            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("UpdateReadMessagesByGroupIdAndUserId")]
        public async Task<ApiResponseViewModel> UpdateReadMessagesByGroupIdAndUserId(string chatGroupId, string userId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new UpdateMessageReadStatusByGroupIdCommand() { GroupChatId = new Guid(chatGroupId), UserId = userId });
            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("error/access-denied")]
        public IActionResult AccessDenied()
        {
            return View(ErrorPageEnum.ACCESS_DENIED);
        }

        [HttpGet]
        [Route("product-test-registration-old/{id}/{email}")]
        public async Task<IActionResult> ProductTestRegister(int? id, string email)
        {
            if (!id.HasValue || email == null || email == "")
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            var request = new EditPage
            {
                Email = email
            };
            var result = await Mediator.Send(request);

            if (result.AddressLine1 == null || result.City == null
                || result.CountryId == null || result.DateOfBirthDay == 0 || result.DateOfBirthMonth == 0 || result.DateOfBirthYear == 0
                || result.DemographicEducationId == 0 || result.DemographicEthnicityId == 0 || result.DemographicGenderId == 0 || result.DemographicMaritalStatusId == 0
                || result.DemographicMonthlyHouseHoldIncomeId == 0 || result.DemographicMonthlyIncomeId == 0 || result.DemographicStateId == 0
                || result.FirstName == "" || result.LastName == "" || result.FirstName == null || result.LastName == null || result.MobileCountryCode == null || result.MobileNumber == null
                || result.Postcode == null)
            {
                return RedirectToAction("Edit", "Profile", new { from = "ProductTestRegister", productId = id.Value, email = email });
            }

            HUTSurveyProjects surveyProjectDetails = new HUTSurveyProjects();
            ApiResponseViewModel response = await Mediator.Send(new GetHUTSurveyProjectDetailsById() { Id = id.Value });
            if (response.Successful)
            {
                surveyProjectDetails = (HUTSurveyProjects)response.Data;
            }
            else
            {

            }
            ApiResponseViewModel isExistParticipantresponse = await Mediator.Send(new IsParticipantEmail() { Id = id.Value, Email = email });
            ProductTestRegisterModel productTestRegisterModel = new ProductTestRegisterModel();
            if (result.AddressLine2 != "" && result.AddressLine2 != null)
            {
                productTestRegisterModel.MasterMemberProfileAddress = result.AddressLine1 + ", " + result.AddressLine2 + ", " + result.Postcode + ", " + result.City + ", " + result.State;
            }
            else
            {
                productTestRegisterModel.MasterMemberProfileAddress = result.AddressLine1 + ", " + result.Postcode + ", " + result.City + ", " + result.State;
            }

            productTestRegisterModel.MasterMemberProfileEmail = result.Email;
            productTestRegisterModel.MasterMemberProfileId = result.Id;

            productTestRegisterModel.SurveyProjectStatus = surveyProjectDetails.IsActive;
            productTestRegisterModel.Reward = surveyProjectDetails.VPointsReward;
            productTestRegisterModel.SurveyProjectName = surveyProjectDetails.ExternalName;
            productTestRegisterModel.SurveyProjectId = surveyProjectDetails.Id;
            productTestRegisterModel.SurveyProjectStartDate = surveyProjectDetails.StartDate.Value;
            productTestRegisterModel.IsParticipantEmail = (bool)isExistParticipantresponse.Data;

            return View(productTestRegisterModel);
        }

        [HttpPost]
        [Route("RegisterParticipantForTest-old")]
        public async Task<ApiResponseViewModel> RegisterParticipantForTest(int projectId, string address, string email)
        {
            ApiResponseViewModel response = await Mediator.Send(new UpdateParticipantDetalsFromMasterProfile() { Id = GetMasterMemberId(Request.HttpContext), ProjectId = projectId, Address = address, Email = email });

            return response;
        }
        [HttpPost]
        [Route("SendVerificationEmailAsync")]
        public async Task<JsonResult> SendVerificationEmailAsync(string email, string from)
        {
            var apiResponse = new ApiResponseViewModel();
            if (email == null || email == "")
            {
                apiResponse.Message = "Invalid request [001]";
                apiResponse.Successful = false;
                return Json(apiResponse);
            }

            var request = new SendAccountVerificationEmail
            {
                Id = GetMasterMemberId(Request.HttpContext),
                Email = email,
                From = from
            };
            if (request.Id == 0)
            {
                apiResponse.Message = "Invalid request [002]";
                apiResponse.Successful = false;
                return Json(apiResponse);
            }
            var result = await Mediator.Send(request);


            if (result.Successful)
            {
                apiResponse.Successful = true;
            }
            else
            {
                apiResponse.Message = result.Message;
                apiResponse.Successful = false;
            }
            return Json(apiResponse);
        }


        [HttpGet]
        [Route("ActivationFailed")]
        public ActionResult ActivationFailed()
        {
            return View();
        }

        [HttpGet]
        [Route("VerifyEmail")]
        public async Task<IActionResult> VerifyEmailAsync()
        {

            var request = new EditPage
            {
                Email = User.Identity.Name
            };

            var result = await Mediator.Send(request);

            return View(result);
        }

        [HttpGet]
        [Route("ConfirmAccountVerification/{email}/{code}")]
        public async Task<ActionResult> ConfirmAccountVerificationAsync(string email, string code)
        {
            var requestFrom = HttpContext.Request.QueryString.ToString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }

            var request = new AccountActivation
            {
                Code = code,
                Id = GetMasterMemberId(Request.HttpContext),
                Email = email
            };
            var result = await Mediator.Send(request);


            if (result.Successful)
            {
                if (string.IsNullOrEmpty(requestFrom))
                {
                    return Redirect($"{result.Data}/profile/edit");
                }
                return Redirect($"{result.Data}/profile/edit?from=" + Uri.UnescapeDataString(requestFrom));
                //return RedirectToAction("Edit", "Profile", new { from = Uri.UnescapeDataString(requestFrom), email });
            }
            else
            {
                return RedirectToAction("ActivationFailed", "Home", new { from = result.Message });
            }
        }

        [HttpGet]
        [Route("product-test/{id}")]
        public IActionResult ProductTest(int? id)
        {
            return View();
        }

        [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            Response.StatusCode = 404;
            return View(ErrorPageEnum.ERROR_404);
        }
        [Route("About")]
        public IActionResult About()
        {
            return View();
        }
        [Route("DataPolicy")]
        public IActionResult DataPolicy()
        {
            return View();
        }

        [Route("Privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("TermsAndConditions")]
        public IActionResult TermsAndConditions()
        {
            return View();
        }

        [Route("return-policy")]
        public IActionResult ReturnPolicy()
        {
            return View();
        }

        [Route("faq")]
        public IActionResult FAQ()
        {
            return View();
        }

        [Route("tnc")]
        public IActionResult TNC()
        {
            return View();
        }

        [Route("Contact")]
        public IActionResult Contact()
        {
            return View();
        }


        [HttpPost]
        [Route("SubmitNewRequest")]
        public async Task<ApiResponseViewModel> SubmitNewRequest(string email, string name, string subject, string message, string recaptcha)
        {
            if (string.IsNullOrEmpty(recaptcha))
            {
                return new ApiResponseViewModel
                {
                    Successful = false,
                    Message = "Invalid request"
                };
            }
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var apiKey = "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";
            string templateId = "e6efdac9-4712-4ca7-80be-cb213f913c55";
            string subjectTitle = "Vodus Rewards - New Request";
            var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus Voupon");
            var to = new SendGrid.Helpers.Mail.EmailAddress("support@vodus.my");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.From = from;
            msg.TemplateId = templateId;
            msg.Personalizations = new System.Collections.Generic.List<Personalization>();
            var personalization = new Personalization();
            personalization.Substitutions = new Dictionary<string, string>();
            personalization.Substitutions.Add("-email-", email);
            personalization.Substitutions.Add("-name-", name);
            personalization.Substitutions.Add("-subject-", subject);
            personalization.Substitutions.Add("-message-", message);
            personalization.Subject = subjectTitle;
            personalization.Tos = new List<EmailAddress>();
            personalization.Tos.Add(to);
            msg.Personalizations.Add(personalization);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully sent email";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed to send email";
            }
            return apiResponseViewModel;
        }


        [HttpPost]
        [Route("SubmitNewRequestNoRec")]
        public async Task<ApiResponseViewModel> SubmitNewRequestNoRec(string email, string name, string subject, string message)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            var apiKey = "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";
            string templateId = "e6efdac9-4712-4ca7-80be-cb213f913c55";
            string subjectTitle = "Vodus Rewards - New Request";
            var from = new SendGrid.Helpers.Mail.EmailAddress("noreply@vodus.my", "Vodus Voupon");
            var to = new SendGrid.Helpers.Mail.EmailAddress("support@vodus.my");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage();
            msg.From = from;
            msg.TemplateId = templateId;
            msg.Personalizations = new System.Collections.Generic.List<Personalization>();
            var personalization = new Personalization();
            personalization.Substitutions = new Dictionary<string, string>();
            personalization.Substitutions.Add("-email-", email);
            personalization.Substitutions.Add("-name-", name);
            personalization.Substitutions.Add("-subject-", subject);
            personalization.Substitutions.Add("-message-", message);
            personalization.Subject = subjectTitle;
            personalization.Tos = new List<EmailAddress>();
            personalization.Tos.Add(to);
            msg.Personalizations.Add(personalization);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully sent email";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed to send email";
            }
            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("{CategoryName}")]
        public async Task<IActionResult> Category(string location, string categoryName)
        {
            if (!String.IsNullOrEmpty(categoryName))
                ViewBag.Category = categoryName;
            else
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);

            UserPrefrencesModel userPrefrencesModel = new UserPrefrencesModel();
            ApiResponseViewModel responseProvinceList = await Mediator.Send(new ProvinceListQuery() { CountryId = 1 });
            List<ProvinceModel> provinceList = new List<ProvinceModel>();
            provinceList = (List<ProvinceModel>)responseProvinceList.Data;
            if (location != null)
            {
                ProvinceModel province = provinceList.Where(x => x.Name.Replace(" ", "-").IndexOf(location) != -1).FirstOrDefault();
                if (province == null)
                {
                    return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
                }
                userPrefrencesModel.ProvinceId = province.Id;
                userPrefrencesModel.ProvinceName = province.Name;
            }
            else
            {
                userPrefrencesModel.ProvinceId = 14;
                userPrefrencesModel.ProvinceName = "Kuala Lumpur";
            }

            ApiResponseViewModel responseProductCategoryList = await Mediator.Send(new ProductCategoryListQuery());
            if (responseProductCategoryList.Successful)
            {
                if (categoryName != null)
                {
                    categoryName = categoryName.ToUpper();
                    var productList = (List<ProductCategoryModel>)responseProductCategoryList.Data;
                    var category = productList.Where(x => x.IsActivated == true && x.Name.Replace(" ", "-").Replace(",", "").ToUpper() == categoryName.Replace(" ", "-")).FirstOrDefault();
                    if (category == null)
                    {

                        return View(ErrorPageEnum.INVALID_REQUEST_PAGE);

                        //return RedirectToAction("Index");
                    }
                    userPrefrencesModel.CategoryId = category.Id;
                    userPrefrencesModel.CategoryName = category.Name;
                }
                else
                {
                    return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
                }
            }

            return View(userPrefrencesModel);
        }

        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Search(string keyword)
        {
            if (!String.IsNullOrEmpty(keyword))
                ViewBag.Category = keyword;
            else
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            UserPrefrencesModel userPrefrencesModel = new UserPrefrencesModel();

            userPrefrencesModel.IsSearch = 1;
            userPrefrencesModel.ProvinceId = 0;
            userPrefrencesModel.ProvinceName = "";
            userPrefrencesModel.CategoryId = 0;
            userPrefrencesModel.CategoryName = "";
            userPrefrencesModel.SearchText = keyword;

            return View(userPrefrencesModel);
        }


        [HttpGet]
        public async Task<IActionResult> Deal(int dealId)
        {
            ProductModel product = new ProductModel();
            ApiResponseViewModel response = await Mediator.Send(new ProductQuery() { ProductId = dealId });
            if (response.Successful)
            {
                product = (ProductModel)response.Data;

                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                Voupon.Rewards.WebApp.Common.Blob.Queries.BlobFilesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobFilesListQuery()
                {
                    Id = product.Id,
                    ContainerName = ContainerNameEnum.Products,
                    FilePath = FilePathEnum.Products_Images
                };
                ApiResponseViewModel imageResponse = await Mediator.Send(command);
                if (imageResponse.Successful)
                {
                    product.ImageFolderUrl = (List<string>)imageResponse.Data;
                }
            }
            return View(product);
        }

        [HttpGet]
        [Route("GetProductList")]
        public async Task<ApiResponseViewModel> GetProductList(int provinceId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductListQuery());
            if (response.Successful)
            {
                var productList = (List<ProductModel>)response.Data;
                var newList = productList.Where(x => x.IsPublished == true && x.IsActivated == true && ((x.OutletProvince.IndexOf(provinceId) != -1 && (x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5)).OrderByDescending(x => ((int)x.CTR * 1000) - x.WeightedCTR).ToList();
                foreach (var item in newList)
                {
                    ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                    Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                    {
                        Id = item.Id,
                        ContainerName = ContainerNameEnum.Products,
                        FilePath = FilePathEnum.Products_Images
                    };
                    ApiResponseViewModel imageResponse = await Mediator.Send(command);
                    if (imageResponse.Successful)
                    {
                        item.ImageFolderUrl = (List<string>)imageResponse.Data;
                    }
                }
                response.Data = newList;
            }
            return response;
        }

        [HttpGet]
        [Route("GetProductListByProvince")]
        public async Task<ApiResponseViewModel> GetProductListByProvince(int provinceId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductListByProvinceQuery
            {
                ProvinceId = provinceId
            });

            return response;
        }

        [HttpGet]
        [Route("GetProductListFromRewardsAdsAI")]
        public async Task<ApiResponseViewModel> GetProductListFromRewardsAdsAI(int provinceId)
        {

            ApiResponseViewModel response = await Mediator.Send(new ProductListQuery());
            if (response.Successful)
            {
                ApiResponseViewModel rewardsAdsResponse = await Mediator.Send(new GetRewardsAdsAIProductQuery() { ProvinceId = provinceId });

                var productList = (List<ProductModel>)response.Data;
                var newList = productList.Where(x => x.IsPublished == true && x.IsActivated == true && (((x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5)).OrderBy(x => x.Title).ToList();
                List<ProductModel> matchList = new List<ProductModel>();
                if (rewardsAdsResponse.Successful)
                {
                    var adsPductList = (List<ProductAds>)rewardsAdsResponse.Data;
                    foreach (var product in adsPductList)
                    {
                        int Id = product.ProductId;
                        var matchItem = newList.FirstOrDefault(x => x.Id == Id);
                        if (matchItem != null)
                        {
                            matchList.Add(matchItem);
                        }
                    }
                    if (matchList.Count > 12)
                        matchList = matchList.Take(12).ToList();
                    foreach (var item in matchList)
                    {
                        ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                        Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                        {
                            Id = item.Id,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Images
                        };
                        ApiResponseViewModel imageResponse = await Mediator.Send(command);
                        if (imageResponse.Successful)
                        {
                            item.ImageFolderUrl = (List<string>)imageResponse.Data;
                        }
                    }
                    response.Successful = true;
                    response.Data = matchList;

                }
                else
                {
                    rewardsAdsResponse = await Mediator.Send(new GetRewardsAdsProductQuery());
                    if (rewardsAdsResponse.Successful)
                    {
                        var adsPductList = (List<RewardsAdsProduct>)rewardsAdsResponse.Data;
                        foreach (var product in adsPductList)
                        {
                            int Id = Int32.Parse(product.ProductUrl.Split("/").Last());
                            var matchItem = newList.FirstOrDefault(x => x.Id == Id);
                            if (matchItem != null)
                            {
                                matchList.Add(matchItem);
                            }
                        }
                        if (matchList.Count > 12)
                            matchList = matchList.OrderBy(x => Guid.NewGuid()).Take(12).ToList();
                        foreach (var item in matchList)
                        {
                            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                            Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                            {
                                Id = item.Id,
                                ContainerName = ContainerNameEnum.Products,
                                FilePath = FilePathEnum.Products_Images
                            };
                            ApiResponseViewModel imageResponse = await Mediator.Send(command);
                            if (imageResponse.Successful)
                            {
                                item.ImageFolderUrl = (List<string>)imageResponse.Data;
                            }
                        }
                        response.Successful = true;
                        response.Data = matchList;
                    }
                    else
                    {
                        if (newList.Count > 12)
                            newList = newList.Take(12).ToList();
                        foreach (var item in newList)
                        {
                            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                            Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                            {
                                Id = item.Id,
                                ContainerName = ContainerNameEnum.Products,
                                FilePath = FilePathEnum.Products_Images
                            };
                            ApiResponseViewModel imageResponse = await Mediator.Send(command);
                            if (imageResponse.Successful)
                            {
                                item.ImageFolderUrl = (List<string>)imageResponse.Data;
                            }
                        }
                        response.Successful = true;
                        response.Data = newList;
                    }
                }
            }
            return response;
        }

        [HttpGet]
        [Route("GetRewardsAdsProductList")]
        public async Task<ApiResponseViewModel> GetRewardsAdsProductList()
        {
            return await Mediator.Send(new ProductListAdsQuery());
        }

        [HttpGet]
        [Route("GetProductListByMerchant")]
        public async Task<ApiResponseViewModel> GetProductListByMerchant(int merchantId, int? pageNumber)
        {
            if (merchantId == 0)
            {
                var externalShopId = Request.Query["s"];
                var externalTypeId = Request.Query["t"];

                var page = 1;
                if (pageNumber.HasValue)
                {
                    page = pageNumber.Value;
                }

                ApiResponseViewModel response = await Mediator.Send(new ProductListByMerchantQuery() { ExternalTypeId = byte.Parse(externalTypeId), ExternalMerchantId = externalShopId });
                if (response.Successful)
                {
                    var productList = (List<ProductModel>)response.Data;
                    var newList = productList.Where(x => x.IsPublished == true && x.IsActivated == true && (((x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5)).OrderBy(x => x.Title).ToList();
                    foreach (var item in newList)
                    {
                        ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                        Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                        {
                            Id = item.Id,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Images
                        };
                        ApiResponseViewModel imageResponse = await Mediator.Send(command);
                        if (imageResponse.Successful)
                        {
                            item.ImageFolderUrl = (List<string>)imageResponse.Data;
                        }
                    }


                    var resultList = new List<SearchProductViewModel>();
                    if (newList != null && newList.Any())
                    {
                        response.Data = newList.Select(x => new SearchProductViewModel
                        {
                            DealExpirationId = x.DealExpirationId,
                            DealType = x.DealType,
                            DealTypeId = x.DealTypeId,
                            DiscountedPrice = x.DiscountedPrice,
                            DiscountRate = x.DiscountRate,
                            ExpirationTypeId = x.ExpirationTypeId,
                            Id = 0,
                            PointsRequired = x.PointsRequired,
                            Price = x.Price,
                            Rating = x.Rating,
                            Title = x.Title,
                            TotalSold = (x.TotalBought.HasValue ? (int)x.TotalBought : 0),
                            ProductSubCategory = x.ProductSubCategory,
                            ProductSubCategoryId = x.ProductSubCategoryId,
                            ProductCategoryId = x.ProductCategoryId,
                            ProductImage = (x.ImageFolderUrl.Count() > 0 ? x.ImageFolderUrl[0] : ""),
                            OutletLocation = x.OutletLocation,
                            ExternalTypeId = x.ExternalTypeId,
                            ExternalItemId = x.ExternalId,
                            ExternalShopId = x.ExternalMerchantId
                        });
                    }
                }
                return response;
            }
            else
            {
                ApiResponseViewModel response = await Mediator.Send(new ProductListByMerchantQuery() { MerchantId = merchantId });
                if (response.Successful)
                {
                    var productList = (List<ProductModel>)response.Data;
                    var newList = productList.Where(x => x.IsPublished == true && x.IsActivated == true && (((x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5)).OrderBy(x => x.Title).ToList();
                    foreach (var item in newList)
                    {
                        ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                        Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                        {
                            Id = item.Id,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Images
                        };
                        ApiResponseViewModel imageResponse = await Mediator.Send(command);
                        if (imageResponse.Successful)
                        {
                            item.ImageFolderUrl = (List<string>)imageResponse.Data;
                        }
                    }


                    var resultList = new List<SearchProductViewModel>();
                    if (newList != null && newList.Any())
                    {
                        response.Data = newList.Select(x => new SearchProductViewModel
                        {
                            DealExpirationId = x.DealExpirationId,
                            DealType = x.DealType,
                            DealTypeId = x.DealTypeId,
                            DiscountedPrice = x.DiscountedPrice,
                            DiscountRate = x.DiscountRate,
                            ExpirationTypeId = x.ExpirationTypeId,
                            Id = x.Id,
                            PointsRequired = x.PointsRequired,
                            Price = x.Price,
                            Rating = x.Rating,
                            Title = x.Title,
                            TotalSold = (x.TotalBought.HasValue ? (int)x.TotalBought : 0),
                            ProductSubCategory = x.ProductSubCategory,
                            ProductSubCategoryId = x.ProductSubCategoryId,
                            ProductCategoryId = x.ProductCategoryId,
                            ProductImage = (x.ImageFolderUrl.Count() > 0 ? x.ImageFolderUrl[0] : ""),
                            OutletLocation = x.OutletLocation,

                        });
                    }
                }
                return response;
            }
        }

        [HttpGet]
        [Route("GetProductCategoryList")]
        public async Task<ApiResponseViewModel> GetProductCategoryList()
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductCategoryListQuery());
            if (response.Successful)
            {
                var productList = (List<ProductCategoryModel>)response.Data;
                var newList = productList.Where(x => x.IsActivated == true).ToList();
                response.Data = newList;
            }
            return response;
        }

        [HttpGet]
        [Route("GetProductDiscountList/{productId}")]
        public async Task<ApiResponseViewModel> GetProductDiscountList(int productId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductDiscountListQuery() { ProductId = productId });
            if (response.Successful)
            {
                var productList = (List<Voupon.Database.Postgres.RewardsEntities.ProductDiscounts>)response.Data;
                var newList = productList.Where(x => x.IsActivated == true).ToList();
                response.Data = newList;
            }
            return response;
        }


        [HttpGet]
        [Route("GetProductSubcategoryList")]
        public async Task<ApiResponseViewModel> GetProductSubcategoryList(int categoryId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProductSubcategoryListQuery() { CategoryId = categoryId });
            if (response.Successful)
            {
                var productList = (List<ProductSubcategoryModel>)response.Data;
                var newList = productList.Where(x => x.IsActivated == true).ToList();
                response.Data = newList;
            }
            return response;
        }


        [HttpGet]
        [Route("GetProductBySubcategoryList")]
        public async Task<ApiResponseViewModel> GetProductBySubcategoryList(string filter)
        {

            var filter1 = JsonConvert.DeserializeObject<string[]>(filter);
            int[] filterList = new int[filter1.Length];
            for (int i = 0; i < filter1.Length; i++)
            {
                filterList[i] = Int32.Parse(filter1[i]);
            }
            ApiResponseViewModel response = await Mediator.Send(new ProductListQuery());
            if (response.Successful)
            {
                var productList = (List<ProductModel>)response.Data;
                var newList = productList.Where(x => x.IsPublished == true && x.IsActivated == true && filterList.Contains(x.ProductSubCategoryId.Value)).OrderBy(x => x.Title).ToList();
                foreach (var item in newList)
                {
                    ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                    Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                    {
                        Id = item.Id,
                        ContainerName = ContainerNameEnum.Products,
                        FilePath = FilePathEnum.Products_Images
                    };
                    ApiResponseViewModel imageResponse = await Mediator.Send(command);
                    if (imageResponse.Successful)
                    {
                        item.ImageFolderUrl = (List<string>)imageResponse.Data;
                    }
                }
                response.Data = newList;
            }
            return response;
        }

        [HttpGet]
        [Route("GetProductBySubcategoryListAndPrice")]
        public async Task<ApiResponseViewModel> GetProductBySubcategoryListAndPrice(string subCatergoryFilter, int priceMin, int priceMax, bool IsPriceFilter, int productTypeId, string locationFilter, string searchText, bool isCategory,int? page)
        {
            if (string.IsNullOrEmpty(searchText) && productTypeId == 0)
            {
                return new ApiResponseViewModel
                {
                    Successful = false,
                    Message = "Invalid search query"
                };
            }
            //ProductType women = ProductType.WomenFashion;
            if (searchText != "")
            {
                ProductType serachType = ProductTypExtension.GetEnumValueByDescription(searchText.ToUpper());
                productTypeId = (int)serachType;
                if (productTypeId != 0)
                {
                    searchText = "";
                    isCategory = true;
                }
            }


            int _priceMin = 0;
            int _priceMax = int.MaxValue;

            var pageNumber = 1;
            if (page.HasValue)
            {
                pageNumber = (int)page;
            }

            if (priceMin > 0)
            {
                _priceMin = priceMin;
            }


            if (priceMax > 0)
            {
                _priceMax = priceMax;
            }
            var filter1 = JsonConvert.DeserializeObject<string[]>(subCatergoryFilter);
            int[] filterList = new int[filter1.Length];
            for (int i = 0; i < filter1.Length; i++)
            {
                filterList[i] = Int32.Parse(filter1[i]);
            }

            var filter3 = JsonConvert.DeserializeObject<string[]>(locationFilter);

            int[] filterLocationList = new int[filter3.Length];
            var aggregatorLocationFilter = new List<int>();

            for (int i = 0; i < filter3.Length; i++)
            {
                filterLocationList[i] = Int32.Parse(filter3[i]);
                aggregatorLocationFilter.Add(filterLocationList[i]);
            }

            ApiResponseViewModel response = new ApiResponseViewModel
            {
                Data = null
            };

            List<ProductModel> newList = new List<ProductModel>();
            List<ProductModel> filteredList = new List<ProductModel>();
            if (!filterLocationList.Contains(19))
            {
                var timeStart = DateTime.Now;

                response = await Mediator.Send(new ProductListSearchQuery
                {
                    SearchText = searchText,
                    PageNumber = pageNumber,
                    SubCategory = filterList,
                    IsCategory = isCategory,
                    ProductTypeId = productTypeId,
                    IsPriceFilter = IsPriceFilter,
                    MinPrice = _priceMin,
                    MaxPrice = _priceMax,
                    Location = filterLocationList
                }); 
                if (!response.Successful)
                {
                    response.Data = null;
                    return response;
                }

                var timeTaken = (DateTime.Now - timeStart).TotalSeconds;

                if (response.Data != null)
                {
                    var productList = (List<ProductModel>)response.Data;
                    filteredList.AddRange(productList);

                    var star = DateTime.Now;
                    foreach (var item in filteredList)
                    {
                        ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                        Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                        {
                            Id = item.Id,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Images
                        };
                        ApiResponseViewModel imageResponse = await Mediator.Send(command);
                        if (imageResponse.Successful)
                        {
                            item.ImageFolderUrl = (List<string>)imageResponse.Data;
                        }
                    }
                    var total = (DateTime.Now - star).TotalSeconds;
                }
            }


            var resultList = new List<SearchProductViewModel>();
            if (filteredList != null && filteredList.Any())
            {
                resultList.AddRange(filteredList.Select(x => new SearchProductViewModel
                {
                    DealExpirationId = x.DealExpirationId,
                    DealType = x.DealType,
                    DealTypeId = x.DealTypeId,
                    DiscountedPrice = x.DiscountedPrice,
                    DiscountRate = x.DiscountRate,
                    ExpirationTypeId = x.ExpirationTypeId,
                    Id = (string.IsNullOrEmpty(x.ExternalId) ? x.Id: 0),
                    PointsRequired = x.PointsRequired,
                    Price = x.Price,
                    Rating = x.Rating,
                    Title = x.Title,
                    TotalSold = (x.TotalBought.HasValue ? (int)x.TotalBought : 0),
                    ProductSubCategory = x.ProductSubCategory,
                    ProductSubCategoryId = x.ProductSubCategoryId,
                    ProductCategoryId = x.ProductCategoryId,
                    ProductImage = (x.ImageFolderUrl != null && x.ImageFolderUrl.Any() ? x.ImageFolderUrl[0] : ""),
                    OutletLocation = x.OutletLocation,
                    ExternalItemId = x.ExternalId,
                    ExternalShopId = x.ExternalMerchantId,
                    ExternalTypeId = x.ExternalTypeId,
                }));
            }

            // External
            /*response = await Mediator.Send(new ProductListSearchQuery
            {
                SearchText = searchText,
                PageNumber = pageNumber,
                SubCategory = filterList,
                IsCategory = isCategory,
                ProductTypeId = productTypeId,
                IsPriceFilter = IsPriceFilter,
                MinPrice = _priceMin,
                MaxPrice = _priceMax,
                Location = filterLocationList,
                IsExternal = true
            });

            filteredList = new List<ProductModel>();

            if (response.Data != null)
            {
                var productList = (List<ProductModel>)response.Data;
                filteredList.AddRange(productList);
                foreach (var item in filteredList)
                {
                    ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                    Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                    {
                        Id = item.Id,
                        ContainerName = ContainerNameEnum.Products,
                        FilePath = FilePathEnum.Products_Images
                    };
                    ApiResponseViewModel imageResponse = await Mediator.Send(command);
                    if (imageResponse.Successful)
                    {
                        item.ImageFolderUrl = (List<string>)imageResponse.Data;
                    }
                }
            }

            resultList.AddRange(filteredList.Select(x => new SearchProductViewModel
            {
                DealExpirationId = x.DealExpirationId,
                DealType = x.DealType,
                DealTypeId = x.DealTypeId,
                DiscountedPrice = x.DiscountedPrice,
                DiscountRate = x.DiscountRate,
                ExpirationTypeId = x.ExpirationTypeId,
                Id = 0,
                PointsRequired = x.PointsRequired,
                Price = x.Price,
                Rating = x.Rating,
                Title = x.Title,
                TotalSold = (x.TotalBought.HasValue ? (int)x.TotalBought : 0),
                ProductSubCategory = x.ProductSubCategory,
                ProductSubCategoryId = x.ProductSubCategoryId,
                ProductCategoryId = x.ProductCategoryId,
                ProductImage = (x.ImageFolderUrl != null && x.ImageFolderUrl.Any() ? x.ImageFolderUrl[0] : ""),
                OutletLocation = x.OutletLocation,
                ExternalItemId = x.ExternalId,
                ExternalShopId = x.ExternalMerchantId,
                ExternalTypeId = x.ExternalTypeId

            }));
            */
            if (resultList != null && resultList.Any())
            {
                response.Data = resultList;
            }
            else
            {
                response.Data = null;
            }
            response.Successful = true;
            return response;
        }

        private List<ProductModel> FilrterLocation(List<ProductModel> filteredList, List<int> filterLocationList)
        {
            List<ProductModel> filteredLocationList = new List<ProductModel>();
            List<int> filterLocations = new List<int>();
            foreach (var filter in filterLocationList)
            {
                if (filter == 17)
                {
                    if (!filterLocationList.Contains(15))
                        filterLocations.Add(15);
                    if (!filterLocationList.Contains(11))
                        filterLocations.Add(11);
                    if (!filterLocationList.Contains(10))
                        filterLocations.Add(10);
                }
                if (filter == 18)
                {
                    if (!filterLocationList.Contains(1))
                        filterLocations.Add(1);
                    if (!filterLocationList.Contains(2))
                        filterLocations.Add(2);
                    if (!filterLocationList.Contains(3))
                        filterLocations.Add(3);
                    if (!filterLocationList.Contains(4))
                        filterLocations.Add(4);
                    if (!filterLocationList.Contains(5))
                        filterLocations.Add(5);
                    if (!filterLocationList.Contains(6))
                        filterLocations.Add(6);
                    if (!filterLocationList.Contains(7))
                        filterLocations.Add(7);
                    if (!filterLocationList.Contains(8))
                        filterLocations.Add(8);
                    if (!filterLocationList.Contains(9))
                        filterLocations.Add(9);
                    if (!filterLocationList.Contains(12))
                        filterLocations.Add(12);
                    if (!filterLocationList.Contains(13))
                        filterLocations.Add(13);
                    if (!filterLocationList.Contains(14))
                        filterLocations.Add(14);
                    if (!filterLocationList.Contains(16))
                        filterLocations.Add(16);
                }
                else
                {
                    if (!filterLocations.Contains(filter))
                        filterLocations.Add(filter);
                }
            }
            List<int> filteredIds = new List<int>();
            foreach (var locationId in filterLocations)
            {
                filteredLocationList.AddRange(filteredList.Where(x => x.OutletProvince.IndexOf(locationId) != -1 && filteredIds.IndexOf(x.Id) == -1).ToList());
                filteredIds.AddRange(filteredLocationList.Select(x => x.Id));
            }

            return filteredLocationList;
        }

        private List<int> FilrterLocationForAggregator(List<int> filterLocationList)
        {
            if (filterLocationList.Contains(20))
            {
                if (!filterLocationList.Contains(17))
                    filterLocationList.Add(17);
                if (!filterLocationList.Contains(18))
                    filterLocationList.Add(18);
            }

            return filterLocationList;
        }

        [HttpGet]
        [Route("GenerateGoogleMerchant")]
        public async Task<ApiResponseViewModel> GoogleMerchantGenerator()
        {
            var page = 1;

            var apiResponseViewModel = new ApiResponseViewModel();
            var googleMerchantResult = await Mediator.Send(new GoogleMerchantKeywordQuery());
            if (googleMerchantResult == null || !googleMerchantResult.Any())
            {
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }

            var selectedProductForGoogleMerchant = new List<SearchProductViewModel>();
            foreach (var googleMerchant in googleMerchantResult)
            {
                List<ProductModel> filteredList = new List<ProductModel>();
                var aggregatorPriceFilter = new List<int>();

                var timeStart = DateTime.Now;
                var response = await Mediator.Send(new ProductListSearchQuery
                {
                    SearchText = googleMerchant.Keyword
                });
                if (response.Data != null)
                {
                    var productList = (List<ProductModel>)response.Data;

                    filteredList = productList.Where(x => x.IsPublished == true && x.IsActivated == true && ((x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5).OrderBy(x => x.Title).ToList();

                    var star = DateTime.Now;
                    foreach (var item in filteredList)
                    {
                        Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery command = new Voupon.Rewards.WebApp.Common.Blob.Queries.BlobSmallImagesListQuery()
                        {
                            Id = item.Id,
                            ContainerName = ContainerNameEnum.Products,
                            FilePath = FilePathEnum.Products_Images
                        };
                        ApiResponseViewModel imageResponse = await Mediator.Send(command);
                        if (imageResponse.Successful)
                        {
                            item.ImageFolderUrl = (List<string>)imageResponse.Data;
                        }
                    }
                }

                var resultList = new List<SearchProductViewModel>();
                if (filteredList != null && filteredList.Any())
                {
                    resultList.AddRange(filteredList.Select(x => new SearchProductViewModel
                    {
                        DealExpirationId = x.DealExpirationId,
                        DealType = x.DealType,
                        DealTypeId = x.DealTypeId,
                        DiscountedPrice = x.DiscountedPrice,
                        DiscountRate = x.DiscountRate,
                        ExpirationTypeId = x.ExpirationTypeId,
                        Id = x.Id,
                        PointsRequired = x.PointsRequired,
                        Price = x.Price,
                        Rating = x.Rating,
                        Title = x.Title,
                        TotalSold = (x.TotalBought.HasValue ? (int)x.TotalBought : 0),
                        ProductSubCategory = x.ProductSubCategory,
                        ProductSubCategoryId = x.ProductSubCategoryId,
                        ProductCategoryId = x.ProductCategoryId,
                        ProductImage = (x.ImageFolderUrl != null && x.ImageFolderUrl.Any() ? x.ImageFolderUrl[0] : ""),
                        OutletLocation = x.OutletLocation,
                        Brand = x.MerchantName,
                        Description = x.Description,
                        Language = googleMerchant.Language

                    }));
                }

                if (!string.IsNullOrEmpty(googleMerchant.Keyword))
                {
                    var aggregator = await Mediator.Send(new AggregatorSearchByKeywordQuery
                    {
                        SearchQuery = googleMerchant.Keyword,
                        //PriceFilter = aggregatorPriceFilter,
                        PageNumber = 1

                    });

                    if (aggregator.Successful)
                    {
                        var aggregatorResultList = new List<SearchProductViewModel>();
                        var aggregatorData = JsonConvert.DeserializeObject<List<SearchProductViewModel>>(aggregator.Data.ToString());
                        if (aggregatorData != null && aggregatorData.Any())
                        {
                            foreach (var aggreagatorItem in aggregatorData)
                            {
                                aggreagatorItem.Language = googleMerchant.Language;
                                aggregatorResultList.Add(aggreagatorItem);
                            }
                        }

                        if (aggregatorResultList != null && aggregatorResultList.Any())
                        {
                            aggregatorResultList.OrderBy(x => x.Price);
                            resultList.AddRange(aggregatorResultList);

                            // Aggregator Filter
                            //var aggregatorFilter = await Mediator.Send(new AggregatorFilterAlgoQuery
                            //{
                            //    SearchQuery = searchText,
                            //    PriceFilter = aggregatorPriceFilter,
                            //    SearchProductModel = resultList

                            //});
                            //if (aggregatorFilter.Successful)
                            //{
                            //    var aggregatorFilterData = JsonConvert.DeserializeObject<List<SearchProductViewModel>>(aggregatorFilter.Data.ToString());
                            //    if (aggregatorFilterData != null && aggregatorFilterData.Any())
                            //    {
                            //        resultList = aggregatorFilterData;
                            //    }
                            //}
                        }
                    }
                }

                if (googleMerchant.SortBy == "sold")
                {
                    var itemsToAdd = resultList.OrderByDescending(x => x.TotalSold).Take(googleMerchant.TotalListing).ToList();
                    selectedProductForGoogleMerchant.AddRange(itemsToAdd);
                }
                else
                {

                }

            }

            if (selectedProductForGoogleMerchant != null && selectedProductForGoogleMerchant.Any())
            {

                //  Get shipping details
                var vodusProducts = selectedProductForGoogleMerchant.Where(x => x.Id > 0).Select(x => x.Id);
                var shippingData = await Mediator.Send(new ShippingCostQuery
                {
                    ProductIds = vodusProducts.ToArray(),
                    ProvinceId = 14
                });

                if (shippingData.Data != null)
                {
                    var shippingResult = (OrderShippingCostsModel)shippingData.Data;

                    foreach (var item in shippingResult.OrderShippingCosts)
                    {
                        var itemToUpdate = selectedProductForGoogleMerchant.Where(x => x.Id == item.ProductId).First();
                        itemToUpdate.ShippingCost = item.OrderShippingCost;
                    }
                }

                foreach (var item in selectedProductForGoogleMerchant.Where(x => x.Id == 0))
                {
                    if (item.Id == 0)
                    {
                        var externalShippingResult = await Mediator.Send(new AggregatorShippingCostQuery()
                        {
                            ExternalItemId = item.ExternalItemId,
                            ExternalShopId = item.ExternalShopId,
                            ExternalTypeId = item.ExternalTypeId,
                            City = "",
                            Town = "Kuala Lumpur"
                        });

                        if (externalShippingResult.Successful)
                        {
                            var aggregatorData = JsonConvert.DeserializeObject<OrderShippingCostForPoductIdAndVariationIdModel>(externalShippingResult.Data.ToString());
                            item.ShippingCost += aggregatorData.OrderShippingCost;
                        }
                    }
                }

                var enList = selectedProductForGoogleMerchant.Where(x => x.Language.ToLower() == "en").ToList();
                var msList = selectedProductForGoogleMerchant.Where(x => x.Language.ToLower() == "ms").ToList();

                if (enList != null && enList.Any())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (XmlWriter writer = XmlWriter.Create(ms))
                        {
                            writer.WriteStartElement("rss", "http://base.google.com/ns/1.0");
                            writer.WriteAttributeString("version", "2.0");
                            writer.WriteStartElement("channel");
                            writer.WriteElementString("title", "Vodus Google Merchant Products");
                            writer.WriteElementString("link", "https://vodus.my");
                            writer.WriteElementString("description", "Vodus Google Merchant Products");
                            foreach (var item in enList)
                            {
                                writer.WriteStartElement("item");
                                writer.WriteElementString("id", (item.Id > 0 ? item.Id.ToString() : item.ExternalItemId + "-" + item.ExternalShopId + "-" + item.ExternalTypeId));
                                decimal price = (decimal)item.DiscountedPrice.Value;
                                if (item.Id == 0)
                                {
                                    if (price < 50)
                                    {
                                        price -= ((decimal)(price * (decimal)0.12));
                                    }
                                    else if (price < 100)
                                    {
                                        price -= ((decimal)(price * (decimal)0.08));
                                    }
                                    else if (price < 200)
                                    {
                                        price -= ((decimal)(price * (decimal)0.06));
                                    }
                                    else if (price < 300)
                                    {
                                        price -= ((decimal)(price * (decimal)0.05));
                                    }
                                    else if (price < 400)
                                    {
                                        price -= ((decimal)(price * (decimal)0.04));
                                    }
                                }

                                writer.WriteElementString("title", item.Title.Length > 150 ? item.Title.Substring(0, 149) : item.Title);
                                writer.WriteElementString("description", item.Description.Length > 5000 ? item.Description.Substring(0, 4999) : item.Title);
                                writer.WriteElementString("link", $"https://vodus.my/product/{item.Id}" + (!string.IsNullOrEmpty(item.ExternalItemId) ? $"?i={item.ExternalItemId}&s={item.ExternalShopId}&t={item.ExternalTypeId}" : ""));
                                writer.WriteElementString("image_link", item.ProductImage);
                                writer.WriteElementString("availability", "in_stock");
                                writer.WriteElementString("price", price.ToString("F") + " MYR");
                                writer.WriteElementString("brand", item.Brand);
                                writer.WriteStartElement("shipping");
                                writer.WriteElementString("price", item.ShippingCost.ToString("F") + " MYR");
                                writer.WriteElementString("country", "MY");
                                writer.WriteEndElement();
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();

                            writer.Flush();
                        }
                        ms.Position = 0;
                        var result = await Mediator.Send(new CreateGoogleMerchantXmlCommand
                        {
                            ms = ms,
                            Filename = "google-merchant-products-en.xml"
                        });
                    }
                }

                if (msList != null && msList.Any())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (XmlWriter writer = XmlWriter.Create(ms))
                        {
                            writer.WriteStartElement("rss", "http://base.google.com/ns/1.0");
                            writer.WriteAttributeString("version", "2.0");
                            writer.WriteStartElement("channel");
                            writer.WriteElementString("title", "Vodus Google Merchant Products");
                            writer.WriteElementString("link", "https://vodus.my");
                            writer.WriteElementString("description", "Vodus Google Merchant Products");
                            foreach (var item in msList)
                            {
                                writer.WriteStartElement("item");
                                writer.WriteElementString("id", (item.Id > 0 ? item.Id.ToString() : item.ExternalItemId + "-" + item.ExternalShopId + "-" + item.ExternalTypeId));

                                decimal price = (decimal)item.DiscountedPrice.Value;
                                if (item.Id == 0)
                                {
                                    if (price < 50)
                                    {
                                        price -= ((decimal)(price * (decimal)0.12));
                                    }
                                    else if (price < 100)
                                    {
                                        price -= ((decimal)(price * (decimal)0.08));
                                    }
                                    else if (price < 200)
                                    {
                                        price -= ((decimal)(price * (decimal)0.06));
                                    }
                                    else if (price < 300)
                                    {
                                        price -= ((decimal)(price * (decimal)0.05));
                                    }
                                    else if (price < 400)
                                    {
                                        price -= ((decimal)(price * (decimal)0.04));
                                    }
                                }

                                writer.WriteElementString("title", item.Title.Length > 150 ? item.Title.Substring(0, 149) : item.Title);
                                writer.WriteElementString("description", item.Description.Length > 5000 ? item.Description.Substring(0, 4999) : item.Title);
                                writer.WriteElementString("link", $"https://vodus.my/product/{item.Id}" + (!string.IsNullOrEmpty(item.ExternalItemId) ? $"?i={item.ExternalItemId}&s={item.ExternalShopId}&t={item.ExternalTypeId}" : ""));
                                writer.WriteElementString("image_link", item.ProductImage);
                                writer.WriteElementString("availability", "in_stock");
                                writer.WriteElementString("price", item.DiscountedPrice.Value.ToString("F") + " MYR");
                                writer.WriteElementString("brand", item.Brand);
                                writer.WriteStartElement("shipping");
                                writer.WriteElementString("price", item.ShippingCost.ToString("F") + " MYR");
                                writer.WriteElementString("country", "MY");
                                writer.WriteEndElement();
                                writer.WriteEndElement();
                            }
                            writer.WriteEndElement();

                            writer.Flush();
                        }
                        ms.Position = 0;
                        var result = await Mediator.Send(new CreateGoogleMerchantXmlCommand
                        {
                            ms = ms,
                            Filename = "google-merchant-products-ms.xml"
                        });
                    }
                }



            }

            apiResponseViewModel.Successful = true;
            apiResponseViewModel.Message = "Done generating google merchant products";
            return apiResponseViewModel;
        }


        [HttpGet]
        [Route("GetProductByType")]
        public async Task<ApiResponseViewModel> GetProductByType(int type, /*int provinceId,*/ int? page)
        {
            //return await Mediator.Send(new ProductListQuery() { Category = type, PageNumber = (int)page });
            //Added by shanuka perera, 2023-02-15
            ApiResponseViewModel response = await Mediator.Send(new ProductListQuery() { Category = type, PageNumber = (int)page });
            if (response.Successful)
            {
                var searchprodList = (List<SearchProductViewModel>)response.Data;
                var shuffList = searchprodList.OrderBy(x => Guid.NewGuid()).ToList();
                response.Data = shuffList;
            }
            return response;

        }


        [HttpGet]
        [Route("GetMerchantList")]
        public async Task<ApiResponseViewModel> GetMerchantList()
        {
            ApiResponseViewModel response = await Mediator.Send(new MerchantListQuery());
            if (response.Successful)
            {
                var merchantList = (List<MerchantModel>)response.Data;
                var newList = merchantList.Where(x => x.IsPublished == true && x.IsBrandShownInHomePage == true).ToList().Take(20);
                response.Data = newList;
            }
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
        [Route("ForgetPassword")]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        public IActionResult ResetPassword(string email, string code)
        {

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Login");
            }
            var model = new ResetPasswordViewModel();
            model.Email = email;
            model.Code = code;
            return View(model);

        }

    }

}
