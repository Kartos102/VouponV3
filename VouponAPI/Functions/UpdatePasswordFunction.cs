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
using Voupon.Common.Enum;
using Microsoft.AspNetCore.Identity;
using System.Text.RegularExpressions;

namespace Voupon.API.Functions
{
    public class UpdatePasswordFunction
    {
        private readonly VodusV2Context _vodusV2Context;
        private readonly SignInManager<Database.Postgres.VodusEntities.Users> _signInManager;
        private readonly UserManager<Database.Postgres.VodusEntities.Users> _userManager;

        public UpdatePasswordFunction(VodusV2Context vodusV2Context, SignInManager<Database.Postgres.VodusEntities.Users> signInManager,UserManager<Database.Postgres.VodusEntities.Users> userManager)
        {
            _vodusV2Context = vodusV2Context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [OpenApiOperation(operationId: "Update pasword", tags: new[] { "Profile" }, Description = "Update profile password", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(UpdateProfilePasswordRequestModel), Description = "JSON request body ")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "Update profile password response message")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no JWT token provided or fail to update")]


        [FunctionName("UpdatePasswordFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "profile/password")] HttpRequest req, ILogger log)
        {
            var response = new UpdateProfilePasswordResponseModel
            {
            };

            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }

            var requestModel = HttpRequestHelper.DeserializeModel<UpdateProfilePasswordRequestModel>(req);

            if (!requestModel.NewPassword.Equals(requestModel.ConfirmNewPassword))
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "New password and reconfirm new password does not match"
                });
            }

            var user = await _vodusV2Context.Users.Where(x => x.Email == auth.Email).FirstOrDefaultAsync();
            if(user == null)
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "Invalid access [002]"
                });
            }

            // ReGex string to check if a string contains uppercase, lowercase, special character & numeric value
            var regexStr = "^(?=.*[a-z])(?=."
                        + "*[A-Z])(?=.*\\d)"
                        + "(?=.*[-+_!@#$%^&*., ?]).+$";

            Regex p = new Regex(regexStr);
            Match m = p.Match(requestModel.NewPassword);
            
            if (string.IsNullOrEmpty(requestModel.NewPassword) || !m.Success)
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "Password update failed. Password requires a minimum of 8 characters with uppercase letters, lowercase letters, numbers and a special character"
                });
            }

            try
            {
                var updateResult = await _userManager.ChangePasswordAsync(user, requestModel.CurrentPassword, requestModel.NewPassword);
                if(!updateResult.Succeeded)
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        ErrorMessage = "Fail to update password"
                    });
                }

                response.Code = 0;
                response.Data = new UpdateProfilePassword
                {
                    Message = "Successfully updated password"
                };

                return new OkObjectResult(response);

            }
            catch (Exception ex)
            {

                return new BadRequestObjectResult(new UpdateProfilePasswordResponseModel
                {
                    Code = 999,
                    ErrorMessage = "Fail to update password"
                });
            }
        }

        protected class UpdateProfilePasswordResponseModel : ApiResponseViewModel
        {
            public UpdateProfilePassword Data { get; set; }
        }

        protected class UpdateProfilePassword
        {
            public string Message { get; set; }
        }

        public class UpdateProfilePasswordRequestModel
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmNewPassword { get; set; }
        }
    }
}
