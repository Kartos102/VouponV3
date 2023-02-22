
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
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.API.ViewModels;
using Voupon.API.Util;
using Voupon.Common.Enum;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace Voupon.API.Functions
{
    public class RequestProfileResetPasswordFunction
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;

        public RequestProfileResetPasswordFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
        }

        [OpenApiOperation(operationId: "Request reset password", tags: new[] { "Profile" }, Description = "Request reset password", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(RequestResetPasswordResponseModel), Summary = "Request reset password")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If unable to process request")]
        [OpenApiRequestBody("application/json", typeof(RequestResetPasswordRequestModel), Description = "JSON request body ")]


        [FunctionName("RequestProfileResetPasswordFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "profile/request-reset-password")] HttpRequest req, ILogger log)
        {
            var response = new RequestResetPasswordResponseModel
            {
                Data = new RequestResetPasswordData()
            };
            try
            {
                var requestModel = HttpRequestHelper.DeserializeModel<RequestResetPasswordRequestModel>(req);

                var user = await vodusV2Context.Users.Where(x => x.Email == requestModel.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    response.Code = 1;
                    response.Data.Message = $"Check your email ({requestModel.Email}) for the reset password link";
                    return new OkObjectResult(response);
                }

                var entity = new Database.Postgres.VodusEntities.PasswordResets();
                entity.Email = requestModel.Email;
                entity.CreatedAt = DateTime.Now;
                entity.ExpireAt = DateTime.Now.AddHours(2);
                entity.ResetCode = Guid.NewGuid().ToString();
                vodusV2Context.PasswordResets.Add(entity);
                vodusV2Context.SaveChanges();
                if (entity.Id == 0)
                {
                    response.Code = -1;
                    response.ErrorMessage = "Unable to generate reset request. Please try again later.";
                    return new BadRequestObjectResult(response);
                }
                else
                {
                    var resetUrl = $"{Environment.GetEnvironmentVariable(EnvironmentKey.VOUPON_URL)}/home/resetpassword?email={ entity.Email}&code={entity.ResetCode}";
                    var sendGridClient = new SendGridClient(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.API_KEY));
                    var msg = new SendGridMessage();
                    msg.SetTemplateId(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.TEMPLATES.RESET_PASSWORD));
                    msg.SetFrom(new EmailAddress(Environment.GetEnvironmentVariable(EnvironmentKey.EMAIL.FROM.NO_REPLY), "Vodus No-Reply"));
                    msg.SetSubject("Reset password request");
                    msg.AddTo(new EmailAddress(entity.Email));
                    msg.AddSubstitution("-userName-", user.FirstName + " " + user.LastName);
                    msg.AddSubstitution("-resetPasswordUrl-", resetUrl);
                    var sendgridResponse = sendGridClient.SendEmailAsync(msg).Result;

                    if (sendgridResponse.StatusCode.ToString() != "Accepted")
                    {
                        response.Code = -1;
                        response.ErrorMessage = "Unable to send email. Please try again later.";
                        return new BadRequestObjectResult(response);
                    }

                }
                response.Code = 0;
                response.Data.Message = $"Check your email ({requestModel.Email}) for the reset password link";
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.ErrorMessage = "Unable to generate reset request. Please try again later.";
                new BadRequestObjectResult(response);
            }
            return new OkObjectResult(response);
        }

        public class RequestResetPasswordRequestModel
        {
            public string Email { get; set; }
            public string Locale { get; set; }
        }

        protected class RequestResetPasswordResponseModel : ApiResponseViewModel
        {
            public RequestResetPasswordData Data { get; set; }
        }

        public class RequestResetPasswordData
        {
            public string Message { get; set; }
        }
    }
}
