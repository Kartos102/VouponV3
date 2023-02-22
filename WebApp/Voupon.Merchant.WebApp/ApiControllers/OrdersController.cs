using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Services.Orders.Queries;

namespace Voupon.Merchant.WebApp.ApiControllers
{
    [Route("v1/api/[controller]")]
    [ApiController]
    public class OrdersController : BaseApiController
    {
        [HttpGet]
        [Route("member-order-points")]
        public async Task<IActionResult> MemberOrderPoints()
        {
            if (!IsSecretValid())
            {
                return BadRequest();
            }

            if(Request.Query["id"] == "")
            {
                return BadRequest();
            }

            var result = await Mediator.Send(new MemberPointsQuery
            {
                MasterMemberProfileId = int.Parse(Request.Query["id"])
            });

            if (result.Successful)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }

        }

        [HttpGet]
        [Route("member-sync-points")]
        public async Task<IActionResult> MemberSyncPoints()
        {
            if (!IsSecretValid())
            {
                return BadRequest();
            }

            if (Request.Query["id"] == "")
            {
                return BadRequest();
            }

            var result = await Mediator.Send(new SyncMasterMemberProfilepointsQuery
            {
                MasterMemberProfileId = int.Parse(Request.Query["id"])
            });

            if (result.Successful)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Message);
            }

        }


    }
}
