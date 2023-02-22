using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
using Microsoft.Azure.Storage;

namespace Voupon.API.Functions
{
    public class GetChatMessageGroup
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly IAzureBlobStorage _azureBlobStorage;
        private string _userId;
        public GetChatMessageGroup(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            this._rewardsDBContext = rewardsDBContext;
            this._azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Get chat message", tags: new[] { "Chat" }, Description = "Get cartchat message", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "groupId", In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "gr", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "limit", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "limit of items", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "offset", In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "offset after number of items", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(ChatMessageGroupResponseModel), Summary = "Get chat users")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to retrieve it from the server")]
        [FunctionName("GetChatMessageGroup")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "chat/message-group")] HttpRequest req, ILogger log) {
            ChatMessageGroupResponseModel response = new ChatMessageGroupResponseModel();
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }

            try
            {
                var Id = req.Query["groupId"];
                var _limit = req.Query["limit"];
                var _offset = req.Query["offset"];
                Guid groupId = new Guid();
                bool isValid = Guid.TryParse(Id, out groupId);
                if (!isValid)
                {
                    response.Code = -1;
                    response.ErrorMessage = "invalid format groupId";
                    return new BadRequestObjectResult(response);
                }
                int limit = 0;
                int.TryParse(_limit, out limit);

                int offset = 0;
                int.TryParse(_offset, out offset);


                response.Data = new ChatMessageGroupData();
                var messages = await _rewardsDBContext.ChatMessages.
                    Where(x => x.ToGroupId == groupId && x.IsUserDeleted == false).OrderByDescending(x => x.CreatedAt).Skip(limit).Take(offset)
                    .ToListAsync();
                if (messages != null)
                {
                    List<ChatMessage> messagesList = new List<ChatMessage>();
                    foreach (var message in messages)
                    {
                        ChatMessage chatMessagesViewModel = new ChatMessage();
                        chatMessagesViewModel.Id = message.Id;
                        chatMessagesViewModel.CreatedAt = message.CreatedAt;
                        chatMessagesViewModel.Message = message.Message;
                        chatMessagesViewModel.CreatedByUserId = message.CreatedByUserId;
                        chatMessagesViewModel.IsCardMessage = message.IsCardMessage;
                        chatMessagesViewModel.IsFileAttached = message.IsFileAttached;


                        if (message.IsFileAttached)
                        {
                            chatMessagesViewModel.FilesList = new List<string>();
                            string[] subs = message.AttachmentsUrl.Split('/');
                            var files = await _azureBlobStorage.ListBlobsAsync(subs[3], subs[4] + "/" + subs[5]);

                            foreach (var file in files)
                            {
                                chatMessagesViewModel.FilesList.Add(file.Uri.ToString());
                            }
                        }
                        messagesList.Add(chatMessagesViewModel);
                    }

                    var sasTokenResult = await GetSasTokenQuery();
                    foreach (var message in messagesList)
                    {
                        if (message.IsFileAttached && !string.IsNullOrEmpty(sasTokenResult))
                        {
                            for (int i = 0; i < message.FilesList.Count; i++)
                            {

                                if (message.FilesList[i] != "" && message.FilesList[i] != null)
                                {
                                   string filename = message.FilesList[i].Replace("http://", "https://");
                                    message.FilesList[i] = filename + sasTokenResult;
                                }

                            }
                        }
                    }
                    response.Code = 0;
                    response.Data.ChatMessage = messagesList;
                    response.Data.Message = "Get Messages Successfully";
                    return new OkObjectResult(response);

                }
                else {
                    response.Code = -1;
                    response.ErrorMessage = "Messages not found";
                    return new NotFoundObjectResult(response);
                }
            }
            catch (Exception ex) {
                response.Code = -1;
                response.ErrorMessage = ex.Message;
                return new UnprocessableEntityObjectResult(response);
            }

        }

        private async Task<string> GetSasTokenQuery() {

            var storageAccountName = Environment.GetEnvironmentVariable(EnvironmentKey.AZURECONFIGURATION.STORAGE_ACCOUNT);
            var accessKey = Environment.GetEnvironmentVariable(EnvironmentKey.AZURECONFIGURATION.STORAGE_KEY);
            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={accessKey}";
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var policy = new SharedAccessAccountPolicy()
            {
                Permissions = SharedAccessAccountPermissions.Read,
                Services = SharedAccessAccountServices.Blob,
                ResourceTypes = SharedAccessAccountResourceTypes.Container | SharedAccessAccountResourceTypes.Object,
                SharedAccessExpiryTime = DateTime.UtcNow.AddMinutes(30),
                Protocols = SharedAccessProtocol.HttpsOnly,
            };

            return storageAccount.GetSharedAccessSignature(policy); ;
        }

        private class ChatMessageGroupResponseModel : ApiResponseViewModel
        {
            public ChatMessageGroupData Data { get; set; }
        }

        private class ChatMessageGroupData
        {

            public List<ChatMessage> ChatMessage { get; set; }
            public string Message { get; set; }
        }

        private class ChatMessage
        {
            public long Id { get; set; }
            public string CreatedByUserId { get; set; }
            public List<string> FilesList { get; set; }
            public string Message { get; set; }
            public bool IsFileAttached { get; set; }
            public bool IsCardMessage { get; set; }
            public DateTime CreatedAt { get; set; }


        }
    }
}
