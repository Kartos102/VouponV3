using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Common.Services.Countries.Queries;
using Voupon.Merchant.WebApp.Common.Services.Districts.Queries;
using Voupon.Merchant.WebApp.Common.Services.Postcodes.Queries;
using Voupon.Merchant.WebApp.Common.Services.Provinces.Queries;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant")]
    public class LocationsController : BaseAppController
    {
        public IActionResult Index()
        {
            return View();
        }

        #region Get
        [HttpGet]
        [Route("GetPostcodeList/{districtId}")]
        public async Task<ApiResponseViewModel> GetPostcodeList(int districtId)
        {
            ApiResponseViewModel response = await Mediator.Send(new PostcodeListQuery() { DistrictId = districtId });
            return response;
        }

        [HttpGet]
        [Route("GetDistrictList/{provinceId}")]
        public async Task<ApiResponseViewModel> GetDistrictList(int provinceId)
        {
            ApiResponseViewModel response = await Mediator.Send(new DistrictListQuery() { ProvinceId = provinceId });
            return response;
        }

        [HttpGet]
        [Route("GetProvinceList/{countryId}")]
        public async Task<ApiResponseViewModel> GetProvinceList(int countryId)
        {
            ApiResponseViewModel response = await Mediator.Send(new ProvinceListQuery() { CountryId = countryId });
            return response;
        }

        [HttpGet]
        [Route("GetCountryList")]
        public async Task<ApiResponseViewModel> GetCountryList()
        {
            ApiResponseViewModel response = await Mediator.Send(new CountryListQuery());
            return response;
        }
        #endregion
    }
}