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
    public class GetChatUsers
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private string _userId;
        private string _errorMessage;
        public GetChatUsers(RewardsDBContext rewardsDBContext)
        {
            this._rewardsDBContext = rewardsDBContext;
        }

        [OpenApiOperation(operationId: "Get chat users", tags: new[] { "Chat" }, Description = "Get chat users", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "isUnreadMessage", In = ParameterLocation.Query, Required = true, Type = typeof(bool), Summary = "Is unread message", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "searchText", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "Search by merchant Name", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = true, Type = typeof(int), Summary = "Limit for pageination", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "offset", In = ParameterLocation.Query, Required = true, Type = typeof(int), Summary = "Offset for pageination", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ChatUsersResponseModel), Summary = "Get chat users")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]

        [FunctionName("GetChatUsers")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/users")] HttpRequest req, ILogger log)
        {

            ChatUsersResponseModel response = new ChatUsersResponseModel();
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }
            _userId = auth.Email;
            ChatSearchQuery request = GetQueryParam(req);
            if (!string.IsNullOrEmpty(_errorMessage))
            {
                response.Code = -1;
                response.ErrorMessage = _errorMessage;
                return new BadRequestObjectResult(response);
            }
            try
            {
                response.Data = new ChatUsersData();
                var messages = await _rewardsDBContext.ChatGroup.AsNoTracking().
                    Include(x => x.ChatGroupUsers).
                    Include(x => x.ChatMessages).
                    Where(x => x.ChatGroupUsers.
                    Any(y => y.UserId == _userId && y.IsUserDeleted == false)).OrderByDescending(x => x.LastUpdatedAt).ToListAsync();
                //var messages = query.Skip(request.Limit).Take(request.Offset).ToList();

                List<ChatUser> chats = new List<ChatUser>();
                if (messages != null)
                {
                    foreach (var chatGroup in messages)
                    {
                        var lastMessage = chatGroup.ChatMessages.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                        ChatUser chatUser = new ChatUser();
                        var chatusers = chatGroup.ChatGroupUsers.ToList();
                        if (!string.IsNullOrEmpty(request.SearchText))
                        {
                            chatusers = chatusers.Where(x => x.UserName.ToLower().Contains(request.SearchText)).ToList();
                        }
                        if (request.IsUnreadMessage)
                        {
                            chatusers = chatusers.Where(x => x.UnreadedMessagesCount > 0).ToList();
                        }
                        var count = 0;
                        for (int i = chatusers.Count - 1; i >= 0; i--)
                        {
                            if (chatusers[i].UserId != _userId)
                            {
                                chatUser.Id = chatusers[i].Id;
                                chatUser.UserId = chatusers[i].UserId;
                                chatUser.UserTypeId = chatusers[i].UserTypeId;
                                chatUser.IsActive = chatusers[i].IsActive;
                                chatUser.ProfileImageUrl = chatusers[i].UserProfileImageUrl;
                                chatUser.MerchantName = chatusers[i].UserName;
                                chatUser.GroupChatId = chatusers[i].ChatGroupId;
                                chatUser.CreatedAt = chatusers[i].CreatedAt;

                                if (chatGroup.LastUpdatedAt.HasValue)
                                {
                                    chatUser.LastUpdatedAt = chatGroup.LastUpdatedAt.Value;
                                }
                                else
                                {
                                    chatUser.LastUpdatedAt = chatGroup.CreatedAt;
                                }

                                if (lastMessage != null)
                                {
                                    chatUser.isMerchantMessage = lastMessage.CreatedByUserId == _userId ? false : true;
                                    chatUser.LastMessage = lastMessage.Message;
                                }
                                else
                                {
                                    chatUser.isMerchantMessage = false;
                                    chatUser.LastMessage = "";
                                }
                                count = chatusers[i].UnreadedMessagesCount;
                            }
                        }
                        chatUser.UnreadedMessagesCount = count;
                        if (!string.IsNullOrEmpty(chatUser.UserId))
                        {
                            chats.Add(chatUser);
                        }
                    }
                    chats = chats.Where(x => x.Id != 0).Skip(request.Offset).Take(request.Limit).ToList();
                    response.Data.Chat = chats;
                }
                else
                {
                    response.Code = -1;
                    response.ErrorMessage = "Messages not found";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.ErrorMessage = ex.Message;
            }
            response.Code = 0;
            response.Data.Message = "Get Messages Successfully";
            return new OkObjectResult(response);
        }
        private ChatSearchQuery GetQueryParam(HttpRequest req)
        {

            ChatSearchQuery query = new ChatSearchQuery();
            query.SearchText = req.Query["searchText"];
            if (!string.IsNullOrEmpty(query.SearchText)) query.SearchText.ToLower();

            bool _isUnreadMessage = false;
            if (Boolean.TryParse(req.Query["isUnreadMessage"], out _isUnreadMessage))
            {
                query.IsUnreadMessage = _isUnreadMessage;
            }
            int _offset = 0;
            if (int.TryParse(req.Query["offset"], out _offset))
            {
                query.Offset = _offset;
            }
            else
            {
                _errorMessage = "invalid offset format value";
            }
            int _limit = 0;
            if (int.TryParse(req.Query["limit"], out _limit))
            {
                query.Limit = _limit;
            }
            else
            {
                _errorMessage = "invalid offset format value";
            }

            return query;
        }
        private class ChatSearchQuery
        {
            public string SearchText { get; set; }
            public int Limit { get; set; }
            public int Offset { get; set; }
            public bool IsUnreadMessage { get; set; }

        }
        private class ChatUsersResponseModel : ApiResponseViewModel
        {
            public ChatUsersData Data { get; set; }
        }

        private class ChatUsersData
        {

            public List<ChatUser> Chat { get; set; }
            public string Message { get; set; }
        }

        private class ChatUser
        {
            public long Id { get; set; }
            public int UnreadedMessagesCount { get; set; }
            public string UserId { get; set; }
            public string MerchantName { get; set; }
            public string ProfileImageUrl { get; set; }
            public int UserTypeId { get; set; }
            public bool IsActive { get; set; }
            public Guid GroupChatId { get; set; }
            public DateTime CreatedAt { get; set; }
            public string LastMessage { get; set; }
            public bool isMerchantMessage { get; set; }
            public DateTime LastUpdatedAt { get; set; }


        }
    }
}
