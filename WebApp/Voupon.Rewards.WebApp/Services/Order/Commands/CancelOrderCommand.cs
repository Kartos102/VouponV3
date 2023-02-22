using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Order.Commands
{
    public class CancelOrderCommand : IRequest<ApiResponseViewModel>
    {
        public Guid OrderId { get; set; }
        public int MasterMemberProfileId { get; set; }
        public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;

            public CancelOrderCommandHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var date = DateTime.Now.AddDays(1);
                    var order = await rewardsDBContext.Orders.Where(x => x.Id == request.OrderId && x.MasterMemberProfileId == request.MasterMemberProfileId && x.OrderStatus == 1).FirstOrDefaultAsync();
                    if (order == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid requests";
                    }

                    order.OrderStatus = 3;
                    rewardsDBContext.Orders.Update(order);
                    await rewardsDBContext.SaveChangesAsync();
                    apiResponseViewModel.Message = "Successfully cancelled order";
                    apiResponseViewModel.Successful = true;
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to check and update pending orders";
                    return apiResponseViewModel;
                }
            }

        }
    }
}
