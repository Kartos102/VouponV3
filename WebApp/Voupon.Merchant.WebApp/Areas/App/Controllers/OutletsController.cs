using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.App.Services.Outlets.ViewModels;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Create;
using Voupon.Merchant.WebApp.Common.Services.Blob.Commands.Delete;
using Voupon.Merchant.WebApp.Common.Services.Blob.Queries;
using Voupon.Merchant.WebApp.Common.Services.Countries.Queries;
using Voupon.Merchant.WebApp.Common.Services.Districts.Queries;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Commands;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Queries;
using Voupon.Merchant.WebApp.Common.Services.Postcodes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Provinces.Queries;
using Voupon.Merchant.WebApp.Controllers;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant")]
    public class OutletsController : BaseController
    {
        public IActionResult Index()
        {
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            if (String.IsNullOrEmpty(MerchantId))
            {
                return View(Infrastructure.Enums.ErrorPageEnum.NOT_ALLOWED_PAGE);
                //return BadRequest();
            }
            var merchantId = Int32.Parse(MerchantId);
            return View(new OutletViewModel() { MerchantId = merchantId });
        }

        #region Post
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
            }
            ;
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

            //  BlobFilesListQuery command1 = new BlobFilesListQuery()
            //  {
            //      Id = outletId,
            //      ContainerName = ContainerNameEnum.Outlets,
            //      FilePath = FilePathEnum.Outlets_Images, GetIListBlobItem=true
            //  }
            //;
            //  ApiResponseViewModel response1 = await Mediator.Send(command1);

            //  foreach (var a in fileContents)
            //  {
            //      var filename = a.FileName;
            //     // if (filename == null)
            //     //     return Content("filename not present");

            //      var path = Path.Combine(
            //                     Directory.GetCurrentDirectory(),
            //                     "wwwroot", filename);

            //      var memory = new MemoryStream();
            //      using (var stream = new FileStream(filename, FileMode.Open))
            //      {
            //          await stream.CopyToAsync(memory);
            //      }
            //      memory.Position = 0;

            //      var b= File(memory, GetContentType(path), Path.GetFileName(path));
            //  }

        }

        //public async Task<IActionResult> Download(string filename)
        //{
        //    if (filename == null)
        //        return Content("filename not present");

        //    var path = Path.Combine(
        //                   Directory.GetCurrentDirectory(),
        //                   "wwwroot", filename);

        //    var memory = new MemoryStream();
        //    using (var stream = new FileStream(path, FileMode.Open))
        //    {
        //        await stream.CopyToAsync(memory);
        //    }
        //    memory.Position = 0;
            
        //    return File(memory, GetContentType(path), Path.GetFileName(path));
        //}

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats officedocument.spreadsheetml.sheet"},  
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
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
        #endregion

        #region Get
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


        [HttpGet]
        [Route("GetMerchantOutlets/{merchantId}")]
        public async Task<ApiResponseViewModel> GetMerchantOutlets(int merchantId)
        {
            if (!VerifyUserAccess(merchantId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }
            ApiResponseViewModel response = new ApiResponseViewModel();
            return await Mediator.Send(new MerchantOutletListQuery() { MerchantId = merchantId });
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
        #endregion

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
        [Route("UpdateOutlet")]
        public async Task<IActionResult> UpdateOutlet(UpdateOutletCommand command)
        {
            if (!await VerifyUserAccessWithOutletId(command.Id))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return BadRequest( badRequestResponse);
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

        private async Task<bool> VerifyUserAccessWithOutletId(int outletId)
        {
           int merchantId=await QueriesOutletMerchantId(outletId);
           return VerifyUserAccess(merchantId);
        }

        private async Task<int> QueriesOutletMerchantId(int outletId)
        {
            int merchantId = 0;
            ApiResponseViewModel response = await Mediator.Send(new OutletQuery() { OutletId = outletId });
            if (response.Successful)
            {
                if(response.Data!=null)
                {
                   var outlet= (Common.Services.Outlets.Models.OutletModel)response.Data;
                    merchantId= outlet.MerchantId;
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
    }
}