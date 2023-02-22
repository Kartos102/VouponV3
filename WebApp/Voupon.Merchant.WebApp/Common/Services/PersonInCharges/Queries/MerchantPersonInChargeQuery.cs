using MediatR;
using Microsoft.EntityFrameworkCore;
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
    public class MerchantPersonInChargeQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }

    public class MerchantPersonInChargeQueryHandler : IRequestHandler<MerchantPersonInChargeQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public MerchantPersonInChargeQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(MerchantPersonInChargeQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                PersonInChargeModel personInChargeModel = new PersonInChargeModel();
                var personInCharge = await rewardsDBContext.PersonInCharges.Include(x => x.StatusType).FirstOrDefaultAsync(x => x.MerchantId == request.MerchantId);             
                if (personInCharge != null)
                {
                    personInChargeModel.Contact = personInCharge.Contact;
                    personInChargeModel.DocumentUrl = personInCharge.DocumentUrl;
                    personInChargeModel.CreatedAt = personInCharge.CreatedAt;
                    personInChargeModel.CreatedByUserId = personInCharge.CreatedByUserId;
                    personInChargeModel.Id = personInCharge.Id;
                    personInChargeModel.IdentityNumber = personInCharge.IdentityNumber;
                    personInChargeModel.LastUpdatedAt = personInCharge.LastUpdatedAt;
                    personInChargeModel.LastUpdatedByUser = personInCharge.LastUpdatedByUser;
                    personInChargeModel.MerchantId = personInCharge.MerchantId;
                    personInChargeModel.Name = personInCharge.Name;
                    personInChargeModel.Position = personInCharge.Position;
                    personInChargeModel.StatusTypeId = personInCharge.StatusTypeId;
                    personInChargeModel.Status = personInCharge.StatusType.Name;
                    personInChargeModel.Remarks = personInCharge.Remarks;
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
