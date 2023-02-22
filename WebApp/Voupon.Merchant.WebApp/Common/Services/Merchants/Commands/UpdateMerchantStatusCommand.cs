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
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Merchants.Commands
{
    public class UpdateMerchantStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public int StatusTypeId { get; set; }
        public string Remarks { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateMerchantStatusCommandHandler : IRequestHandler<UpdateMerchantStatusCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateMerchantStatusCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateMerchantStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var merchant = await rewardsDBContext.Merchants.FirstAsync(x => x.Id == request.MerchantId);
            if (merchant != null)
            {
                merchant.StatusTypeId = request.StatusTypeId;
                merchant.Remarks = request.Remarks;
                merchant.LastUpdatedAt = request.LastUpdatedAt;
                merchant.LastUpdatedByUserId = request.LastUpdatedByUserId;
                //if (request.StatusTypeId == StatusTypeEnum.APPROVED)
                //{
                //    if (!String.IsNullOrEmpty(merchant.PendingChanges))
                //    {
                //        var item = JsonConvert.DeserializeObject<Database.Postgres.RewardsEntities.Merchants>(merchant.PendingChanges);                       
                //        merchant.CountryId = item.CountryId;
                //        merchant.ProvinceId = item.ProvinceId;
                //        merchant.DistritId = item.DistritId;
                //        merchant.PostCodeId = item.PostCodeId;
                //        merchant.Address_1 = item.Address_1;
                //        merchant.Address_2 = item.Address_2;
                //        merchant.RegistrationNumber = item.RegistrationNumber;
                //        merchant.BIDDocumentUrl = item.BIDDocumentUrl;
                //        merchant.BusinessTypeId = item.BusinessTypeId;
                //        merchant.CompanyContact = item.CompanyContact;
                //        merchant.CompanyName = item.CompanyName;
                //        merchant.Description = item.Description;
                //        merchant.DisplayName = item.DisplayName;
                //        merchant.LogoUrl = item.LogoUrl;
                //        merchant.WebsiteSiteUrl = item.WebsiteSiteUrl;
                //    }
                //    merchant.PendingChanges = "";
                //    merchant.PendingChanges = JsonConvert.SerializeObject(merchant);
                //}           

                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Merchant Status Successfully";
            }
            else
            {
                response.Message = "Merchant not found";
            }
            return response;
        }
    }
}
