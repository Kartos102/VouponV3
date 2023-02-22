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

namespace Voupon.API.Functions
{
    public class GetChatGroupIdFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public GetChatGroupIdFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get chat group id", tags: new[] { "Chat" }, Description = "Get chat group id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "merchant id", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(name: "s", In = ParameterLocation.Query, Required = false, Type = typeof(string), Summary = "external shop id", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(name: "t", In = ParameterLocation.Query, Required = false, Type = typeof(byte), Summary = "External type id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(GroupChatResponseModel), Summary = "get chat group")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If no id is supplied")]


        [FunctionName("GetChatGroupId")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/group")] HttpRequest req, ILogger log)
        {
            var response = new GroupChatResponseModel
            {
                Data = new SendMessageObj()
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

            string groupString = "";
            ChatGroup groupChat = null;
            Guid chatId = new Guid();
            string userToSendTO = "";

            ChatUsers chatSender = new ChatUsers();
            ChatUsers chatReceiveUser = new ChatUsers();


            var merchantId = req.Query["id"];

            if (string.IsNullOrEmpty(merchantId))
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "Fail to get data [001]"
                });
            }

            int mechantIdValue = 0;
            int.TryParse(merchantId.ToString(), out mechantIdValue);

            if (mechantIdValue == 0)
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    Code = -1,
                    ErrorMessage = "Fail to get data [003]"
                });
            }

            var merchant = await rewardsDBContext.Merchants.Where(x => x.Id == mechantIdValue).FirstOrDefaultAsync();

            var sendMessageObject = new SendMessageObj
            {
                Message = new Message
                {
                    Sender = auth.Email.ToLower(),
                    TypeId = 1
                }
            };

            if (string.IsNullOrEmpty(merchant.ExternalId))
            {

                var userClaim = await rewardsDBContext.UserClaims.Include(x => x.User).Where(x => x.ClaimValue == merchant.Id.ToString() && x.ClaimType == "MerchantId").FirstOrDefaultAsync();

                if (userClaim == null || userClaim.User == null)
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        Code = -1,
                        ErrorMessage = "Fail to get data [003]"
                    });
                }

                var userId = userClaim.User.Email.ToLower();
                groupString = userId + "_" + auth.Email.ToLower();
                userToSendTO = userId + "_m";

                chatSender.UserId = auth.Email.ToLower();
                chatSender.UserTypeId = 1;
                chatReceiveUser.UserId = userId;
                chatReceiveUser.UserTypeId = 2;

                chatId = CreateGUIDFromString(groupString);
                groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();

                sendMessageObject.TypeId = 2;
            }
            else
            {
                groupString = merchant.ExternalId.ToLower() + "_" + auth.Email.ToLower();
                // admin
                userToSendTO = "admin@vodus.com" + "_a";

                chatSender.UserId = auth.Email.ToLower();
                chatSender.UserTypeId = 1;
                chatReceiveUser.UserId = merchant.ExternalId.ToLower();
                chatReceiveUser.UserTypeId = 3;

                chatId = CreateGUIDFromString(groupString);
                groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();

                sendMessageObject.TypeId = 3;

            }

            if (groupChat != null)
            {
                sendMessageObject.Message.ChatGroupId = groupChat.Id.ToString();

                var chatUser = groupChat.ChatGroupUsers.Where(x => x.UserTypeId != 1).FirstOrDefault();
                if (chatUser != null)
                {
                    sendMessageObject.UserId = chatUser.UserId;
                    sendMessageObject.UserImageUrl = chatUser.UserProfileImageUrl;
                    sendMessageObject.UserName = chatUser.UserName;
                }
                response.Data = sendMessageObject;
            }
            else
            {
                if (chatSender.UserTypeId == 1)
                {
                    var vouponUser = vodusV2Context.Users.Where(x => x.Email.ToLower() == chatSender.UserId).FirstOrDefault();
                    if (vouponUser != null)
                    {
                        chatSender.UserName = vouponUser.FirstName + " " + vouponUser.LastName;
                        chatSender.UserProfileImageUrl = "";
                    }
                }
                else
                {
                    var merchantUser = rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.Email.ToLower() == chatSender.UserId).FirstOrDefault();
                    if (merchantUser != null)
                    {
                        var merchantRole = rewardsDBContext.Merchants.Where(x => x.Id == merchantUser.UserRoles.FirstOrDefault().MerchantId).FirstOrDefault();
                        if (merchantRole != null)
                        {
                            chatSender.UserName = merchantRole.DisplayName;
                            chatSender.UserProfileImageUrl = merchantRole.LogoUrl;
                        }

                    }
                }
                if (chatReceiveUser.UserTypeId == 1)
                {
                    var vouponUser = vodusV2Context.Users.Where(x => x.Email.ToLower() == chatReceiveUser.UserId).FirstOrDefault();
                    if (vouponUser != null)
                    {
                        chatReceiveUser.UserName = vouponUser.FirstName + " " + vouponUser.LastName;
                        chatReceiveUser.UserProfileImageUrl = "";

                        sendMessageObject.UserId = vouponUser.Email.ToLower();
                        sendMessageObject.UserImageUrl = "";
                        sendMessageObject.UserName = chatReceiveUser.UserName;
                    }
                }
                else if (chatReceiveUser.UserTypeId == 2)
                {
                    var merchantUser = rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.Email.ToLower() == chatReceiveUser.UserId).FirstOrDefault();
                    if (merchantUser != null)
                    {
                        var merchantRole = rewardsDBContext.Merchants.Where(x => x.Id == merchantUser.UserRoles.FirstOrDefault().MerchantId).FirstOrDefault();
                        if (merchantRole != null)
                        {
                            chatReceiveUser.UserName = merchantRole.DisplayName;
                            chatReceiveUser.UserProfileImageUrl = merchantRole.LogoUrl;

                            sendMessageObject.UserId = merchantUser.Email.ToLower();
                            sendMessageObject.UserImageUrl = merchantRole.LogoUrl;
                            sendMessageObject.UserName = merchantRole.DisplayName;
                        }

                    }
                }
                else
                {
                    chatReceiveUser.UserId = merchant.ExternalId;
                    chatReceiveUser.UserName = merchant.DisplayName;
                    chatReceiveUser.UserProfileImageUrl = merchant.LogoUrl;

                    sendMessageObject.UserId = merchant.ExternalId;
                    sendMessageObject.UserImageUrl = merchant.LogoUrl;
                    sendMessageObject.UserName = merchant.DisplayName;
                }

                ChatGroup chatGroup = new ChatGroup()
                {
                    Id = chatId,
                    CreatedAt = DateTime.Now,
                    IsActive = true,
                    ChatGroupUsers = new List<ChatGroupUsers>(),
                    ChatMessages = new List<ChatMessages>()
                };
                ChatGroupUsers chatGroupSender = new ChatGroupUsers()
                {
                    ChatGroupId = chatId,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UserId = chatSender.UserId,
                    UserName = chatSender.UserName,
                    UserTypeId = chatSender.UserTypeId,
                    UserProfileImageUrl = chatSender.UserProfileImageUrl,
                    UnreadedMessagesCount = 1
                };
                ChatGroupUsers chatGroupReciver = new ChatGroupUsers()
                {
                    ChatGroupId = chatId,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UserId = chatReceiveUser.UserId,
                    UserName = chatReceiveUser.UserName,
                    UserTypeId = chatReceiveUser.UserTypeId,
                    UserProfileImageUrl = chatReceiveUser.UserProfileImageUrl
                };
                chatGroup.ChatGroupUsers.Add(chatGroupSender);
                chatGroup.ChatGroupUsers.Add(chatGroupReciver);
                rewardsDBContext.ChatGroup.Add(chatGroup);
                await rewardsDBContext.SaveChangesAsync();

                sendMessageObject.Message.ChatGroupId = chatGroup.Id.ToString();
                response.Data = sendMessageObject;

            }

            return new OkObjectResult(response);
        }

        private static Guid CreateGUIDFromString(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                Guid g = new Guid(hashBytes);
                return g;
            }
        }

        private class ChatUsers
        {
            public string UserId { get; set; }
            public int UserTypeId { get; set; }
            public string UserName { get; set; }
            public string UserProfileImageUrl { get; set; }
        }

        protected class GroupChatResponseModel : ApiResponseViewModel
        {
            public SendMessageObj Data { get; set; }
        }

        public class SendMessageObj
        {
            public Message Message { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string UserImageUrl { get; set; }
            public int TypeId { get; set; }

        }

        public class Message
        {
            public string Sender { get; set; }
            public string SenderName { get; set; }
            public string ImageUrl { get; set; }
            public string Text { get; set; }
            public string ChatGroupId { get; set; }
            public int TypeId { get; set; }
        }

    }
}
