using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.Services.Chats.Commands
{
    public class DeleteChatCommand : IRequest<ApiResponseViewModel>
    {
        public string ChatGroupId { get; set; }
    }

    public class DeleteChatCommandHandler : IRequestHandler<DeleteChatCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public DeleteChatCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DeleteChatCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            var chatGroupUsers = await rewardsDBContext.ChatGroupUsers.Include(x=> x.ChatGroup).ThenInclude(x=> x.ChatMessages).Where(y=> y.ChatGroupId.ToString() == request.ChatGroupId).ToListAsync();
            if (chatGroupUsers != null)
            {
                foreach(var chatGroupUser in chatGroupUsers)
                {
                    chatGroupUser.IsUserDeleted = true;
                }
                foreach (var messages in chatGroupUsers.FirstOrDefault().ChatGroup.ChatMessages.ToList())
                {
                    messages.IsUserDeleted = true;
                }
                
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Delete chat Successfully";
            }
            else
            {
                response.Message = "chat not found";
            }
            return response;
        }
    }
}
