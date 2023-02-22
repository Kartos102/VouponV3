using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Outlets.Commands
{
    public class CreateOutletCommand : IRequest<ApiResponseViewModel>
    {
        public int MerchantId { get; set; }
        public string Name { get; set; }
        public string StreetName { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public int CountryId { get; set; }
        public int ProvinceId { get; set; }
        public int DistrictId { get; set; }
        public int PostCodeId { get; set; }
        public string Contact { get; set; }
        public string ImgUrl { get; set; }
        public string OpeningHour_1 { get; set; }
        public string OpeningHour_2 { get; set; }
        public string OpeningHour_3 { get; set; }
        public string OpeningHour_4 { get; set; }
        public string OpeningHour_5 { get; set; }
        public string OpeningHour_6 { get; set; }
        public string OpeningHour_7 { get; set; }
        public string OpeningHour_8 { get; set; }
        public bool IsActivated { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
    }

    public class CreateOutletCommandHandler : IRequestHandler<CreateOutletCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CreateOutletCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CreateOutletCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var outlet = new Voupon.Database.Postgres.RewardsEntities.Outlets();
                outlet.Address_1 = request.Address_1;
                outlet.Address_2 = request.Address_2;
                outlet.Contact = request.Contact;
                outlet.CountryId = request.CountryId;
                outlet.CreatedAt = request.CreatedAt;
                outlet.CreatedByUserId = request.CreatedByUserId;
                outlet.DistrictId = request.DistrictId;
                outlet.ImgUrl = request.ImgUrl;
                outlet.IsActivated = request.IsActivated;
                outlet.MerchantId = request.MerchantId;
                outlet.Name = request.Name;
                outlet.OpeningHour_1 = request.OpeningHour_1;
                outlet.OpeningHour_2 = request.OpeningHour_2;
                outlet.OpeningHour_3 = request.OpeningHour_3;
                outlet.OpeningHour_4 = request.OpeningHour_4;
                outlet.OpeningHour_5 = request.OpeningHour_5;
                outlet.OpeningHour_6 = request.OpeningHour_6;
                outlet.OpeningHour_7 = request.OpeningHour_7;
                outlet.OpeningHour_8 = request.OpeningHour_8;
                outlet.PostCodeId = request.PostCodeId;
                outlet.ProvinceId = request.ProvinceId;
                outlet.StreetName = request.StreetName;
                await rewardsDBContext.Outlets.AddAsync(outlet);
                rewardsDBContext.SaveChanges();
                response.Data = outlet.Id;
                response.Successful = true;
                response.Message = "Add Outlet Successfully";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
