using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
    public class MerchantListByStatusQuery : IRequest<ApiResponseViewModel>
    {
        public int Status { get; set; }
    }
    public class MerchantListByStatusQueryHandler : IRequestHandler<MerchantListByStatusQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public MerchantListByStatusQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(MerchantListByStatusQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var merchants = await rewardsDBContext.Merchants.Include(x => x.Outlets).Include(x => x.Country).Include(x => x.Distrit).Include(x => x.PostCode).Include(x => x.Province).Include(x => x.BankAccounts).Include(x => x.PersonInCharges).Include(x => x.StatusType).ToListAsync();
                var statusTypes = await rewardsDBContext.StatusTypes.ToListAsync();
                List<MerchantModel> list = new List<MerchantModel>();
                foreach (var item in merchants)
                {
                    bool IsMatch = false;
                    var personInCharge = item.PersonInCharges.Count > 0 ? item.PersonInCharges.First() : null;// await rewardsDBContext.PersonInCharges.FirstOrDefaultAsync(x => x.MerchantId == item.Id);
                    var bankAccount = item.BankAccounts.Count > 0 ? item.BankAccounts.First() : null;// await rewardsDBContext.BankAccounts.FirstOrDefaultAsync(x => x.MerchantId == item.Id);


                    Voupon.Database.Postgres.RewardsEntities.Merchants jsonMerchantItem = null;
                    Voupon.Database.Postgres.RewardsEntities.PersonInCharges jsonPersonInChargeItem = null;
                    Voupon.Database.Postgres.RewardsEntities.BankAccounts jsonBankAccountItem = null;
                    if (!String.IsNullOrEmpty(item.PendingChanges))
                    {
                        jsonMerchantItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.Merchants>(item.PendingChanges);
                    }

                    if (jsonMerchantItem != null)
                    {
                        if (jsonMerchantItem.StatusTypeId == request.Status)
                        {
                            IsMatch = true;
                        }
                    }
                    else
                    {
                        if (item.StatusTypeId == request.Status)
                        {
                            IsMatch = true;
                        }
                    }

                    if (personInCharge != null && !String.IsNullOrEmpty(personInCharge.PendingChanges))
                    {
                        jsonPersonInChargeItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.PersonInCharges>(personInCharge.PendingChanges);
                    }


                    if (jsonPersonInChargeItem != null)
                    {
                        if (jsonPersonInChargeItem.StatusTypeId == request.Status)
                        {
                            IsMatch = true;
                        }
                    }
                    else if (personInCharge != null)
                    {
                        if (personInCharge.StatusTypeId == request.Status)
                        {
                            IsMatch = true;
                        }
                    }


                    if (bankAccount != null && !String.IsNullOrEmpty(bankAccount.PendingChanges))
                    {
                        jsonBankAccountItem = JsonConvert.DeserializeObject<Voupon.Database.Postgres.RewardsEntities.BankAccounts>(bankAccount.PendingChanges);
                    }

                    if (jsonBankAccountItem != null)
                    {
                        if (jsonBankAccountItem.StatusTypeId == request.Status)
                        {
                            IsMatch = true;
                        }
                    }
                    else if (bankAccount != null)
                    {
                        if (bankAccount.StatusTypeId == request.Status)
                        {
                            IsMatch = true;
                        }
                    }

                    if (!IsMatch)
                    {
                        continue;
                    }


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
                    newItem.DefaultCommission = item.DefaultCommission;
                    newItem.TotalOutlets = item.Outlets != null ? item.Outlets.Count() : 0;
                    newItem.IsPublished = item.IsPublished;
                    newItem.PICStatusTypeId = item.PersonInCharges.First().StatusTypeId;
                    newItem.PICRemarks = item.PersonInCharges.First().Remarks;
                    newItem.PICStatus = statusTypes.First(x => x.Id == item.PersonInCharges.First().StatusTypeId).Name;
                    newItem.BankRemarks = item.BankAccounts.First().Remarks;
                    newItem.BankStatus = statusTypes.First(x => x.Id == item.BankAccounts.First().StatusTypeId).Name;
                    newItem.BankStatusTypeId = item.BankAccounts.First().StatusTypeId;

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
