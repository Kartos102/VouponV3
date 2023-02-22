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
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Chats.Commands
{
    public class UpdateChatGroupUserReadStatusCommand : IRequest<ApiResponseViewModel>
    {
        public Guid GroupChatId { get; set; }
        public string UserId { get; set; }
    }

    public class UpdateChatGroupUserReadStatusCommandHandler : IRequestHandler<UpdateChatGroupUserReadStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateChatGroupUserReadStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateChatGroupUserReadStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var chatGroupUser = await rewardsDBContext.ChatGroupUsers.Where(y=> y.ChatGroupId == request.GroupChatId && y.UserId == request.UserId).FirstOrDefaultAsync();
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
