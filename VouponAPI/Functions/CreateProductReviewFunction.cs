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
using Microsoft.AspNetCore.Identity;
using Voupon.Common;
using JWT.Algorithms;
using JWT;
using JWT.Serializers;
using Google.Apis.Auth;
using JWT.Builder;
using System.IO;
using System.Text.RegularExpressions;

namespace Voupon.API.Functions.CreateProductReview
{
    public class CreateProductReviewFunction
    {
        private readonly UserManager<Database.Postgres.VodusEntities.Users> userManager;
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;
        private readonly SignInManager<Database.Postgres.VodusEntities.Users> signInManager;
        private readonly IJwtAlgorithm _algorithm;
        private readonly IJsonSerializer _serializer;
        private readonly IBase64UrlEncoder _base64Encoder;
        private readonly IJwtEncoder _jwtEncoder;
        private readonly int _tenMegaBytes = 10 * 1024 * 1024;
        private readonly int _oneMegaByyes = 1 * 1024 * 1024;
        private readonly List<string> _allowedExtention = new List<string>() { ".jpg", ".jpeg", ".png", ".jfif", ".mp4" };
        private int _masterMemberProfileId;
        private string _errorMessage ="";
        private string _containerName = "reviews";
        public CreateProductReviewFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer,
            SignInManager<Database.Postgres.VodusEntities.Users> signInManager, UserManager<Database.Postgres.VodusEntities.Users> userManager, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Reviews", tags: new[] { "Products" }, Description = "Create product review", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(CreateReviewResponseModel), Summary = "Success message")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to create review")]
        [OpenApiRequestBody("multipart/form-data", typeof(CreateReviewRequestModel), Description = "JSON request body ")]

        [FunctionName("CreateProductReviewFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "product/review")] HttpRequest req, ILogger log)
        {
            try
            {
               
                var response = new CreateReviewResponseModel
                {
                    Data = new CreateReviewData()
                };
                var auth = new Authentication(req);
                if (!auth.IsValid)
                {
                    response.RequireLogin = true;
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Invalid access. Please login to first [001]"
                    });
                }
                _masterMemberProfileId = auth.MasterMemberProfileId;
                CreateReviewRequestModel request = GenerateRequestObject(req.Form);
                if (!string.IsNullOrEmpty(_errorMessage)) {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = _errorMessage
                    });

                }


                var item = await rewardsDBContext.ProductReview.FirstOrDefaultAsync(x => x.OrderItemId == request.OrderItemId);
                if (item != null)
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Order Item already reviewed"
                    });
                }
                else
                {
                    


                    var member = await vodusV2Context.MasterMemberProfiles.FirstOrDefaultAsync(x => x.Id == _masterMemberProfileId);
                    var user = await vodusV2Context.Users.FirstOrDefaultAsync(x => x.Id == member.UserId);
                    var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
                    var newReview = new ProductReview();
                    newReview.Id = Guid.NewGuid();
                    newReview.Comment = request.Comment;
                    if (newReview.Comment == null)newReview.Comment = "";
                    newReview.MasterMemberProfileId = _masterMemberProfileId;
                    newReview.MerchantId = request.MerchantId;
                    newReview.OrderItemId = request.OrderItemId;
                    newReview.ProductId = request.ProductId;
                    newReview.Rating = request.Rating;
                    newReview.CreatedAt = DateTime.Now;
                    newReview.ProductTitle = product.Title;
                    newReview.MemberName = (String.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName + " ") + (String.IsNullOrEmpty(user.LastName) ? "" : user.LastName);

                    int videoCount = 0;
                    Regex reg = new Regex("[*'\",_&#^@]");
                    List<ProductReviewUploads> uploads = new List<ProductReviewUploads>();
                    if (req.Form.Files.Count != 0)
                    {
                        foreach (var file in req.Form.Files)
                        {
                            int breaker = file.FileName.LastIndexOf(".");
                            FileProperty property = new FileProperty()
                            {
                                Filename = reg.Replace(file.FileName.Substring(0, breaker).ToLower()," "),
                                Extention = file.FileName.Substring(breaker).ToLower(),
                                Size = file.Length,
                                ContentType = file.ContentType,
                            };

                            // Check if video only can upload 1
                            if (property.Extention == ".mp4") {
                                if (videoCount > 0) {
                                    return new BadRequestObjectResult(new ApiResponseViewModel
                                    {
                                        Code = -1,
                                        ErrorMessage = "Only can upload 1 video !"
                                    }) ;
                                }
                            
                            }

                            string err = ValidateFile(property);

                            if (!String.IsNullOrEmpty(err))
                            {
                                return new BadRequestObjectResult(new ApiResponseViewModel
                                {
                                    Code = -1,
                                    ErrorMessage = err
                                });
                            }

                            string url = await UploadContent(file, property, request.ProductId);
                            if (!String.IsNullOrEmpty(_errorMessage))
                            {

                                return new BadRequestObjectResult(new ApiResponseViewModel
                                {
                                    Code = -1,
                                    ErrorMessage = _errorMessage
                                });

                            }

                            if (property.Extention == ".mp4") {
                                    videoCount += 1;
                            }
                            ProductReviewUploads productUpload = new ProductReviewUploads();
                            productUpload.Id = Guid.NewGuid();
                            productUpload.MimeType = file.ContentType;
                            productUpload.FileUrl = url;
                            uploads.Add(productUpload);
                        }
                    }

                    //newReview.ProductReviewUploads = uploads;

                    rewardsDBContext.ProductReview.Add(newReview);
                    await rewardsDBContext.SaveChangesAsync();


                    var productReviewList = await rewardsDBContext.ProductReview.Where(x => x.ProductId == request.ProductId).ToListAsync();
                    var averageRating = productReviewList.Sum(x => x.Rating) / productReviewList.Count;

                    product.Rating = averageRating;
                    product.TotalRatingCount = product.TotalRatingCount + 1;

                    var merchant = await rewardsDBContext.Merchants.FirstOrDefaultAsync(x => x.Id == request.MerchantId);
                    productReviewList = await rewardsDBContext.ProductReview.Where(x => x.MerchantId == request.MerchantId).ToListAsync();
                    averageRating = productReviewList.Sum(x => x.Rating) / productReviewList.Count;
                    merchant.Rating = averageRating;
                    merchant.TotalRatingCount = merchant.TotalRatingCount + 1;
                    

                    await rewardsDBContext.SaveChangesAsync();
                    //response.Successful = true;
                    //response.Message = "Order Item reviewed";
                }

                response.Code = 0;
                response.Data.IsSuccessful = true;
                response.Data.Message = "Successfully created review";
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    ErrorMessage = "Something is not right.. Please try again later."
                });
            }
        }


        protected string ValidateFile(FileProperty file) 
        {
            if (!_allowedExtention.Contains(file.Extention)) { 
                return "Please upload Images having extensions: png, jpeg, jpg, jfif ,mp4 only";
            }
            if (file.Extention == ".mp4") {
                if (file.Size > _tenMegaBytes) return "Maximum File size for video is 10MB";
            }
            else {
                if (file.Size > _oneMegaByyes) return "Maximum File size for image is 1MB";
            }

            return null;

        }

        protected async Task<string> UploadContent(IFormFile file, FileProperty property, int productId ) 
          {
              byte[] fileData;
              using (MemoryStream ms = new MemoryStream())
              {
                  file.CopyTo(ms);
                  fileData = ms.ToArray();
                var now = DateTime.Now.ToString("ddMMyyyyss"); ;

                var filename = $"{now}-{property.Filename.Replace(" ","-")}{property.Extention}";
                  ms.Position = 0;
                  var blob = await azureBlobStorage.UploadBlobAsync(ms,$"reviews/{productId}/review/{filename}", property.ContentType,
                    _containerName, true);
                  if (blob == null)
                  {
                      _errorMessage = "Fail to process request. Please try again later";

                    return null;
                  }
                  
                  return blob.StorageUri.PrimaryUri.ToString().Replace("http://", "https://");
              }
          }

        protected CreateReviewRequestModel GenerateRequestObject(IFormCollection form) {
            
            CreateReviewRequestModel result = new CreateReviewRequestModel();

            string id = form["orderItemId"];
            if (id != null) {
                try
                {
                    Guid orderItemId = new Guid(id);
                    result.OrderItemId = orderItemId;
                }
                catch (Exception ex) {

                    _errorMessage = "Invalid Order Item Id [001]";
                    return null;
                }
            }


            string comment = form["comment"];
            
            result.Comment = comment;
            result.Rating = decimal.Parse(form["rating"]);

            int _merchantId = 0;
            if (int.TryParse(form["merchantId"], out _merchantId))
            {
                result.MerchantId = _merchantId;
            }
            else
            {
                _errorMessage = "Invalid Merchant Id [001]";
                return null;
            }

            int _productId = 0;
            if (int.TryParse(form["productId"], out _productId))
            {
                result.ProductId = _productId;
            }
            else
            {
                _errorMessage = "Invalid Product Id [001]";
                return null;
            }

            return result;

        }
        protected class FileProperty
        {
            public string Filename { get; set; }
            public string Extention { get; set; }
            public long Size { get; set; }

            public string ContentType { get; set; }
        }

        protected class CreateReviewResponseModel : ApiResponseViewModel
        {
            public CreateReviewData Data { get; set; }
        }

        protected class CreateReviewData
        {
            public bool IsSuccessful { get; set; }
            public string Message { get; set; }
        }

        protected class CreateReviewRequestModel
        {
            public Guid OrderItemId { get; set; }
            public string Comment { get; set; }
            public List<FileUpload> Files { get; set; }
            public decimal Rating { get; set; }
            public int MerchantId { get; set; }

            public int ProductId { get; set; }

        }

        public class FileUpload
        {
            public string Filename { get; set; }
        }

    }
}