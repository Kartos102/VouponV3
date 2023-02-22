using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Chats.Queries
{
    public class ChatQuery : IRequest<ApiResponseViewModel>
    {
        public string UserId { get; set; }
    }

    public class ChatQueryHandler : IRequestHandler<ChatQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ChatQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ChatQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                //BankAccountModel bankAccountModel = new BankAccountModel();
                var messages = await rewardsDBContext.ChatGroup.Include(x => x.ChatGroupUsers).Include(x => x.ChatMessages).Where(x => x.ChatGroupUsers.Any(y => y.UserId == request.UserId)).OrderByDescending(x => x.LastUpdatedAt).AsNoTracking().ToListAsync();

                if (messages != null)
                {
                    foreach(var chatGroup in messages)
                    {
                        var chatusers = chatGroup.ChatGroupUsers.ToList();
                        for (int i = chatusers.Count - 1; i >= 0; i--)
                        {
                            if (chatusers[i].UserId == request.UserId)
                            {
                                chatusers.RemoveAt(i);
                                continue;
                            }
                            chatusers[i].ChatGroup = null;
                        }
                        chatGroup.ChatGroupUsers = chatusers;
                        foreach (var chatMessage in chatGroup.ChatMessages)
                        {
                            chatMessage.ToGroup = null;
                        }
                    }
                    response.Successful = true;
                    response.Message = "Get Messages Successfully";
                    response.Data = messages;
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
