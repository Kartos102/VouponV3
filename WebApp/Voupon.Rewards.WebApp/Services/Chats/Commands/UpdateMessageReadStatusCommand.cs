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
    public class UpdateMessageReadStatusCommand : IRequest<ApiResponseViewModel>
    {
        public long MessageId { get; set; }
    }

    public class UpdateMessageReadStatusCommandHandler: IRequestHandler<UpdateMessageReadStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateMessageReadStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateMessageReadStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var message = await rewardsDBContext.ChatMessages.Where(y=> y.Id == request.MessageId).FirstOrDefaultAsync();
            if (message != null)
            {
                message.IsReaded = true;

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
