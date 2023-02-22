using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Voupon.API.Util;
using Voupon.API.ViewModels;
using Voupon.Common.Encryption;


namespace Voupon.API.Functions
{
    public class GetChatToken
    {
        [OpenApiOperation(operationId: "Get chat token", tags: new[] { "Chat" }, Description = "Get chat token", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ChatUserTokenResponseModel), Summary = "Get chat token")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]

        [FunctionName("GetChatToken")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/token")] HttpRequest req, ILogger log)
        {

            ChatUserTokenResponseModel response = new ChatUserTokenResponseModel();
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            var userId = auth.Email;
            try
            {
                string token = new ChatToken
                {
                    Email = userId,
                    CreatedAt = DateTime.Now

                }.ToChatTokenValue();

                response.Code = 0;
                response.Data = new ChatTokenData();
                response.Data.Token = token;
                response.Data.Message = "Get Token Successfully";
                return new OkObjectResult(response);

            }
            catch{
                response.Code = -1;
                response.Data.Message = "Fail to generate token";
                return new OkObjectResult(response);

            }
           /* response.Code = 0;
            response.Data.Message = "Get Messages Successfully";
            return new OkObjectResult(response);*/
        }
        private class ChatUserTokenResponseModel : ApiResponseViewModel
        {
            public ChatTokenData Data { get; set; }
        }

        private class ChatTokenData
        {

            public string Token { get; set; }
            public string Message { get; set; }
        }

    }

}
