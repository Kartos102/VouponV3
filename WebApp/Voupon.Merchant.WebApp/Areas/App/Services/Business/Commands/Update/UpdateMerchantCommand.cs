using MediatR;
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
        public string companyDescribtion { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
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
            if(merchant!=null)
            {
                var merchantModel = new Merchants();
                merchantModel.BusinessTypeId = request.CompanyTypeId;
                merchantModel.CountryId = request.CountryId;
                merchantModel.ProvinceId = request.ProvinceId;
                merchantModel.DistritId = request.DistrictId;
                merchantModel.PostCodeId = request.PostcodeId;
                merchantModel.DisplayName = request.DisplayName;
                merchantModel.CompanyName = request.CompanyName;
                merchantModel.RegistrationNumber = request.RegistrationNumber;
                merchantModel.CompanyContact = request.CompanyContact;
                merchantModel.WebsiteSiteUrl = request.CompanyWebsite;
                merchantModel.Address_1 = request.Address_1;
                merchantModel.Address_2 = request.Address_2;
                merchantModel.Description = request.companyDescribtion;
                merchantModel.LastUpdatedAt = DateTime.Now;
                merchantModel.PendingChanges = "";
                var pendingChanges = JsonConvert.SerializeObject(merchantModel);
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
