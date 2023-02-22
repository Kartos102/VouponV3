using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Database.Postgres.RewardsEntities;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ChatSystemAzureFunction.Services.Blob;
//using ChatSystemAzureFunction.Services.Blob.Commands.Create;
using ChatSystemAzureFunction.ViewModels;
using MediatR;
using Voupon.Common.Encryption;

namespace ChatSystemAzureFunction
{
    public class Chat : ServerlessHub
    {
        private static HttpClient httpClient = new HttpClient();
        private readonly IAzureBlob azureBlob;
        private readonly VodusV2Context vodusV2Context;
        private readonly RewardsDBContext rewardsDBContext;

        public Chat(IAzureBlob azureBlob, VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
        {
            this.azureBlob = azureBlob;
            this.vodusV2Context = vodusV2Context;
            this.rewardsDBContext = rewardsDBContext;
        }

        [FunctionName("index")]
        public static IActionResult Index([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, ExecutionContext context)
        {
            var path = Path.Combine(context.FunctionAppDirectory, "content", "index.html");
            return new ContentResult
            {
                Content = File.ReadAllText(path),
                ContentType = "text/html",
            };
        }

        [FunctionName("negotiate")]
        public static SignalRConnectionInfo Negotiate(
    [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
    [SignalRConnectionInfo(HubName = "serverlessSample", UserId = "{headers.x-ms-client-principal-id}")] SignalRConnectionInfo connectionInfo)
        {
            var chatToken = req.Headers["chat-token"];
            var userId = req.Headers["x-ms-client-principal-id"];

            if (string.IsNullOrEmpty(chatToken) || string.IsNullOrEmpty(userId))
            {
                return null;
            }

            try
            {
                var chatTokenResult = ChatToken.FromChatTokenValue(chatToken);

                if (userId.ToString().Split("_")[0] != chatTokenResult.Email)
                {
                    return null;
                }

            }
            catch (Exception e)
            {
                // log
            }

            return connectionInfo;
        }


        [FunctionName("SendFiles")]
        public async Task<ApiResponseViewModel> SendFiles(
          [HttpTrigger(AuthorizationLevel.Anonymous, "post")]/*[FromForm] SendFilesRequestParams request,*/ HttpRequest req,
          [SignalR(HubName = "serverlessSample")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            try
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var requestMessage = new SendMessageObj();
                //NameValueCollection col = req.Content.ReadAsFormDataAsync().Result;
                var formdata = req.ReadFormAsync();
                //var requestMessage = req.Form.UserId;
                var files = req.Form.Files;
                var userId = req.Form["UserId"].ToString();
                var sender = req.Form["Sender"].ToString();
                int senderTypeId = Convert.ToInt32(req.Form["SenderTypeId"]);
                var typeId = Convert.ToInt32(req.Form["TypeId"]);
                string requestFrom = req.Headers["Origin"].ToString().ToLower();
                string groupString = "";
                Message message = new Message()
                {
                    IsFileAttached = true,
                    TypeId = senderTypeId,
                    Sender = sender,
                    Text = ""
                };
                requestMessage.UserId = userId;
                requestMessage.TypeId = typeId;
                ChatGroup groupChat = null;
                Guid chatId = new Guid();
                string userToSendTO = "";

                ChatUsers chatSender = new ChatUsers();
                ChatUsers chatReceiveUser = new ChatUsers();
              

                if (requestFrom == Environment.GetEnvironmentVariable("VouponMerchantAppBaseUrl").ToLower())
                {
                    if (requestMessage.TypeId == 3)
                    {
                        if (message.Sender.ToLower() == "admin@vodus.com")
                        {
                            groupString = requestMessage.UserId.ToLower() + "_" + message.Sender.ToLower();
                            // admin
                            userToSendTO = requestMessage.UserId.ToLower() + "_m";

                            chatSender.UserId = message.Sender.ToLower();
                            chatSender.UserTypeId = message.TypeId;
                            chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                            chatReceiveUser.UserTypeId = requestMessage.TypeId;

                            chatId = CreateGUIDFromString(groupString);
                            groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                        }
                        else
                        {
                            groupString = message.Sender.ToLower() + "_" + requestMessage.UserId.ToLower();
                            // admin
                            userToSendTO = "admin@vodus.com" + "_a";

                            chatSender.UserId = message.Sender.ToLower();
                            chatSender.UserTypeId = message.TypeId;
                            chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                            chatReceiveUser.UserTypeId = requestMessage.TypeId;

                            chatId = CreateGUIDFromString(groupString);
                            groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                        }
                    }
                    else
                    {
                        groupString = message.Sender.ToLower() + "_" + requestMessage.UserId.ToLower();
                        userToSendTO = requestMessage.UserId.ToLower() + "_v";

                        chatSender.UserId = message.Sender.ToLower();
                        chatSender.UserTypeId = message.TypeId;
                        chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                        chatReceiveUser.UserTypeId = requestMessage.TypeId;

                        chatId = CreateGUIDFromString(groupString);
                        groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                    }
                }
                else if (requestFrom == Environment.GetEnvironmentVariable("VouponBaseUrl").ToLower())
                {
                    if (requestMessage.TypeId == 3)
                    {
                        groupString = requestMessage.UserId.ToLower() + "_" + message.Sender.ToLower();
                        // admin
                        userToSendTO = "admin@vodus.com" + "_a";

                        chatSender.UserId = message.Sender.ToLower();
                        chatSender.UserTypeId = message.TypeId;
                        chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                        chatReceiveUser.UserTypeId = requestMessage.TypeId;

                        chatId = CreateGUIDFromString(groupString);
                        groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                    }
                    else
                    {
                        groupString = requestMessage.UserId.ToLower() + "_" + message.Sender.ToLower();
                        userToSendTO = requestMessage.UserId.ToLower() + "_m";

                        chatSender.UserId = message.Sender.ToLower();
                        chatSender.UserTypeId = message.TypeId;
                        chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                        chatReceiveUser.UserTypeId = requestMessage.TypeId;

                        chatId = CreateGUIDFromString(groupString);
                        groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                    }
                }

                message.ChatGroupId = chatId.ToString();
                ChatMessages newMessage;

                if (groupChat != null)
                {
                    newMessage = new ChatMessages()
                    {
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = message.Sender,
                        ToGroupId = chatId,
                        Message = message.Text,
                        IsFileAttached = true
                    };
                    groupChat.ChatMessages.Add(newMessage);

                    var reciveruser = groupChat.ChatGroupUsers.Where(x => x.UserId == chatSender.UserId).FirstOrDefault();
                    if (reciveruser != null)
                    {
                        reciveruser.UnreadedMessagesCount++;
                        groupChat.LastUpdatedAt = DateTime.Now;
                    }
                    foreach (var user in groupChat.ChatGroupUsers.ToList())
                    {
                        user.IsUserDeleted = false;
                        user.IsMerchantDeleted = false;
                    }
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
                        var MerchantUser = rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.Email.ToLower() == chatSender.UserId).FirstOrDefault();
                        if (MerchantUser != null)
                        {
                            var merchant = rewardsDBContext.Merchants.Where(x => x.Id == MerchantUser.UserRoles.FirstOrDefault().MerchantId).FirstOrDefault();
                            if (merchant != null)
                            {
                                chatSender.UserName = merchant.DisplayName;
                                chatSender.UserProfileImageUrl = merchant.LogoUrl;
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
                        }
                    }
                    else if (chatReceiveUser.UserTypeId == 2)
                    {
                        var MerchantUser = rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.Email.ToLower() == chatReceiveUser.UserId).FirstOrDefault();
                        if (MerchantUser != null)
                        {
                            var merchant = rewardsDBContext.Merchants.Where(x => x.Id == MerchantUser.UserRoles.FirstOrDefault().MerchantId).FirstOrDefault();
                            if (merchant != null)
                            {
                                chatReceiveUser.UserName = merchant.DisplayName;
                                chatReceiveUser.UserProfileImageUrl = merchant.LogoUrl;
                            }

                        }
                    }
                    else
                    {
                        chatReceiveUser.UserName = requestMessage.UserName;
                        chatReceiveUser.UserProfileImageUrl = requestMessage.UserImageUrl;
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
                    newMessage = new ChatMessages()
                    {
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = message.Sender,
                        ToGroupId = chatId,
                        Message = message.Text,
                        IsFileAttached = true
                    };
                    chatGroup.ChatMessages.Add(newMessage);
                    rewardsDBContext.ChatGroup.Add(chatGroup);
                    message.SenderName = chatSender.UserName;
                    message.ImageUrl = chatSender.UserProfileImageUrl;
                }

                rewardsDBContext.SaveChanges();
                if (files.Count > 0)
                {
                    ApiResponseViewModel response = new ApiResponseViewModel();
                    response.Data = await azureBlob.CreateFilesCommand(newMessage.Id, files, "files", "chats", false);
                    if (response.Data != null)
                    {
                        newMessage.AttachmentsUrl = (string)response.Data;
                        rewardsDBContext.SaveChanges();
                        response.Successful = true;
                    }

                }

                await signalRMessages.AddAsync(
               new SignalRMessage
               {
                   // the message will only be sent to these user IDs
                   UserId = userToSendTO,
                   Target = "newMessage",
                   Arguments = new[] { message }
               });
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "File Sent";
                apiResponseViewModel.Data = message.ChatGroupId;
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        [FunctionName("SendMessage")]
        public async Task<ApiResponseViewModel> SendMessage(
   [HttpTrigger(AuthorizationLevel.Anonymous, "post")] SendMessageObj requestMessage, HttpRequest req,
   [SignalR(HubName = "serverlessSample")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var apiResponseViewModel = new ApiResponseViewModel();

            try
            {
                string requestFrom = req.Headers["Origin"].ToString().ToLower();
                string groupString = "";
                Message message = requestMessage.Message;
                var a = Environment.GetEnvironmentVariable("VouponMerchantAppBaseUrl");
                ChatGroup groupChat = null;
                Guid chatId = new Guid();
                string userToSendTO = "";

                ChatUsers chatSender = new ChatUsers();
                ChatUsers chatReceiveUser = new ChatUsers();

                if (requestFrom == Environment.GetEnvironmentVariable("VouponMerchantAppBaseUrl").ToLower())
                {
                    if (requestMessage.TypeId == 3)
                    {
                        if (message.Sender.ToLower() == "admin@vodus.com")
                        {
                            groupString = requestMessage.UserId.ToLower() + "_" + message.Sender.ToLower();
                            // admin
                            userToSendTO = requestMessage.UserId.ToLower() + "_m";

                            chatSender.UserId = message.Sender.ToLower();
                            chatSender.UserTypeId = message.TypeId;
                            chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                            chatReceiveUser.UserTypeId = requestMessage.TypeId;

                            chatId = CreateGUIDFromString(groupString);
                            groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                        }
                        else
                        {
                            groupString = message.Sender.ToLower() + "_" + requestMessage.UserId.ToLower();
                            // admin
                            userToSendTO = "admin@vodus.com" + "_a";

                            chatSender.UserId = message.Sender.ToLower();
                            chatSender.UserTypeId = message.TypeId;
                            chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                            chatReceiveUser.UserTypeId = requestMessage.TypeId;

                            chatId = CreateGUIDFromString(groupString);
                            groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                        }
                    }
                    else
                    {
                        groupString = message.Sender.ToLower() + "_" + requestMessage.UserId.ToLower();
                        userToSendTO = requestMessage.UserId.ToLower() + "_v";

                        chatSender.UserId = message.Sender.ToLower();
                        chatSender.UserTypeId = message.TypeId;
                        chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                        chatReceiveUser.UserTypeId = requestMessage.TypeId;

                        //merchantEmail = message.Sender.ToLower();
                        //userEmail = requestMessage.UserId.ToLower();
                        chatId = CreateGUIDFromString(groupString);
                        groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                    }
                }
                else if (requestFrom == Environment.GetEnvironmentVariable("VouponBaseUrl").ToLower())
                {
                    if (requestMessage.TypeId == 3)
                    {
                        groupString = requestMessage.UserId.ToLower() + "_" + message.Sender.ToLower();
                        // admin
                        userToSendTO = "admin@vodus.com" + "_a";

                        chatSender.UserId = message.Sender.ToLower();
                        chatSender.UserTypeId = message.TypeId;
                        chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                        chatReceiveUser.UserTypeId = requestMessage.TypeId;

                        chatId = CreateGUIDFromString(groupString);
                        groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                    }
                    else
                    {
                        groupString = requestMessage.UserId.ToLower() + "_" + message.Sender.ToLower();
                        userToSendTO = requestMessage.UserId.ToLower() + "_m";

                        chatSender.UserId = message.Sender.ToLower();
                        chatSender.UserTypeId = message.TypeId;
                        chatReceiveUser.UserId = requestMessage.UserId.ToLower();
                        chatReceiveUser.UserTypeId = requestMessage.TypeId;

                        chatId = CreateGUIDFromString(groupString);
                        groupChat = rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Where(x => x.Id == chatId && x.IsActive == true).FirstOrDefault();
                    }
                }
                message.ChatGroupId = chatId.ToString();

                if (groupChat != null)
                {
                    ChatMessages newMessage = new ChatMessages()
                    {
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = message.Sender,
                        ToGroupId = chatId,
                        Message = message.Text,
                        IsCardMessage = message.IsCardMessage
                    };
                    groupChat.ChatMessages.Add(newMessage);
                    var reciveruser = groupChat.ChatGroupUsers.Where(x => x.UserId == chatSender.UserId).FirstOrDefault();
                    if (reciveruser != null)
                    {
                        reciveruser.UnreadedMessagesCount++;
                        groupChat.LastUpdatedAt = DateTime.Now;
                    }
                    foreach (var user in groupChat.ChatGroupUsers.ToList())
                    {
                        user.IsUserDeleted = false;
                        user.IsMerchantDeleted = false;
                    }
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
                        var MerchantUser = rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.Email.ToLower() == chatSender.UserId).FirstOrDefault();
                        if (MerchantUser != null)
                        {
                            var merchant = rewardsDBContext.Merchants.Where(x => x.Id == MerchantUser.UserRoles.FirstOrDefault().MerchantId).FirstOrDefault();
                            if (merchant != null)
                            {
                                chatSender.UserName = merchant.DisplayName;
                                chatSender.UserProfileImageUrl = merchant.LogoUrl;
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
                        }
                    }
                    else if (chatReceiveUser.UserTypeId == 2)
                    {
                        var MerchantUser = rewardsDBContext.Users.Include(x => x.UserRoles).Where(x => x.Email.ToLower() == chatReceiveUser.UserId).FirstOrDefault();
                        if (MerchantUser != null)
                        {
                            var merchant = rewardsDBContext.Merchants.Where(x => x.Id == MerchantUser.UserRoles.FirstOrDefault().MerchantId).FirstOrDefault();
                            if (merchant != null)
                            {
                                chatReceiveUser.UserName = merchant.DisplayName;
                                chatReceiveUser.UserProfileImageUrl = merchant.LogoUrl;
                            }

                        }
                    }
                    else
                    {
                        chatReceiveUser.UserName = requestMessage.UserName;
                        chatReceiveUser.UserProfileImageUrl = requestMessage.UserImageUrl;
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
                    ChatMessages newMessage = new ChatMessages()
                    {
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = message.Sender,
                        ToGroupId = chatId,
                        Message = message.Text,
                        IsCardMessage = message.IsCardMessage
                    };
                    chatGroup.ChatMessages.Add(newMessage);
                    rewardsDBContext.ChatGroup.Add(chatGroup);
                    message.SenderName = chatSender.UserName;
                    message.ImageUrl = chatSender.UserProfileImageUrl;
                }

                rewardsDBContext.SaveChanges();


                await signalRMessages.AddAsync(
                new SignalRMessage
                {
                    // the message will only be sent to these user IDs
                    UserId = userToSendTO,
                    Target = "newMessage",
                    Arguments = new[] { message }
                });
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Message Sent";
                apiResponseViewModel.Data = message.ChatGroupId;
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed to Send the message: " + ex.ToString();
                return apiResponseViewModel;
            }
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
            public long DatabaseId { get; set; }
            public string Sender { get; set; }
            public string SenderName { get; set; }
            public string ImageUrl { get; set; }
            public string Text { get; set; }
            public string ChatGroupId { get; set; }
            public int TypeId { get; set; }
            public bool IsCardMessage { get; set; }
            public bool IsFileAttached { get; set; }
            public string AttachmentsUrl { get; set; }
        }

        private class ChatUsers
        {
            public string UserId { get; set; }
            public int UserTypeId { get; set; }
            public string UserName { get; set; }
            public string UserProfileImageUrl { get; set; }
        }

        public class SendFilesRequestParams
        {
            IFormFileCollection Files { get; }
            public string UserId { get; set; }
            public int UserTypeId { get; set; }
            public string Sender { get; set; }
            public int TypeId { get; set; }
        }
    }
}
