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
    public class UpdatePersonInChargePendingChangesStatusCommand : IRequest<ApiResponseViewModel>
    {       
        public int PersonInChargeId { get; set; }    
        public string Remarks { get; set; }
        public int StatusTypeId { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdatePersonInChargePendingChangesStatusCommandHandler : IRequestHandler<UpdatePersonInChargePendingChangesStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdatePersonInChargePendingChangesStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdatePersonInChargePendingChangesStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            if (!string.IsNullOrEmpty(request.Remarks))
                request.Remarks = "Vodus Admin : " + request.Remarks + "<i class='meesage-time'>" + request.LastUpdatedAt.ToString("d-M-yyyy h:mm tt") + "</i>";
            var personInCharge = await rewardsDBContext.PersonInCharges.FirstOrDefaultAsync(x => x.Id == request.PersonInChargeId);
            if (personInCharge != null)
            {
                var jsonString = "";
                if (String.IsNullOrEmpty(personInCharge.PendingChanges))
                {
                    jsonString = JsonConvert.SerializeObject(personInCharge);
                }
                else
                    jsonString = personInCharge.PendingChanges;
                var newItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.PersonInCharges>(jsonString);
                newItem.LastUpdatedAt = request.LastUpdatedAt;
                newItem.LastUpdatedByUser = request.LastUpdatedByUserId;
                newItem.PendingChanges = "";
                if (!string.IsNullOrEmpty(newItem.Remarks) && !string.IsNullOrEmpty(request.Remarks))
                    newItem.Remarks = newItem.Remarks + "<br>" + request.Remarks;
                else if (!string.IsNullOrEmpty(request.Remarks))
                    newItem.Remarks = request.Remarks;
                newItem.StatusTypeId = request.StatusTypeId;
                personInCharge.PendingChanges = "";
                personInCharge.PendingChanges = JsonConvert.SerializeObject(newItem);
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Person In Charge Info for Review";
            }
            else
            {
                response.Message = "Person In Charge not found";
            }
            return response;
        }    

    }
}
