﻿using MediatR;
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
    public class MerchantListQuery : IRequest<ApiResponseViewModel>
    {
    }
    public class MerchantListQueryHandler : IRequestHandler<MerchantListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public MerchantListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(MerchantListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var merchants = await rewardsDBContext.Merchants.AsNoTracking().Include(x => x.Outlets).Include(x => x.Country).Include(x => x.Distrit).Include(x => x.PostCode).Include(x => x.Province).Include(x => x.BankAccounts).Include(x => x.PersonInCharges).Include(x => x.StatusType).ToListAsync();
                var statusTypes = await rewardsDBContext.StatusTypes.AsNoTracking().ToListAsync();
                List<MerchantModel> list = new List<MerchantModel>();
                foreach (var item in merchants)
                {
                    MerchantModel newItem = new MerchantModel();
                    newItem.Id = item.Id;
                    newItem.Code = item.Code;
                    newItem.CountryId = item.CountryId;
                    newItem.CountryName = item.Country != null ? item.Country.Name : "";
                    newItem.ProvinceId = item.ProvinceId;
                    newItem.ProvinceName = item.Province != null ? item.Province.Name : "";
                    newItem.DistritId = item.DistritId;
                    newItem.DistritName = item.Distrit != null ? item.Distrit.Name : "";
                    newItem.PostCodeId = item.PostCodeId;
                    newItem.PostCodeName = item.PostCode != null ? item.PostCode.Name : "";
                    newItem.Address_1 = item.Address_1;
                    newItem.Address_2 = item.Address_2;
                    newItem.RegistrationNumber = item.RegistrationNumber;
                    newItem.BIDDocumentUrl = item.BIDDocumentUrl;
                    newItem.BusinessTypeId = item.BusinessTypeId;
                    newItem.CompanyContact = item.CompanyContact;
                    newItem.CompanyName = item.CompanyName;
                    newItem.Description = item.Description;
                    newItem.DisplayName = item.DisplayName;
                    newItem.LogoUrl = item.LogoUrl;
                    newItem.WebsiteSiteUrl = item.WebsiteSiteUrl;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.CreatedByUserId = item.CreatedByUserId;
                    newItem.LastUpdatedAt = item.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    newItem.StatusTypeId = item.StatusTypeId;
                    newItem.Status = item.StatusType.Name;
                    newItem.Remarks = item.Remarks;
                    newItem.TotalOutlets = item.Outlets != null ? item.Outlets.Count() : 0;
                    newItem.IsPublished = item.IsPublished;
                    newItem.IsBrandShownInHomePage = item.IsBrandShownInHomePage;

                    var personInCharge = item.PersonInCharges.FirstOrDefault();
                    if(personInCharge != null)
                    {
                        newItem.PICStatusTypeId = personInCharge.StatusTypeId;
                        newItem.PICRemarks = personInCharge.Remarks;
                        newItem.PICStatus = statusTypes.First(x => x.Id == item.PersonInCharges.First().StatusTypeId).Name;
                    }

                    var bank = item.BankAccounts.FirstOrDefault();
                    if(bank != null)
                    {
                        newItem.BankRemarks = item.BankAccounts.First().Remarks;
                        newItem.BankStatus = statusTypes.First(x => x.Id == item.BankAccounts.First().StatusTypeId).Name;
                        newItem.BankStatusTypeId = item.BankAccounts.First().StatusTypeId;
                    }

                 
                    newItem.DefaultCommission = item.DefaultCommission;
                    newItem.IsTestAccount = item.IsTestAccount;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Merchants List Successfully";
                response.Data = list;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;


        }
    }
}
