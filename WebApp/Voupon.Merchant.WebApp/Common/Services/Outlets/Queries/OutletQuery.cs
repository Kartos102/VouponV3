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
    public class OutletQuery : IRequest<ApiResponseViewModel>
    {
        public int OutletId { get; set; }
    }

    public class OutletQueryHandler : IRequestHandler<OutletQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public OutletQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(OutletQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var outlet = await rewardsDBContext.Outlets.Include(x => x.Country).Include(x => x.PostCode).Include(x => x.District).Include(x => x.Province).FirstOrDefaultAsync(x => x.Id == request.OutletId);
                if (outlet != null)
                {
                    OutletModel newItem = new OutletModel();
                    newItem.Id = outlet.Id;
                    newItem.Name = outlet.Name;
                    newItem.ImgUrl = outlet.ImgUrl;
                    newItem.IsActivated = outlet.IsActivated;
                    newItem.LastUpdatedAt = outlet.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = outlet.LastUpdatedByUserId;
                    newItem.MerchantId = outlet.MerchantId;
                    newItem.OpeningHour_1 = outlet.OpeningHour_1;
                    newItem.OpeningHour_2 = outlet.OpeningHour_2;
                    newItem.OpeningHour_3 = outlet.OpeningHour_3;
                    newItem.OpeningHour_4 = outlet.OpeningHour_4;
                    newItem.OpeningHour_5 = outlet.OpeningHour_5;
                    newItem.OpeningHour_6 = outlet.OpeningHour_6;
                    newItem.OpeningHour_7 = outlet.OpeningHour_7;
                    newItem.OpeningHour_8 = outlet.OpeningHour_8;
                    newItem.PostCodeId = outlet.PostCodeId;
                    newItem.Postcode = outlet.PostCode != null ? outlet.PostCode.Name : "";
                    newItem.ProvinceId = outlet.ProvinceId;
                    newItem.Province = outlet.Province != null ? outlet.Province.Name : "";
                    newItem.DistrictId = outlet.DistrictId;
                    newItem.District = outlet.District != null ? outlet.District.Name : "";
                    newItem.CountryId = outlet.CountryId;
                    newItem.Country = outlet.Country != null ? outlet.Country.Name : "";
                    newItem.Address_1 = outlet.Address_1;
                    newItem.Address_2 = outlet.Address_2;
                    newItem.Contact = outlet.Contact;
                    newItem.CreatedAt = outlet.CreatedAt;
                    newItem.CreatedByUserId = outlet.CreatedByUserId;
                    newItem.StreetName = outlet.StreetName;

                    response.Successful = true;
                    response.Message = "Get Outlet Successfully";
                    response.Data = newItem;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Outlet not found";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
