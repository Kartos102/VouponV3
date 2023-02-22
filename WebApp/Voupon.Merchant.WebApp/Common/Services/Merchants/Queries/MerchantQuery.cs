using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Merchants.Queries
{
    public class MerchantQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }

    public class MerchantQueryHandler : IRequestHandler<MerchantQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public MerchantQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(MerchantQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var merchant = await rewardsDBContext.Merchants.Include(x => x.Country).Include(x => x.Distrit).Include(x => x.PostCode).Include(x => x.Province).Include(x => x.StatusType).Include(x => x.BusinessType).Include(x => x.PersonInCharges).Include(x => x.BankAccounts).ThenInclude(x => x.Bank).FirstOrDefaultAsync(x => x.Id == request.MerchantId);
                if (merchant != null)
                {
                    MerchantModel merchantModel = new MerchantModel();
                    merchantModel.Id = merchant.Id;
                    merchantModel.Code = merchant.Code;
                    merchantModel.CountryId = merchant.CountryId;
                    merchantModel.CountryName = merchant.Country != null ? merchant.Country.Name : null;
                    merchantModel.ProvinceId = merchant.ProvinceId;
                    merchantModel.ProvinceName = merchant.Province != null ? merchant.Province.Name : null;
                    merchantModel.DistritId = merchant.DistritId;
                    merchantModel.DistritName = merchant.Distrit != null ? merchant.Distrit.Name : null;
                    merchantModel.PostCodeId = merchant.PostCodeId;
                    merchantModel.PostCodeName = merchant.PostCode != null ? merchant.PostCode.Name : null;
                    merchantModel.Address_1 = merchant.Address_1;
                    merchantModel.Address_2 = merchant.Address_2;
                    merchantModel.RegistrationNumber = merchant.RegistrationNumber;
                    merchantModel.BIDDocumentUrl = merchant.BIDDocumentUrl;
                    merchantModel.BusinessTypeId = merchant.BusinessTypeId;
                    merchantModel.BusinessType = merchant.BusinessType != null ? merchant.BusinessType.Name : null;
                    merchantModel.CompanyContact = merchant.CompanyContact;
                    merchantModel.CompanyName = merchant.CompanyName;
                    merchantModel.Description = merchant.Description;
                    merchantModel.DisplayName = merchant.DisplayName;
                    merchantModel.LogoUrl = merchant.LogoUrl;
                    merchantModel.WebsiteSiteUrl = merchant.WebsiteSiteUrl;
                    merchantModel.CreatedAt = merchant.CreatedAt;
                    merchantModel.CreatedByUserId = merchant.CreatedByUserId;
                    merchantModel.LastUpdatedAt = merchant.LastUpdatedAt;
                    merchantModel.LastUpdatedByUserId = merchant.LastUpdatedByUserId;
                    merchantModel.StatusTypeId = merchant.StatusTypeId;
                    merchantModel.Status = merchant.StatusType.Name;
                    merchantModel.Remarks = merchant.Remarks;
                    merchantModel.IsPublished = merchant.IsPublished;
                    merchantModel.DefaultCommission = merchant.DefaultCommission;
                    response.Successful = true;
                    response.Message = "Get Merchant Successfully";
                    response.Data = merchantModel;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Merchant not found";
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
