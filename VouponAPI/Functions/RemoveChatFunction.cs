using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.API.Util;
using Voupon.API.ViewModels;
using System.ComponentModel;
namespace Voupon.API.Functions
{
    public class RemoveChatFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        public RemoveChatFunction(RewardsDBContext rewardsDBContext)
        {
            this._rewardsDBContext = rewardsDBContext;
        }

        [OpenApiOperation(operationId: "Remove chat group", tags: new[] { "Chat" }, Description = "Remove chat group", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(name: "id", In = ParameterLocation.Query, Required = false, Type = typeof(int), Summary = "Cart id", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to remove chat group")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RemoveChatGroupResponseModel), Summary = "Sucessfuly remove chat")]

        [FunctionName("RemoveChatFuntion")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "chat/remove/{chatGroupId}")] HttpRequest req, Guid chatGroupId, ILogger log)
        {
            RemoveChatGroupResponseModel response = new RemoveChatGroupResponseModel();
            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.Code = -1;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }

            try
            {
                response.Data = new RemoveChatGroupData();
                var chatGroupUsers = await _rewardsDBContext.ChatGroupUsers.Include(x => x.ChatGroup).ThenInclude(x => x.ChatMessages).Where(y => y.ChatGroupId == chatGroupId).ToListAsync();
                if (chatGroupUsers != null)
                {
                    foreach (var chatGroupUser in chatGroupUsers)
                    {
                        chatGroupUser.IsUserDeleted = true;
                    }
                    foreach (var messages in chatGroupUsers.FirstOrDefault().ChatGroup.ChatMessages.ToList())
                    {
                        messages.IsUserDeleted = true;
                    }

                    _rewardsDBContext.SaveChanges();
                    response.Data.IsSuccessful = true;
                    response.Data.Message = "Delete chat Successfully";
                    return new OkObjectResult(response);
                }
                else
                {
                    response.Data.IsSuccessful = true;
                    response.Data.Message = "chat not found";
                    return new NotFoundObjectResult(response);
                }
                
            }
            catch (Exception ex) {
                response.Code = -1;

                response.ErrorMessage = ex.Message;
                return new BadRequestObjectResult(response);
            }

        }

        private class RemoveChatGroupResponseModel : ApiResponseViewModel
        {
            public RemoveChatGroupData Data { get; set; }
        }

        private class RemoveChatGroupData
        {
            public bool IsSuccessful { get; set; }

            public string Message { get; set; }
        }


    }
}
