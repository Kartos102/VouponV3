using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Common.Services.Blob.Queries;
using Voupon.Merchant.WebApp.Common.Services.Chats.Commands;
using Voupon.Merchant.WebApp.Common.Services.Chats.Queries;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;
using Voupon.Merchant.WebApp.ViewModels;
using static Voupon.Merchant.WebApp.Common.Services.Chats.Queries.GetChatMessagesByGroupIdHandler;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant,Staff")]
    public class ChatController : BaseAppController
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Route("GetChatMessages")]
        public async Task<ApiResponseViewModel> GetChatMessages()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new ChatQuery() { UserId = User.Identity.GetUserName() });
            
            return apiResponseViewModel;
        }
        [HttpGet]
        [Route("GetChatUsers")]
        public async Task<ApiResponseViewModel> GetChatUsers()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetChatUsers() { UserId = User.Identity.GetUserName() });

            return apiResponseViewModel;
        }

         [HttpGet]
        [Route("GetChatMessagesByGroupId")]
        public async Task<ApiResponseViewModel> GetChatMessagesByGroupId(string chatGroupId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetChatMessagesByGroupId() { GroupChatId = new Guid(chatGroupId) });
            var messagesList = (List<ChatMessagesViewModel>)apiResponseViewModel.Data;
            var sasTokenResult = await Mediator.Send(new SASTokenQuery());
            foreach (var message in messagesList)
            {
                if (message.IsFileAttached && sasTokenResult.Successful)
                {
                    for (int i = 0; i < message.FilesList.Count; i++)
                    {

                        if (message.FilesList[i] != "" && message.FilesList[i] != null)
                        {
                            message.FilesList[i] = message.FilesList[i] + (string)sasTokenResult.Data;
                        }

                    }
                }
            }
            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("UpdateReadMessagesByGroupIdAndUserId")]
        public async Task<ApiResponseViewModel> UpdateReadMessagesByGroupIdAndUserId(string chatGroupId, string userId)
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new UpdateChatGroupUserReadStatusCommand() { GroupChatId = new Guid(chatGroupId), UserId = userId });
            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("GetUnreadedMessagesCount")]
        public async Task<ApiResponseViewModel> GetUnreadedMessagesCount()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetUnreadedMessagesCount() { UserId = User.Identity.GetUserName() });
            return apiResponseViewModel;
        }
    }
}
