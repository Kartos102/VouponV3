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
    public class UpdateMerchantPendingChangesCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public int? CompanyTypeId { get; set; }
        public int? CountryId { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public int? PostcodeId { get; set; }
        public string DisplayName { get; set; }
        public string CompanyName { get; set; }
        public string RegistrationNumber { get; set; }
        public string CompanyContact { get; set; }
        public string CompanyWebsite { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public int StatusTypeId     { get; set; }
        public string LogoUrl { get; set; }
        public string BIDDocumentUrl { get; set; }
        public string Description { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateMerchantPendingChangesCommandHandler : IRequestHandler<UpdateMerchantPendingChangesCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateMerchantPendingChangesCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateMerchantPendingChangesCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var merchant = await rewardsDBContext.Merchants.FirstAsync(x => x.Id == request.MerchantId);
            if (merchant != null)
            {
                var jsonString = "";
                if (String.IsNullOrEmpty(merchant.PendingChanges))
                {
                    jsonString = JsonConvert.SerializeObject(merchant);
                }
                else
                    jsonString = merchant.PendingChanges;
                var newChanges = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Merchants>(jsonString);
                newChanges.Description = request.Description;
                // var newChanges = new Database.Postgres.RewardsEntities.Merchants();              
                newChanges.StatusTypeId = request.StatusTypeId;
                //newChanges.Id = merchant.Id;
               // newChanges.CreatedAt = merchant.CreatedAt;
                //newChanges.CreatedByUserId = merchant.CreatedByUserId;
                newChanges.BIDDocumentUrl = request.BIDDocumentUrl;
                newChanges.LogoUrl = request.LogoUrl;
                newChanges.BusinessTypeId = request.CompanyTypeId;
                newChanges.CountryId = request.CountryId;
                newChanges.ProvinceId = request.ProvinceId;
                newChanges.DistritId = request.DistrictId;
                newChanges.PostCodeId = request.PostcodeId;
                newChanges.DisplayName = request.DisplayName;
                newChanges.CompanyName = request.CompanyName;
                newChanges.RegistrationNumber = request.RegistrationNumber;
                newChanges.CompanyContact = request.CompanyContact;
                newChanges.WebsiteSiteUrl = request.CompanyWebsite;
                newChanges.Address_1 = request.Address_1;
                newChanges.Address_2 = request.Address_2;
                newChanges.LastUpdatedAt = request.LastUpdatedAt;
                newChanges.LastUpdatedByUserId = request.LastUpdatedByUserId;
                newChanges.PendingChanges = "";
                var pendingChanges = JsonConvert.SerializeObject(newChanges);
                merchant.PendingChanges = pendingChanges;

                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Merchant Successfully";
            }
            else
            {
                response.Message = "Merchant not found";
            }
            return response;
        }
    }
}
