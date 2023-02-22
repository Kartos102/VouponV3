using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.Users.Commands;
using Voupon.Merchant.WebApp.Common.Services.Users.Queries;
using Voupon.Merchant.WebApp.Services.Identity.Commands.Update;
using Voupon.Merchant.WebApp.ViewModels;
using Microsoft.AspNetCore.Identity;
using Voupon.Merchant.WebApp.Infrastructure.Extensions;

namespace Voupon.Merchant.WebApp.Areas.App.Controllers
{
    [Area("App")]
    [Route("App/[controller]")]
    [Authorize(Roles = "Merchant")]
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
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            List<Voupon.Database.Postgres.RewardsEntities.Users> model = new List<Voupon.Database.Postgres.RewardsEntities.Users>();
            UserListWithMerchantIdQuery command = new UserListWithMerchantIdQuery() { MerchantId= Int32.Parse(MerchantId)};
            var response = await Mediator.Send(command);
            if (response.Successful)
            {
                model = (List<Voupon.Database.Postgres.RewardsEntities.Users>)response.Data;
            }
            else
            {
                return View(Infrastructure.Enums.ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            return View(model);
        }

        [HttpGet]
        [Route("GetRoleList")]
        public async Task<ApiResponseViewModel> GetRoleList()
        {
            ApiResponseViewModel response = await Mediator.Send(new RoleListQuery());
            if (response.Successful)
            {
                var data = (List<Voupon.Database.Postgres.RewardsEntities.Roles>)response.Data;
                data.RemoveAll(x => x.Name == "Manager" || x.Name == "Supervisor" || x.Name == "Admin");
                response.Data = data;
            }
            return response;
        }

        [HttpGet]
        [Route("GetMerchantList")]
        public async Task<ApiResponseViewModel> GetMerchantList()
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
            ApiResponseViewModel response = await Mediator.Send(new MerchantListQuery());
            if(response.Successful)
            {
                var merchants=(List<Common.Services.Merchants.Models.MerchantModel>)response.Data;
                response.Data= merchants.Where(x => x.Id == Int32.Parse(MerchantId)).ToList();
            }
            return response;
        }


        [HttpGet]
        [Route("GetUser/{userId}")]
        public async Task<ApiResponseViewModel> GetUser(string userId)
        {
            if (!await VerifyUserAccessWithUserId(userId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

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
           
            if (!command.MerchantId.HasValue  || !VerifyUserAccess(command.MerchantId.Value))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            if (!await VerifyUserAccessWithUserId(command.UserId.ToString()))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("UpdateUserPassword")]
        public async Task<ApiResponseViewModel> UpdateUserPassword(ResetPasswordForUserCommand command)
        {
            var adminUserId = User.Identity.GetUserId();
            command.AdminUserId = adminUserId;
            if (!await VerifyUserAccessWithUserId(command.AdminUserId))
            {
                ApiResponseViewModel badRequestResponse = new ApiResponseViewModel();
                badRequestResponse.Successful = false;
                badRequestResponse.Message = "Invalid credentials";
                return badRequestResponse;
            }

            ApiResponseViewModel response = await Mediator.Send(command);
            return response;
        }

        [HttpPost]
        [Route("AddUser")]
        public async Task<ApiResponseViewModel> AddUser([FromForm] AddUserCommand command)
        {
            var userid = User.Identity.GetUserId();
            var MerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
                       

            ApiResponseViewModel response = await Mediator.Send(command);
            if(response.Successful)
            {
               var newUser=(Voupon.Database.Postgres.RewardsEntities.Users)response.Data;
                UpdateUserCommand updateUserCommand = new UpdateUserCommand();
                updateUserCommand.MerchantId = Int32.Parse(MerchantId);
                updateUserCommand.RoleId = new Guid("1EC9F440-7A2B-4E69-90C9-A4BEA022DD38");
                updateUserCommand.UserId = new Guid(newUser.Id.ToString());
                ApiResponseViewModel UpdateResponse = await Mediator.Send(updateUserCommand);
                response.Data = null;
            }
            return response;
        }

        private async Task<bool> VerifyUserAccessWithUserId(string userId)
        {
            int merchantId = await QueriesUserMerchantId(userId);
            return VerifyUserAccess(merchantId);
        }

        private async Task<int> QueriesUserMerchantId(string userId)
        {
            int merchantId = 0;
            ApiResponseViewModel response = await Mediator.Send(new UserQuery() { UserId = userId });
            if (response.Successful)
            {
                var user = (Users)response.Data;
            
                string MerchantId= user.UserClaims.FirstOrDefault(x => x.ClaimType == "MerchantId") != null ? user.UserClaims.First().ClaimValue : "";
             
                if(!String.IsNullOrEmpty(MerchantId))
                {
                    merchantId = Int32.Parse(MerchantId);
                }              
            }         
            return merchantId;
        }
        private bool VerifyUserAccess(int merchantId)
        {
            bool IsMatch = false;
            try
            {
                var userid = User.Identity.GetUserId();
                if (User.IsInRole("Admin"))
                {
                    IsMatch = true;
                    return IsMatch;
                }
                var userMerchantId = User.Claims.FirstOrDefault(x => x.Type == "MerchantId").Value;
                if (Int32.Parse(userMerchantId) == merchantId)
                {
                    IsMatch = true;
                }
            }
            catch (Exception ex)
            {
                IsMatch = false;
            }
            return IsMatch;
        }
    }
}