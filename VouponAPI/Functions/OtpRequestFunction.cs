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
using Voupon.Common.SMS.SMSS360;

namespace Voupon.API.Functions
{
    public class OtpRequestFunction
    {


        private readonly VodusV2Context vodusV2Context;
        private readonly ISMSS360 smss360;

        public OtpRequestFunction(ISMSS360 smss360,
            VodusV2Context vodusV2Context)

        {
            this.vodusV2Context = vodusV2Context;
            this.smss360 = smss360;
        }



        [OpenApiOperation(operationId: "Request OTP", tags: new[] { "OTP" }, Description = "Request OTP", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "MobileCountryCode", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Country Code", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "MobileNumber", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "Country Code", Visibility = OpenApiVisibilityType.Important)]


        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Summary = "OTP Request")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "If JWT token provided")]

        [FunctionName("GetOtpRequestFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "otp/request")] HttpRequest req, ILogger log)
        {

            var response = new OtpResponseModal
            {
                Data = new OtpResponse()
            };

            var auth = new Authentication(req);
            if (!auth.IsValid)
            {
                response.RequireLogin = true;
                response.ErrorMessage = "Invalid token provided. Please re-login first.";
                return new BadRequestObjectResult(response);
            }

            string MobileCountryCode = req.Query["MobileCountryCode"];
            string MobileNumber = req.Query["MobileNumber"];




            if (MobileCountryCode.Equals(null) || MobileCountryCode.Equals(""))
            {
                response.ErrorMessage = "Please Provide Mobile Country Code";
                return new BadRequestObjectResult(response);
            }

            if (MobileNumber.Equals(null) || MobileNumber.Equals(""))
            {
                response.ErrorMessage = "Please Provide Mobile Number";
                return new BadRequestObjectResult(response);
            }

            if (MobileCountryCode.Trim() != "60")
            {

                response.ErrorMessage = "Only Malaysian Mobile Numbers allowed";
                return new BadRequestObjectResult(response);
            }

            //New Requested Validation - Number can be associated with one profile only 
            var mobileData = await vodusV2Context.MasterMemberProfiles.Include(x => x.User)
                                                                   .Where(x => x.MobileNumber == MobileNumber && x.MobileCountryCode == MobileCountryCode && x.User.Email != auth.Email && x.MobileVerified == "Y")
                                                                   .FirstOrDefaultAsync();

            if (mobileData != null)
            {
                response.ErrorMessage = "This mobile number has already been used for another Vodus account. Please use another mobile number";
                return new BadRequestObjectResult(response);
            }


            // OTP Request Operation

            //Get user details
            var user = await vodusV2Context.Users.Include(x => x.MasterMemberProfiles).Where(x => x.Email == auth.Email).FirstOrDefaultAsync();

            if (user == null)
            {
                response.ErrorMessage = "Invalid Request";
                return new BadRequestObjectResult(response);
            }

            var masterprofileId = user.MasterMemberProfiles.First().Id;
            var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterprofileId).FirstOrDefaultAsync();
            if (master == null)
            {
                response.ErrorMessage = "Invalid Request";
                return new BadRequestObjectResult(response);
            }


            if (master.LastGenDateTime != null)
            {

                DateTime currentTime = DateTime.Now;
                TimeZoneInfo localZone = TimeZoneInfo.Local;

                if (localZone.Id != "Singapore Standard Time")
                {
                    TimeZoneInfo tzi01 = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                    currentTime = TimeZoneInfo.ConvertTime(currentTime, tzi01);
                }

                double diffInSeconds = currentTime.Subtract(master.LastGenDateTime.Value).TotalSeconds;

                if (diffInSeconds <= 30)
                {

                    response.ErrorMessage = "Please wait 30 seconds.";
                    return new BadRequestObjectResult(response);
                };
            }


            if (master.MobileNumber.Equals(MobileNumber) && master.MobileVerified.Equals("Y"))
            {

                response.ErrorMessage = "This mobile number is already verified.";
                return new BadRequestObjectResult(response);
            }


            Random r = new Random();
            int randNum = r.Next(1000000);
            string OTP = randNum.ToString("D6");
            master.LastGenOtp = OTP;

            //Setting Malasia Time As the last gen time

            DateTime currentTimeSvr = DateTime.Now;
            TimeZoneInfo localZoneSvr = TimeZoneInfo.Local;

            if (localZoneSvr.Id != "Singapore Standard Time")
            {
                TimeZoneInfo tzi02 = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
                currentTimeSvr = TimeZoneInfo.ConvertTime(currentTimeSvr, tzi02);
            }



            master.LastGenDateTime = currentTimeSvr;


            vodusV2Context.MasterMemberProfiles.Update(master);
            var result = await vodusV2Context.SaveChangesAsync();

            if (result > 0)
            {

                var message = string.Format(SMSS360.Templates.Template1, OTP);
                var SmsNumber = "+" + MobileCountryCode + MobileNumber;

                var smsResult = await smss360.SendMessage(SmsNumber, message, "xxxx");
                if (smsResult != "0" && smsResult != "1606")
                {
                    response.ErrorMessage = "OTP Request Failed";
                    return new BadRequestObjectResult(response);

                }


                response.Data.messsage = "OTP Request Success.";
                return new OkObjectResult(response);
            }
            else
            {
                response.ErrorMessage = "OTP Request Failed";
                return new BadRequestObjectResult(response);


            }


        }


        protected class OtpResponseModal : ApiResponseViewModel
        {
            public OtpResponse Data { get; set; }
        }

        protected class OtpResponse
        {
            public string messsage { get; set; }
        }

    }
}
