using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Voupon.Merchant.WebApp.Common.Services.Blob.Queries;
using Voupon.Merchant.WebApp.Common.Services.Chats.Queries;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Merchant.WebApp.Areas.App.Controllers;
using static Voupon.Merchant.WebApp.Common.Services.Chats.Queries.GetChatMessagesByGroupIdHandler;
using Voupon.Merchant.WebApp.Common.Services.Chats.Commands;
using Microsoft.AspNetCore.Identity;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ChatController : BaseAdminController
    {
        public IActionResult Index()
        {
            return View();
        }

        [Route("MerchantChat")]
        public IActionResult MerchantChat()
        {
            return View();
        }

        [Route("AllChats")]
        public IActionResult AllChats()
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
            apiResponseViewModel = await Mediator.Send(new GetAdminChatUsers() );

            return apiResponseViewModel;
        }

         [HttpGet]
        [Route("GetAllChats")]
        public async Task<ApiResponseViewModel> GetAllChats()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetAdminAllChats());

            return apiResponseViewModel;
        }
        
        [HttpGet]
        [Route("GetChatMerchants")]
        public async Task<ApiResponseViewModel> GetChatMerchants()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetAdminChatMerchants());

            return apiResponseViewModel;
        }
        [HttpGet]
        [Route("GetUnreadedMessagesCountFromUsers")]
        public async Task<ApiResponseViewModel> GetUnreadedMessagesCountFromUsers()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetAdminUnreadedMessagesCountFromUsers() );

            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("GetUnreadedMessagesCountFromMerchants")]
        public async Task<ApiResponseViewModel> GetUnreadedMessagesCountFromMerchants()
        {
            ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
            apiResponseViewModel = await Mediator.Send(new GetAdminUnreadedMessagesCountFromMerchants());

            return apiResponseViewModel;
        }

        [HttpGet]
        [Route("GetAggregatorMerchantWithUserUnreadMessageCount")]
        public async Task<ApiResponseViewModel> GetUnreadAggregatorMerchantWithUserCount()
        {
            return await Mediator.Send(new GetAggregatorMerchantWithUserUnreadMessageCount());
        }

        [HttpGet]
        [Route("GetVodusWithUserUnreadMessageCount")]
        public async Task<ApiResponseViewModel> GetVodusWithUserUnreadMessageCount()
        {
            return await Mediator.Send(new GetVodusWithUserUnreadMessageCount());
        }

        [HttpGet]
        [Route("GetMerchantWithVodusUnreadMessageCount")]
        public async Task<ApiResponseViewModel> GetMerchantWithVodusUnreadMessageCount()
        {
            return await Mediator.Send(new GetMerchantWithVodusUnreadMessageCount());
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

    }
}
