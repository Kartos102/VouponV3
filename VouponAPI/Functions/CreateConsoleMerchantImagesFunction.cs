using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.API.ViewModels;
using Voupon.API.Util;
using System.IO;

namespace Voupon.API.Functions.Blog
{
    public class CreateConsoleMerchantImagesFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly IAzureBlobStorage _azureBlobStorage;

        public CreateConsoleMerchantImagesFunction(IHttpClientFactory httpClientFactory, RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            _rewardsDBContext = rewardsDBContext;
            _azureBlobStorage = azureBlobStorage;
        }

        [FunctionName("CreateConsoleMerchantImagesFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "console/merchant-images")] HttpRequest req, ILogger log)
        {
            var response = new ApiResponseViewModel
            {
            };

            try
            {
                var request = HttpRequestHelper.DeserializeModel<MerchantImagesRequestModel>(req);

                var existingMerchant = await _rewardsDBContext.Merchants.Where(x => x.ExternalId == request.ExternalMerchantId).FirstOrDefaultAsync();
                if (existingMerchant == null)
                {
                    response.ErrorMessage = "Invalid external merchant id";
                    return new OkObjectResult(response);
                }

                //  Handle images
                var imageUrl = "";

                if (request.Images != null && request.Images.Count() > 0)
                {
                    //  Delete existing images
                    _azureBlobStorage.DeleteContainerFiles(ContainerNameEnum.Merchants, existingMerchant.Id + "/" + FilePathEnum.Products_Images);

                    foreach (var image in request.Images)
                    {

                        using (System.Net.WebClient webClient = new System.Net.WebClient())
                        {
                            byte[] imageByte = webClient.DownloadData($"https://cf.shopee.com.my/file/{image}");
                            Stream stream = new MemoryStream(imageByte);
                            var contentType = "jpeg";
                            stream.Position = 0;
                            var blob = await _azureBlobStorage.UploadBlobAsync(stream, existingMerchant.Id + "/" +
                               FilePathEnum.Products_Images + "/" + image, contentType,
                              ContainerNameEnum.Merchants, true);
                            imageUrl += "<br/>";
                            imageUrl += "<img src='" + blob.StorageUri.PrimaryUri.ToString() + "'/>";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    existingMerchant.Description = existingMerchant.Description + imageUrl;
                    _rewardsDBContext.Merchants.Update(existingMerchant);
                    await _rewardsDBContext.SaveChangesAsync();
                }


                //  Handle carousel images
                var imageList = new List<string>();
                if (request.CarouselImages != null && request.CarouselImages.Count() > 0)
                {
                    foreach (var image in request.CarouselImages)
                    {
                        using (System.Net.WebClient webClient = new System.Net.WebClient())
                        {
                            byte[] imageByte = webClient.DownloadData($"https://cf.shopee.com.my/file/{image}");
                            Stream stream = new MemoryStream(imageByte);
                            var contentType = "jpeg";
                            stream.Position = 0;
                            var blob = await _azureBlobStorage.UploadBlobAsync(stream, existingMerchant.Id + "/" +
                               FilePathEnum.Products_Images + "/" + image, contentType,
                              ContainerNameEnum.Merchants, true);

                            imageList.Add(blob.StorageUri.PrimaryUri.ToString().Replace("http:","https:"));
                        }
                    }
                }

                if (imageList != null && imageList.Count() > 0)
                {
                    var orderNumber = 1;
                    foreach (var image in imageList)
                    {
                        await _rewardsDBContext.MerchantCarousel.AddAsync(new MerchantCarousel
                        {
                            MerchantId = existingMerchant.Id,
                            ImageUrl = image,
                            OrderNumber = orderNumber,
                            StatusId = 1,
                            CreatedAt = DateTime.Now,
                            CreatedByUserId = Guid.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.ADMIN_USER_ID))
                        });
                        orderNumber++;
                    }
                    await _rewardsDBContext.SaveChangesAsync();
                }

                response.Code = 0;

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                response.ErrorMessage = "Fail to create merchant images json" + ex.ToString();
                return new BadRequestObjectResult(response);

            }
        }

        private class MerchantImagesRequestModel
        {
            public string ExternalMerchantId { get; set; }
            public string[] Images { get; set; }
            public string[] CarouselImages { get; set; }

        }


    }
}
