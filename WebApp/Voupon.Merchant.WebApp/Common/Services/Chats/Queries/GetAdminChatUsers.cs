using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.Chat;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Chats.Queries
{
    public class GetAdminChatUsers : IRequest<ApiResponseViewModel>
    {
        //public string UserId { get; set; }
    }

    public class GetAdminChatUsersHandler : IRequestHandler<GetAdminChatUsers, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public GetAdminChatUsersHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(GetAdminChatUsers request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                //BankAccountModel bankAccountModel = new BankAccountModel();
                var chatGroups= await rewardsDBContext.ChatGroup
                    .Include(x => x.ChatGroupUsers)
                    .Where(x => x.ChatGroupUsers
                    .Any(y => y.UserTypeId == (int)UserType.Merchat))
                    .OrderByDescending(x => x.LastUpdatedAt).AsNoTracking().ToListAsync();

                
                if (chatGroups != null)
                {
                    List<ChatGroup> listChatGroup = chatGroups.ToList();

                    List<Guid> ids = listChatGroup.Select(x => x.Id).ToList();
                    var listChatMessage = await rewardsDBContext.ChatMessages.Where(x => ids.Contains(x.ToGroupId)).OrderByDescending(x => x.CreatedAt).ToListAsync();

                    List<AllChatViewModel> list = new List<AllChatViewModel>();

                    foreach (var chatGroupUser in chatGroups.ToList())
                    {
                        AllChatViewModel chat = new AllChatViewModel();

                        bool IsExecluded = false;
                        var chatusers = chatGroupUser.ChatGroupUsers.ToList();
                        for (int i = chatusers.Count - 1; i >= 0; i--)
                        {
                            chat.UserId = chatusers[i].UserId;
                            chat.UserName = chatusers[i].UserName;
                            chat.Id = chatusers[i].Id;
                            chat.ChatGroupId = chatusers[i].ChatGroupId;
                            
                            switch (i)
                            {
                                case 0: chat.Receiver = chatusers[i];break;
                                case 1: chat.Sender = chatusers[i]; chat.UnreadedMessagesCount += chatusers[i].UnreadedMessagesCount; break; 
                            }

                            if (chatusers[i].UserTypeId == (int)UserType.Admin)
                            {
                                IsExecluded = true;
                                chatusers = null;
                                break;
                            }
                            chatusers[i].ChatGroup = null;
                        }
                        //chatGroupUser.ChatGroupUsers = chatusers;
                        if (!IsExecluded)
                        {
                            var msg = listChatMessage.Where(x => x.ToGroupId == chat.ChatGroupId).OrderByDescending(x => x.CreatedAt).ToList();
                            //var lastmessages = rewardsDBContext.ChatMessages.Where(x => x.ToGroupId == chatGroupUser.Id).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                            chat.LastMessage = msg[0];
                            chat.LastChat = msg[0].CreatedAt;
                            list.Add(chat);
                            //chatGroups.Remove(chatGroupUser);
                        }

                    }
                    response.Successful = true;
                    response.Message = "Get Messages Successfully";
                    response.Data = list;
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
    }
}
