using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.PersonInCharges.Queries
{
    public class PersonInChargePendingChangesQuery : IRequest<ApiResponseViewModel>
    {
        public int PersonInChargeId { get; set; }
    }

    public class PersonInChargePendingChangesQueryHandler : IRequestHandler<PersonInChargePendingChangesQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public PersonInChargePendingChangesQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(PersonInChargePendingChangesQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                PersonInChargeModel personInChargeModel = new PersonInChargeModel();
                var personInCharge = await rewardsDBContext.PersonInCharges.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.PersonInChargeId);
                if (personInCharge != null)
                {
                    string json = "";
                    if (String.IsNullOrEmpty(personInCharge.PendingChanges))
                    {
                        json = JsonConvert.SerializeObject(personInCharge);
                        var temp = await rewardsDBContext.PersonInCharges.FirstOrDefaultAsync(x => x.Id == request.PersonInChargeId);
                        temp.PendingChanges = json;
                        rewardsDBContext.SaveChanges();
                    }
                    else
                    {
                        json = personInCharge.PendingChanges;
                    }
                    var item = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.PersonInCharges>(json);
                    personInChargeModel.Contact = item.Contact;
                    personInChargeModel.DocumentUrl = item.DocumentUrl;
                    personInChargeModel.CreatedAt = item.CreatedAt;
                    personInChargeModel.CreatedByUserId = item.CreatedByUserId;
                    personInChargeModel.Id = item.Id;
                    personInChargeModel.IdentityNumber = item.IdentityNumber;
                    personInChargeModel.LastUpdatedAt = item.LastUpdatedAt;
                    personInChargeModel.LastUpdatedByUser = item.LastUpdatedByUser;
                    personInChargeModel.MerchantId = item.MerchantId;
                    personInChargeModel.Name = item.Name;
                    personInChargeModel.Position = item.Position;
                    personInChargeModel.StatusTypeId = item.StatusTypeId;
                    personInChargeModel.Status = rewardsDBContext.StatusTypes.First(x => x.Id == item.StatusTypeId).Name;
                    personInChargeModel.Remarks = item.Remarks;
                    response.Successful = true;
                    response.Message = "Get Person In Charge Successfully";
                    response.Data = personInChargeModel;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Person In Charge not found";
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
