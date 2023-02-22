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
    public class GetUnreaadMessageCount
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private string _userId;
        public GetUnreaadMessageCount(RewardsDBContext rewardsDBContext)
        {
            this._rewardsDBContext = rewardsDBContext;
        }
        [OpenApiOperation(operationId: "Get chat unread count", tags: new[] { "Chat" }, Description = "Get chat unread count", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ChatUnreadResponseModel), Summary = "Get chat users")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]

        [FunctionName("GetChatUnreadMessageCount")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/unread-message-count")] HttpRequest req, ILogger log) {
            ChatUnreadResponseModel response = new ChatUnreadResponseModel();
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            _userId = auth.Email;

            try
            {
                response.Data = new ChatUnreadMessageData();
                var ChatGroupUsers = await _rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.ChatGroupUsers.Any(y => y.UserId == _userId && y.UserTypeId == 1)).AsNoTracking().ToListAsync();
                int unreadedMessegesForUser = 0;
                if (ChatGroupUsers != null)
                {
                    foreach (var chatGroup in ChatGroupUsers)
                    {
                        var chatusers = chatGroup.ChatGroupUsers.ToList();
                        for (int i = chatusers.Count - 1; i >= 0; i--)
                        {
                            if (chatusers[i].UserTypeId != 1)
                            {
                                unreadedMessegesForUser += chatusers[i].UnreadedMessagesCount;
                            }
                            chatusers[i].ChatGroup = null;
                        }
                        chatGroup.ChatGroupUsers = chatusers;
                    }
                    response.Code = 0;
                    response.Data.Message = "Get Unreaded Messages Successfully";
                    response.Data.TotalUnreadMessage = unreadedMessegesForUser;
                    return new OkObjectResult(response);
                }
                else
                {
                    response.Code = -1;
                    response.ErrorMessage = "Messages not found";
                    return new NotFoundObjectResult(response);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.ErrorMessage = ex.Message;
                return new BadRequestObjectResult(response);
            }
            return new BadRequestObjectResult(response);

        }
        public class ChatUnreadResponseModel : ApiResponseViewModel
        {
            public ChatUnreadMessageData Data { get; set; }
        }

        public class ChatUnreadMessageData
        {
            public int TotalUnreadMessage { get; set; }
            public string Message { get; set; }
        }
    }
}
