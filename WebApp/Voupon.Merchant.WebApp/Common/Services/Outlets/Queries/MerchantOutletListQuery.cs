using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Outlets.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Outlets.Queries
{  
    public class MerchantOutletListQuery : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
    }
    public class MerchantOutletListQueryHandler : IRequestHandler<MerchantOutletListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public MerchantOutletListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(MerchantOutletListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.Outlets.Where(x => x.MerchantId == request.MerchantId && x.IsDeleted != true).Include(x => x.Country).Include(x => x.PostCode).Include(x => x.District).Include(x => x.Province).ToListAsync();
                List<OutletModel> list = new List<OutletModel>();
                foreach (var item in items)
                {
                    OutletModel newItem = new OutletModel();
                    newItem.Id = item.Id;
                    newItem.Name = item.Name;
                    newItem.ImgUrl = item.ImgUrl;
                    newItem.IsActivated = item.IsActivated;
                    newItem.LastUpdatedAt = item.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    newItem.MerchantId = item.MerchantId;
                    newItem.OpeningHour_1 = item.OpeningHour_1;
                    newItem.OpeningHour_2 = item.OpeningHour_2;
                    newItem.OpeningHour_3 = item.OpeningHour_3;
                    newItem.OpeningHour_4 = item.OpeningHour_4;
                    newItem.OpeningHour_5 = item.OpeningHour_5;
                    newItem.OpeningHour_6 = item.OpeningHour_6;
                    newItem.OpeningHour_7 = item.OpeningHour_7;
                    newItem.OpeningHour_8 = item.OpeningHour_8;
                    newItem.PostCodeId = item.PostCodeId;
                    newItem.Postcode = item.PostCode != null ? item.PostCode.Name : "";
                    newItem.ProvinceId = item.ProvinceId;
                    newItem.Province = item.Province != null ? item.Province.Name : "";
                    newItem.DistrictId = item.DistrictId;
                    newItem.District = item.District != null ? item.District.Name : "";
                    newItem.CountryId = item.CountryId;
                    newItem.Country = item.Country != null ? item.Country.Name : "";
                    newItem.Address_1 = item.Address_1;
                    newItem.Address_2 = item.Address_2;
                    newItem.Contact = item.Contact;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.CreatedByUserId = item.CreatedByUserId;
                    newItem.StreetName = item.StreetName;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Merchant Outlet List Successfully";
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
