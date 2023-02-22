using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Infrastructure.Enums;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Chats.Queries
{
    public class GetAggregatorMerchantWithUserUnreadMessageCount : IRequest<ApiResponseViewModel>
    {
        //public string UserId { get; set; }
    }

    public class GetAggregatorMerchantWithUserUnreadMessageCountHandler : IRequestHandler<GetAggregatorMerchantWithUserUnreadMessageCount, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public GetAggregatorMerchantWithUserUnreadMessageCountHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(GetAggregatorMerchantWithUserUnreadMessageCount request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var ChatGroupUsers = await rewardsDBContext.ChatGroup
                    .Include(x => x.ChatGroupUsers)
                    .Where(x => x.ChatGroupUsers.Any(y => y.UserTypeId == (int)UserType.Merchat))
                    .AsNoTracking().ToListAsync();
                int unreadedMessegesForUser = 0;
                if (ChatGroupUsers != null)
                {
                    foreach(var chatGroup in ChatGroupUsers)
                    {
                        bool IsExecluded = false;
                        var chatusers = chatGroup.ChatGroupUsers.ToList();
                        var temp = 0;
                        for (int i = chatusers.Count - 1; i >= 0; i--)
                        {
                            if (chatusers[i].UserTypeId == (int)UserType.Merchat)
                            {
                                unreadedMessegesForUser += chatusers[i].UnreadedMessagesCount;
                            }
                            chatusers[i].ChatGroup = null;
                        }
                        if (!IsExecluded) 
                        {
                            unreadedMessegesForUser += temp;
                        }
                        chatGroup.ChatGroupUsers = chatusers;
                    }
                    response.Successful = true;
                    response.Message = "Get Messages Successfully";
                    response.Data = unreadedMessegesForUser;
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
