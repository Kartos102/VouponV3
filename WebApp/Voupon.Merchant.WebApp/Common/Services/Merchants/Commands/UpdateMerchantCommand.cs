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
    public class UpdateMerchantCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public int CompanyTypeId { get; set; }
        public int CountryId { get; set; }
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public int PostcodeId { get; set; }
        public string DisplayName { get; set; }
        public string CompanyName { get; set; }
        public string RegistrationNumber { get; set; }
        public string CompanyContact { get; set; }
        public string CompanyWebsite { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public string Description { get; set; }
        
        public string LogoUrl { get; set; }
        public string BIDDocumentUrl { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }

    public class UpdateMerchantCommandHandler : IRequestHandler<UpdateMerchantCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateMerchantCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateMerchantCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var merchant = await rewardsDBContext.Merchants.FirstAsync(x => x.Id == request.MerchantId);
            if (merchant != null)
            {
                var merchant2 = await rewardsDBContext.Merchants.AsNoTracking().FirstAsync(x => x.Id == request.MerchantId);
                merchant2.BIDDocumentUrl = request.BIDDocumentUrl;
                merchant2.LogoUrl = request.LogoUrl;
                merchant2.BusinessTypeId = request.CompanyTypeId;
                merchant2.CountryId = request.CountryId;
                merchant2.ProvinceId = request.ProvinceId;
                merchant2.DistritId = request.DistrictId;
                merchant2.PostCodeId = request.PostcodeId;
                merchant2.DisplayName = request.DisplayName;
                merchant2.CompanyName = request.CompanyName;
                merchant2.RegistrationNumber = request.RegistrationNumber;
                merchant2.CompanyContact = request.CompanyContact;
                merchant2.WebsiteSiteUrl = request.CompanyWebsite;
                merchant2.Address_1 = request.Address_1;
                merchant2.Address_2 = request.Address_2;
                merchant2.LastUpdatedAt = request.LastUpdatedAt;
                merchant2.LastUpdatedByUserId = request.LastUpdatedByUserId;
                merchant2.Description = request.Description;
                merchant2.PendingChanges = "";

                merchant.Description = request.Description;
                merchant.BIDDocumentUrl = request.BIDDocumentUrl;
                merchant.LogoUrl = request.LogoUrl;
                merchant.BusinessTypeId = request.CompanyTypeId;
                merchant.CountryId = request.CountryId;
                merchant.ProvinceId = request.ProvinceId;
                merchant.DistritId = request.DistrictId;
                merchant.PostCodeId = request.PostcodeId;
                merchant.DisplayName = request.DisplayName;
                merchant.CompanyName = request.CompanyName;
                merchant.RegistrationNumber = request.RegistrationNumber;
                merchant.CompanyContact = request.CompanyContact;
                merchant.WebsiteSiteUrl = request.CompanyWebsite;
                merchant.Address_1 = request.Address_1;
                merchant.Address_2 = request.Address_2;
                merchant.LastUpdatedAt = request.LastUpdatedAt;
                merchant.LastUpdatedByUserId = request.LastUpdatedByUserId;
                merchant.PendingChanges = "";
                var pendingChanges = JsonConvert.SerializeObject(merchant2);
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
