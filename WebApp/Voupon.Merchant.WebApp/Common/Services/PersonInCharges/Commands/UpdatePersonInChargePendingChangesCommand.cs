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
    public class UpdatePersonInChargePendingChangesCommand : IRequest<ApiResponseViewModel>
    {
        public int StatusTypeId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Contact { get; set; }
        public string Position { get; set; }
        public string IdentityNumber { get; set; }
        public string DocumentUrl { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdatePersonInChargePendingChangesCommandHandler : IRequestHandler<UpdatePersonInChargePendingChangesCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdatePersonInChargePendingChangesCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdatePersonInChargePendingChangesCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var personInCharge = await rewardsDBContext.PersonInCharges.FirstAsync(x => x.Id == request.Id);
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
                newItem.StatusTypeId = request.StatusTypeId;                            
                newItem.DocumentUrl = request.DocumentUrl;
                newItem.Name = request.Name;
                newItem.Contact = request.Contact;
                newItem.Position = request.Position;
                newItem.IdentityNumber = request.IdentityNumber;
                newItem.LastUpdatedAt = request.LastUpdatedAt;
                newItem.LastUpdatedByUser = request.LastUpdatedByUserId;
                personInCharge.PendingChanges = "";
                var pendingChanges = JsonConvert.SerializeObject(newItem);
                personInCharge.PendingChanges = pendingChanges;
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Person In Charge Info Successfully";
            }
            else
            {
                response.Message = "Person In Charge not found";
            }
            return response;
        }    

    }
}
