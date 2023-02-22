using System;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using Voupon.API.Util;
using Voupon.API.ViewModels;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;

namespace Voupon.API.Functions
{
    public class SendAccountVerificationEmalFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly VodusV2Context _vodusV2Context;

        public SendAccountVerificationEmalFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            _rewardsDBContext = rewardsDBContext;
            _vodusV2Context = vodusV2Context;
        }

        [OpenApiOperation(operationId: "Request email verification", tags: new[] { "Email" }, Description = "Verify Email", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "email", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "email to verify")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(string), Description = "The OK response")]

        [FunctionName("SendAccountVerificationEmalFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "profile/send-email-verification")] HttpRequest req,ILogger log)
        {
            var apiResponse = new EmailVerificationResponseModel
            {
                Data = new RequestEmailVerificationData()
            };

            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                apiResponse.RequireLogin = true;
                apiResponse.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(apiResponse);
            }

            try
            {
                var requestModel = HttpRequestHelper.DeserializeModel<EmailVerificationRequestModel>(req);

                var user = await _vodusV2Context.Users.Where(x => x.Email == requestModel.Email).FirstOrDefaultAsync();
                if (user == null)
                {
                    apiResponse.ErrorMessage = "Email is not correct ";
                    apiResponse.Code = -1;
                    return new BadRequestObjectResult(apiResponse);
                }

                if (string.IsNullOrEmpty(user.ActivationCode))
                {
                    user.ActivationCode = Guid.NewGuid().ToString();
                }

                var url = $"{Environment.GetEnvironmentVariable(EnvironmentKey.VOUPON_URL)}/ConfirmAccountVerification/{ requestModel.Email}/{user.ActivationCode}";

                var sendGridClient = new SendGridClient(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.API_KEY));
                var msg = new SendGridMessage();
                msg.SetTemplateId(Environment.GetEnvironmentVariable(EnvironmentKey.SENDGRID.TEMPLATES.VERIFY_EMAIL));
                msg.SetFrom(new EmailAddress(Environment.GetEnvironmentVariable(EnvironmentKey.EMAIL.FROM.NO_REPLY), "Vodus No-Reply"));
                msg.SetSubject("Email verification");
                msg.AddTo(new EmailAddress(requestModel.Email));
                msg.AddSubstitution("-userName-", user.FirstName + " " + user.LastName);
                msg.AddSubstitution("-verificationUrl-", url);
                var sendgridResponse = sendGridClient.SendEmailAsync(msg).Result;

                if (sendgridResponse.StatusCode.ToString() != "Accepted")
                {
                    apiResponse.Code = -1;
                    apiResponse.ErrorMessage = "Unable to send verification email. Please try again.";
                    return new BadRequestObjectResult(apiResponse);
                }

                //  Update user email
                user.NewPendingVerificationEmail = requestModel.Email;
                _vodusV2Context.Users.Update(user);
                _vodusV2Context.SaveChanges();

                apiResponse.Data.Message = "Email sent successfully";
                apiResponse.Code = 0;

                return new OkObjectResult(apiResponse);
            }
            catch (Exception ex)
            {
                apiResponse.Code = -1;
                apiResponse.ErrorMessage = "Unable to send verification email. Please try again later.";
                return new BadRequestObjectResult(apiResponse);
            }

        }

        public class EmailVerificationRequestModel
        {
            public string Email { get; set; }
        }

        protected class EmailVerificationResponseModel : ApiResponseViewModel
        {
            public RequestEmailVerificationData Data { get; set; }
        }

        public class RequestEmailVerificationData
        {
            public string Message { get; set; }
        }
    }
}

