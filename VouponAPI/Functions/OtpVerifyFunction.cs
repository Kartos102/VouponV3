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
    public class OtpVerifyFunction
    {

        private readonly VodusV2Context vodusV2Context;
        private readonly ISMSS360 smss360;

        public OtpVerifyFunction(ISMSS360 smss360, VodusV2Context vodusV2Context)
        {
            this.vodusV2Context = vodusV2Context;
            this.smss360 = smss360;
        }

        [OpenApiOperation(operationId: "Request OTP", tags: new[] { "OTP" }, Description = "Verify OTP", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(name: "Authorization", In = ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Bearer xxxx", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiRequestBody("application/json", typeof(otpVerifyRequestModal), Description = "JSON request body ")]



        [FunctionName("OtpVerifyFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "otp/verify")] HttpRequest req, ILogger log)
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



            var requestModel = HttpRequestHelper.DeserializeModel<otpVerifyRequestModal>(req);
            
            string MobileCountryCode = requestModel.MobileCountryCode;
            string MobileNumber = requestModel.MobileNumber;
            string otpInput = requestModel.otpInput;


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

            if (otpInput.Equals(null) || otpInput.Equals(""))
            {
                response.ErrorMessage = "OTP Value cannot be null";
                return new BadRequestObjectResult(response);
            }

            if (MobileCountryCode.Trim() != "60")
            {

                response.ErrorMessage = "Only Malaysian Mobile Numbers allowed";
                return new BadRequestObjectResult(response);
            }

            //New Requested Validation - Number can be associated with one profile only 

            //New Requested Validation - Number can be associated with one profile only 
            var mobileData = await vodusV2Context.MasterMemberProfiles.Include(x => x.User)
                                                                   .Where(x => x.MobileNumber == MobileNumber && x.MobileCountryCode == MobileCountryCode && x.User.Email != auth.Email && x.MobileVerified == "Y")
                                                                   .FirstOrDefaultAsync();


            if (mobileData != null)
            {
                response.ErrorMessage = "This mobile number has already been used for another Vodus account. Please use another mobile number";
                return new BadRequestObjectResult(response);
            }



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


                if (diffInSeconds >= 240)
                {

                    response.ErrorMessage = "OTP Expired. Please request new OTP";
                    return new BadRequestObjectResult(response);
                };
            }


            //Check User submitted OTP Request was 
            if (otpInput.Trim().Equals(master.LastGenOtp))
            {

                master.MobileVerified = "Y";
                master.MobileCountryCode = MobileCountryCode;
                master.MobileNumber = MobileNumber;

            }

            else
            {

                master.MobileVerified = "N";
                response.ErrorMessage = "Invalid OTP";
                return new BadRequestObjectResult(response);


            }



            vodusV2Context.MasterMemberProfiles.Update(master);
            var result = await vodusV2Context.SaveChangesAsync();

            response.Data.messsage = "Verification success";
            return new OkObjectResult(response);

        }

        protected class OtpResponseModal : ApiResponseViewModel
        {
            public OtpResponse Data { get; set; }
        }

        protected class OtpResponse
        {
            public string messsage { get; set; }
        }

        public class otpVerifyRequestModal
        {

            public string MobileCountryCode { get; set; }
            public string MobileNumber { get; set; }
            public string otpInput { get; set; }

        }

    }


}
