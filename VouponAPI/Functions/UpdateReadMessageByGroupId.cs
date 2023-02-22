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
using Voupon.API.Util;
using Voupon.API.ViewModels;

namespace Voupon.API.Functions
{
    public class UpdateReadMessageByGroupId
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private string _userId;
        public UpdateReadMessageByGroupId(RewardsDBContext rewardsDBContext)
        {
            this._rewardsDBContext = rewardsDBContext;
        }
        [OpenApiOperation(operationId: "update read message", tags: new[] { "Chat" }, Description = "update read message", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UpdateReadMessageResponseModel), Summary = "the result is order detail")]

        [FunctionName("UpdateReadMessageByGroupId")]

        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "chat/update-read-message/{chatGroupId}")] HttpRequest req, ILogger log, Guid chatGroupId) {
            UpdateReadMessageResponseModel response = new UpdateReadMessageResponseModel();
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.Code = -1;
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            _userId = auth.Email;
            try
            {
                response.Data = new UpdateReadMessageData();
                var chatGroupUser = await _rewardsDBContext.ChatGroupUsers.Where(y => y.ChatGroupId == chatGroupId && y.UserTypeId != 1).FirstOrDefaultAsync();
                if (chatGroupUser != null)
                {
                    chatGroupUser.UnreadedMessagesCount = 0;

                    _rewardsDBContext.SaveChanges();
                    response.Code = 0;
                    response.Data.Message = "update read message Successfully";
                    return new OkObjectResult(response);
                }
                else
                {
                    response.Code = -1;
                    response.ErrorMessage = "message not found";
                    return new NotFoundObjectResult(response);
                }
                
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.ErrorMessage = ex.Message;
                return new BadRequestObjectResult(response);
            }
        }
        private class UpdateReadMessageResponseModel : ApiResponseViewModel
        {
            public UpdateReadMessageData Data { get; set; }
        }

        private class UpdateReadMessageData
        {
            public string Message { get; set; }
        }
    }
}
