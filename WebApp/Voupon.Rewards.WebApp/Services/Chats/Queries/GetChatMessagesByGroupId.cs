using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Services.Chats.Queries
{
    public class GetChatMessagesByGroupId : IRequest<ApiResponseViewModel>
    {
        public Guid GroupChatId { get; set; }
    }

    public class GetChatMessagesByGroupIdHandler : IRequestHandler<GetChatMessagesByGroupId, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IAzureBlobStorage azureBlobStorage;
        public GetChatMessagesByGroupIdHandler(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.azureBlobStorage = azureBlobStorage;

        }

        public async Task<ApiResponseViewModel> Handle(GetChatMessagesByGroupId request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var messages = await rewardsDBContext.ChatMessages.Where(x => x.ToGroupId == request.GroupChatId && x.IsUserDeleted == false).AsNoTracking().ToListAsync();


                if (messages != null)
                {
                    List<ChatMessagesViewModel> ChatMessagesViewModelList = new List<ChatMessagesViewModel>();
                    foreach (var message in messages)
                    {
                        ChatMessagesViewModel chatMessagesViewModel = new ChatMessagesViewModel();
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
                            var files = await azureBlobStorage.ListBlobsAsync(subs[3], subs[4]+"/" + subs[5]);

                            foreach (var file in files)
                            {
                                chatMessagesViewModel.FilesList.Add(file.Uri.ToString());
                            }
                        }
                        ChatMessagesViewModelList.Add(chatMessagesViewModel);
                    }
                    response.Successful = true;
                    response.Message = "Get Messages Successfully";
                    response.Data = ChatMessagesViewModelList;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Messages not found";
                }
            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public class ChatMessagesViewModel
        {
            public long Id { get; set; }
            public string CreatedByUserId { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAt { get; set; }
            public long? ParentMessageid { get; set; }
            public bool IsCardMessage { get; set; }
            public bool IsFileAttached { get; set; }
            public string AttachmentsUrl { get; set; }
            public Guid ToGroupId { get; set; }
            public bool IsReaded { get; set; }
            public List<string> FilesList { get; set; }



            public virtual ChatGroup ToGroup { get; set; }
        }
    }
}
