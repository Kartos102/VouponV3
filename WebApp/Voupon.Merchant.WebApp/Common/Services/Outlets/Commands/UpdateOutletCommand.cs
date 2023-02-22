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
    public class UpdateOutletCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StreetName { get; set; }
        public string Address_1 { get; set; }
        public string Address_2 { get; set; }
        public int? CountryId { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public int? PostCodeId { get; set; }
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
        public DateTime? LastUpdatedAt { get; set; }
        public Guid? LastUpdatedByUserId { get; set; }
    }


    public class UpdateOutletCommandHandler : IRequestHandler<UpdateOutletCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateOutletCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateOutletCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var outlet = rewardsDBContext.Outlets.FirstOrDefault(x => x.Id == request.Id);
                outlet.Address_1 = request.Address_1;
                outlet.Address_2 = request.Address_2;
                outlet.Contact = request.Contact;
                outlet.CountryId = request.CountryId;
                outlet.LastUpdatedAt = request.LastUpdatedAt;
                outlet.LastUpdatedByUserId = request.LastUpdatedByUserId;
                outlet.DistrictId = request.DistrictId;
                outlet.ImgUrl = request.ImgUrl;
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
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Update Outlet Successfully";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
