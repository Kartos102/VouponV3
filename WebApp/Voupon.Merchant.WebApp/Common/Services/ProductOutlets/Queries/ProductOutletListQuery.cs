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

namespace Voupon.Merchant.WebApp.Common.Services.ProductOutlets.Queries
{
    public class ProductOutletListQuery : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
    }
    public class ProductOutletListQueryHandler : IRequestHandler<ProductOutletListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public ProductOutletListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(ProductOutletListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.ProductOutlets.Where(x => x.ProductId == request.ProductId).Include(x => x.Outlet).Include(x => x.Outlet.Country).Include(x => x.Outlet.Province).Include(x => x.Outlet.District).Include(x => x.Outlet.PostCode).ToListAsync();
                List<OutletModel> list = new List<OutletModel>();
                foreach (var productOutlet in items)
                {
                    var item = productOutlet.Outlet;
                    OutletModel newItem = new OutletModel();
                    newItem.Id = item.Id;
                    newItem.MerchantId = item.MerchantId;
                    newItem.Address_1 = item.Address_1;
                    newItem.Address_2 = item.Address_2;
                    newItem.Contact = item.Contact;
                    newItem.CountryId = item.CountryId;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.CreatedByUserId = item.CreatedByUserId;
                    newItem.DistrictId = item.DistrictId;
                    newItem.District = item.District != null ? item.District.Name : "";
                    newItem.ImgUrl = item.ImgUrl;
                    newItem.IsActivated = item.IsActivated;
                    newItem.LastUpdatedAt = item.LastUpdatedAt;
                    newItem.LastUpdatedByUserId = item.LastUpdatedByUserId;
                    newItem.Name = item.Name;
                    newItem.OpeningHour_1 = item.OpeningHour_1;
                    newItem.OpeningHour_2 = item.OpeningHour_2;
                    newItem.OpeningHour_3 = item.OpeningHour_3;
                    newItem.OpeningHour_4 = item.OpeningHour_4;
                    newItem.OpeningHour_5 = item.OpeningHour_5;
                    newItem.OpeningHour_6 = item.OpeningHour_6;
                    newItem.OpeningHour_7 = item.OpeningHour_7;
                    newItem.OpeningHour_8 = item.OpeningHour_8;
                    newItem.PostCodeId = item.PostCodeId;
                    newItem.ProvinceId = item.ProvinceId;
                    newItem.Province = item.Province != null ? item.Province.Name : "";
                    newItem.StreetName = item.StreetName;
                    list.Add(newItem);
                }
                response.Successful = true;
                response.Message = "Get Product Outlet List Successfully";
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
