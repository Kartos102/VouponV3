using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.Users;
using Voupon.Merchant.WebApp.Areas.App.Controllers;
using Voupon.Merchant.WebApp.Common.Services.Merchants.Queries;
using Voupon.Merchant.WebApp.Common.Services.Users.Commands.Members;
using Voupon.Merchant.WebApp.Common.Services.Users.Queries;
using Voupon.Merchant.WebApp.Common.Services.Users.Queries.Members;
using Voupon.Merchant.WebApp.ViewModels;

using ZXing;

namespace Voupon.Merchant.WebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]")]
    [Authorize(Roles = "Admin")]
    public class MembersController : BaseAppController
    {
        #region banned user list Backlog - 1756
        [Route("BanList")]
        public async Task<IActionResult> BanList()
        {
            List<BannedUsers> model = new List<BannedUsers>();
            BannedUserListQuery command = new BannedUserListQuery();
            var response = await Mediator.Send(command);
            if (response.Successful)
            {
                model = (List<BannedUsers>)response.Data;
            }

            // ------------------
            foreach (var item in model)
            {
                PointsQuery command2 = new PointsQuery() { UserId = item.UserId };
                var response2 = await Mediator.Send(command2);
                if (response2.Successful)
                {
                    var pointsModel = (ProfilePointsViewModel)response2.Data;
                    item.VPointsGained = pointsModel.DemographicPoints + pointsModel.BonusPoints  + pointsModel.ResponseCollectedPoints  ;
                }
                else
                {
                    item.VPointsGained = null;
                }
            }
            return View(model);
        }


        [HttpGet]
        [Route("GetBannedUser/{bannedId}")]
        public async Task<ApiResponseViewModel> GetBannedUser(string bannedId)
        {
            ApiResponseViewModel response = await Mediator.Send(new BannedUserQuery() { BannedId = bannedId });
            if (response.Successful)
            {
                var user = (BannedUsers)response.Data;
                response.Data = user;
            }

            return response;
        }

        [HttpPost]
        [Route("AddBannedUser")]
        public async Task<ApiResponseViewModel> AddBannedUser([FromForm] AddBannedUserCommand command)
        {
            ApiResponseViewModel response = await Mediator.Send(command);
            response.Data = null;
            return response;
        }

        [HttpPost]
        [Route("UpdateBannedUser")]
        public async Task<ApiResponseViewModel> UpdateBannedUser([FromForm] UpdateBannedUserCommand command)
        {
            ApiResponseViewModel response = await Mediator.Send(command);
            response.Data = null;
            return response;
        }

        [HttpPost]
        [Route("UnBanUser")]
        public async Task<ApiResponseViewModel> UnBanUser([FromForm] UnBannedUserCommand command)
        {
            ApiResponseViewModel response = await Mediator.Send(command);
            response.Data = null;
            return response;
        }
        #endregion banned user list Backlog - 1756


        #region Voupon Admin: Member List 1758 
        [Route("List")]
        public async Task<IActionResult> List()
        {
            List<UserProfileListViewModel> model = new List<UserProfileListViewModel>();
            UserProfileListQuery command = new UserProfileListQuery();
            var response = await Mediator.Send(command);
            if (response.Successful)
            {
                model = (List<UserProfileListViewModel>)response.Data;
            }

            // ------------------
            foreach (var item in model)
            {
                PointsQuery command2 = new PointsQuery() { UserId = item.Id.ToString() };
                var response2 = await Mediator.Send(command2);
                if (response2.Successful)
                {
                    var pointsModel = (ProfilePointsViewModel)response2.Data;
                    //item.VPoints = pointsModel.DemographicPoints + pointsModel.BonusPoints + pointsModel.UsedPoints + pointsModel.ResponseCollectedPoints + pointsModel.AvailablePoints;
                    item.VPoints = pointsModel.DemographicPoints  + pointsModel.TotalPsyPoints + pointsModel.TotalSurveyPoints + pointsModel.BonusPoints;
                }
                else
                {
                    item.VPoints = null;
                }
            }

            return View(model);
        }

        [Route("GetUserProfile/{userId}")]
        public async Task<ApiResponseViewModel> GetUserProfile(string UserId)
        {
            UserProfileViewModel model = new UserProfileViewModel();
            UserProfileQuery command = new UserProfileQuery() { UserId = UserId };

            var response = await Mediator.Send(command);
            if (response.Successful)
            {
                model = (UserProfileViewModel)response.Data;
            }

            PointsQuery command2 = new PointsQuery() { UserId = UserId };
            var response2 = await Mediator.Send(command2);
            if (response2.Successful)
            {
                var pointsModel = (ProfilePointsViewModel)response2.Data;
                model.VPoints = pointsModel.DemographicPoints + pointsModel.BonusPoints + pointsModel.UsedPoints + pointsModel.ResponseCollectedPoints + pointsModel.AvailablePoints;
            }
            response.Data = model;
            return response;
        }
        #endregion Voupon Admin: Member List 1758 
    }
}