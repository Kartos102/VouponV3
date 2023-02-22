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
    public class UpdateMessageReadStatusByGroupIdCommand : IRequest<ApiResponseViewModel>
    {
        public Guid GroupChatId { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateMessageReadStatusByGroupIdCommandHandler : IRequestHandler<UpdateMessageReadStatusByGroupIdCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateMessageReadStatusByGroupIdCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateMessageReadStatusByGroupIdCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var chatGroupUser = await rewardsDBContext.ChatGroupUsers.Where(y => y.ChatGroupId == request.GroupChatId && y.UserId == request.UserId).FirstOrDefaultAsync();
            if (chatGroupUser != null)
            {
                chatGroupUser.UnreadedMessagesCount = 0;

                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "update read message Successfully";
            }
            else
            {
                response.Message = "message not found";
            }
            return response;
        }
    }
}
