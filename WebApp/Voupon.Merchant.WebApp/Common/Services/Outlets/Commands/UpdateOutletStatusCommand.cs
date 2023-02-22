using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Outlets.Commands
{
    public class UpdateOutletStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int OutletId { get; set; }
        public bool Status { get; set; }
    }

    public class UpdateOutletStatusCommandHandler : IRequestHandler<UpdateOutletStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateOutletStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateOutletStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var outlet = rewardsDBContext.Outlets.FirstOrDefault(x => x.Id == request.OutletId);
                if (outlet != null)
                {
                    outlet.IsActivated = request.Status;
                    await rewardsDBContext.SaveChangesAsync();
                    response.Successful = true;
                    response.Message = "Update Outlet Status Successfully";
                }
                else
                {
                    response.Message = "Outlet not found";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
