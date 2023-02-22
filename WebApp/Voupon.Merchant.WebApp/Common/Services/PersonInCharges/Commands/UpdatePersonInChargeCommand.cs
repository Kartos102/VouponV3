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

namespace Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Commands
{
    public class UpdatePersonInChargeCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Position { get; set; }
        public string IdentityNumber { get; set; }
        public string DocumentUrl { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdatePersonInChargeCommandHandler : IRequestHandler<UpdatePersonInChargeCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdatePersonInChargeCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdatePersonInChargeCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var personInCharge = await rewardsDBContext.PersonInCharges.FirstAsync(x => x.Id == request.Id);
            if (personInCharge != null)
            {
                try
                {
                    var personInCharge2 = await rewardsDBContext.PersonInCharges.AsNoTracking().FirstAsync(x => x.Id == request.Id);
                    personInCharge2.DocumentUrl = request.DocumentUrl;
                    personInCharge2.Name = request.Name;
                    personInCharge2.Contact = request.Contact;
                    personInCharge2.Position = request.Position;
                    personInCharge2.IdentityNumber = request.IdentityNumber;
                    personInCharge2.LastUpdatedAt = request.LastUpdatedAt;
                    personInCharge2.LastUpdatedByUser = request.LastUpdatedByUserId;
                    personInCharge2.PendingChanges = "";
                    var pendingChanges = JsonConvert.SerializeObject(personInCharge2);
                    personInCharge.DocumentUrl = request.DocumentUrl;
                    personInCharge.Name = request.Name;
                    personInCharge.Contact = request.Contact;
                    personInCharge.Position = request.Position;
                    personInCharge.IdentityNumber = request.IdentityNumber;
                    personInCharge.LastUpdatedAt = request.LastUpdatedAt;
                    personInCharge.LastUpdatedByUser = request.LastUpdatedByUserId;
                    personInCharge.PendingChanges = "";                    
                    
                    personInCharge.PendingChanges = pendingChanges;
                    rewardsDBContext.SaveChanges();
                    response.Successful = true;
                    response.Message = "Update Person In Charge Successfully";
                }
                catch(Exception ex)
                {
                    response.Message = ex.Message;
                }
            }
            else
            {
                response.Message = "Person In Charge not found";
            }
            return response;
        }    

    }
}
