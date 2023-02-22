﻿using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.App.Services.Business.ViewModels;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Business.Commands.Update
{
    public class UpdatePersonInChargeStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int StatusTypeId { get; set; }
        public string Remarks { get; set; }
    }

    public class UpdatePersonInChargeStatusCommandHandler : IRequestHandler<UpdatePersonInChargeStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdatePersonInChargeStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdatePersonInChargeStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var personInCharge = await rewardsDBContext.PersonInCharges.FirstAsync(x => x.Id == request.Id);
            if (personInCharge != null)
            {
                if (request.StatusTypeId == 4)
                {
                    if (!String.IsNullOrEmpty(personInCharge.PendingChanges))
                    {
                        var item = JsonConvert.DeserializeObject<PersonInCharges>(personInCharge.PendingChanges);
                        personInCharge.Contact = item.Contact;
                        //personInChargeModel.DocumentUrl = item.DocumentUrl;                       
                        personInCharge.IdentityNumber = item.IdentityNumber;        
                        personInCharge.Name = item.Name;
                        personInCharge.Position = item.Position;                      
                    }
                }
                personInCharge.StatusTypeId = request.StatusTypeId;
                personInCharge.Remarks = request.Remarks;
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Person In Charge Status Successfully";
            }
            else
            {
                response.Message = "Person In Charge not found";
            }
            return response;
        }
    }
}
