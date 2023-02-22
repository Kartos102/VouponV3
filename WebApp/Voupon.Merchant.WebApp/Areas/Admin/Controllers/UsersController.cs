using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.App.Controllers;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.Users.Commands;
using Voupon.Merchant.WebApp.Common.Services.Users.Queries;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseAppController
    {
        public class UserModel
        {
            public string UserName { get; set; }
            public string Id { get; set; }
            public string RoleId { get; set; }
            public string MerchantId { get; set; }
        }

        public async Task<IActionResult> Index()
        {
            List<Voupon.Database.Postgres.RewardsEntities.Users> model = new List<Voupon.Database.Postgres.RewardsEntities.Users>();
            UserListQuery command = new UserListQuery();
            var response = await Mediator.Send(command);
            if (response.Successful)
            {
                model = (List<Voupon.Database.Postgres.RewardsEntities.Users>)response.Data;
            }
            return View(model);
        }

        [HttpGet]
        [Route("GetRoleList")]
        public async Task<ApiResponseViewModel> GetRoleList()
        {
            ApiResponseViewModel response = await Mediator.Send(new RoleListQuery());
            if(response.Successful)
            {
               var data= (List<Voupon.Database.Postgres.RewardsEntities.Roles>)response.Data;
                data.RemoveAll(x => x.Name == "Manager" || x.Name == "Supervisor");
                response.Data = data;
            }
            return response;
        }

        [HttpGet]
        [Route("GetMerchantList")]
        public async Task<ApiResponseViewModel> GetMerchantList()
        {
            ApiResponseViewModel response = await Mediator.Send(new MerchantListQuery());
            return response;
        }


        [HttpGet]
        [Route("GetUser/{userId}")]
        public async Task<ApiResponseViewModel> GetUser(string userId)
        {
            ApiResponseViewModel response = await Mediator.Send(new UserQuery() { UserId = userId });
            if (response.Successful)
            {
                var user = (Users)response.Data;
                UserModel model = new UserModel();
                model.Id = user.Id.ToString();
                model.UserName = user.UserName;
                model.RoleId = user.UserRoles != null ? (user.UserRoles.Count > 0 ? user.UserRoles.First().RoleId.ToString() : "") : "";
                model.MerchantId = user.UserClaims.FirstOrDefault(x => x.ClaimType == "MerchantId") != null ? user.UserClaims.First().ClaimValue : "";
                response.Data = model;
            }

            return response;
        }

        [HttpPost]
        [Route("UpdateUser")]
        public async Task<ApiResponseViewModel> UpdateProduct([FromForm] UpdateUserCommand command)
        {                    
            ApiResponseViewModel response = await Mediator.Send(command);           
            return response;
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<ApiResponseViewModel> AddUser([FromForm] AddUserCommand command)
        {
            ApiResponseViewModel response = await Mediator.Send(command);
            response.Data = null;
            return response;
        }
    }
}