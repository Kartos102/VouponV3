using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.App.Services.Business.ViewModels;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Business.Commands.Update
{
    public class UpdatePersonInChargeCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int MerchantId { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Position { get; set; }
        public string IdentityNumber { get; set; }
        public string Document { get; set; }
        
    }

    public class UpdatePersonInChargeCommandHandler : IRequestHandler<UpdatePersonInChargeCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdatePersonInChargeCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdatePersonInChargeCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            
            var personInCharge = await rewardsDBContext.PersonInCharges.FirstAsync(x => x.Id == request.Id);
            if(personInCharge != null)
            {
                var personIncahrgeModel = new PersonInCharges();
                personIncahrgeModel.Id = request.Id;
                personIncahrgeModel.MerchantId = request.MerchantId;
                personIncahrgeModel.Name = request.Name;
                personIncahrgeModel.Contact = request.Contact;
                personIncahrgeModel.Position = request.Position;
                personIncahrgeModel.IdentityNumber = request.IdentityNumber;
                personIncahrgeModel.LastUpdatedAt = DateTime.Now;
                personIncahrgeModel.PendingChanges = "";
                var pendingChanges = JsonConvert.SerializeObject(personIncahrgeModel);
                personInCharge.PendingChanges = pendingChanges;
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Person In Charge Successfully";
            }
            else
            {
                response.Message = "Person In Charge not found";
            }         
            return response;
        }
   
        private void Image()
        {
            //try
            //{
            //    var fileContent = System.Web.HttpContext.Current.Request.Files["UploadedPresetImage"];
            //    var filename = "Preset_Image/0/" + User.Identity.GetClientProfileId() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + "." + fileContent.ContentType.Split('/')[1];
            //    using (MemoryStream ms = new MemoryStream())
            //    {
            //        fileContent.InputStream.Position = 0;
            //        fileContent.InputStream.CopyTo(ms);
            //        ms.Position = 0;
            //        var storageHelper = new StorageHelper();
            //        var blob = storageHelper.UploadBlob(ms, filename, fileContent.ContentType, StorageHelper.AzureContainer.ClientLibraryImages, true);

            //        fileUrl = blob.StorageUri.PrimaryUri.ToString();
            //        imageBase64Data = "data:" + fileContent.ContentType + ";base64," + Convert.ToBase64String(ms.ToArray(), 0, (int)ms.Length);
            //        var result = new UploadImageResponse
            //        {
            //            FileUrl = fileUrl,
            //            Base64Data = imageBase64Data
            //        };
            //        apiResponse.message = "Successfully uploaded image";
            //        apiResponse.data = result;
            //    }
            //    apiResponse.successful = true;
            //}
            //catch (Exception ex)
            //{
            //    //@todo log
            //    apiResponse.message = "Fail to upload image to library";
            //    apiResponse.successful = false;
            //}
            //return Json(apiResponse);
        }
    
    }
}
