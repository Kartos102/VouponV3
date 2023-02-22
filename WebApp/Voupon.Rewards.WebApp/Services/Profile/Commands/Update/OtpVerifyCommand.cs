using MediatR;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Common.SMS.SMSS360;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Profile.Commands.Update
{

    //Based on Update Shipping command
    public class OtpVerifyCommand : IRequest<ApiResponseViewModel>
    {

        public string MobileCountryCode { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string Email { get; set; }
        public string otpInput { get; set; }



        public class OtpVerifyCommandHandler : IRequestHandler<OtpVerifyCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;

            public OtpVerifyCommandHandler(ISMSS360 smss360, VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;

            }



            public async Task<ApiResponseViewModel> Handle(OtpVerifyCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {
                    //  Update user profile
                    var user = await vodusV2Context.Users.Include(x => x.MasterMemberProfiles).Where(x => x.Email == request.Email).FirstOrDefaultAsync();
                    if (user == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request";
                        return apiResponseViewModel;
                    }
                    var masterprofileId = user.MasterMemberProfiles.First().Id;
                    var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterprofileId).FirstOrDefaultAsync();

                    //Check OTP Validity  

                    if (request.MobileCountryCode.Trim() != "60")
                    {
                        apiResponseViewModel.Message = "Only Malaysian Mobile Numbers allowed";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    //New Requested Validation - Number can be associated with one profile only 
                    //New Requested Validation - Number can be associated with one profile only 
                    var mobileData = await vodusV2Context.MasterMemberProfiles.Include(x => x.User)
                                                                           .Where(x => x.MobileNumber == request.MobileNumber && x.MobileCountryCode == request.MobileCountryCode && x.User.Email != request.Email && x.MobileVerified == "Y")
                                                                           .FirstOrDefaultAsync();
                    if (mobileData != null)
                    {

                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "This mobile number has already been used for another Vodus account. Please use another mobile number";
                        return apiResponseViewModel;
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

                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "OTP Expired. Please request new OTP";
                            return apiResponseViewModel;
                        };
                    }


                    if (master == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request";
                        return apiResponseViewModel;
                    }

                    //Check User submitted OTP Request was 
                    if (request.otpInput.Trim().Equals(master.LastGenOtp))
                    {
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Message = "Verification success";
                        master.MobileVerified = "Y";
                        master.MobileCountryCode = request.MobileCountryCode;
                        master.MobileNumber = request.MobileNumber;
                    }

                    else
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "*Invalid OTP";
                        master.MobileVerified = "N";
                    }


                    vodusV2Context.MasterMemberProfiles.Update(master);
                    var result = await vodusV2Context.SaveChangesAsync();


                    return apiResponseViewModel;

                }
                catch (Exception ex)
                {
                    await new Logs
                    {
                        Description = ex.ToString(),
                        Email = request.Email,
                        JsonData = JsonConvert.SerializeObject(request),
                        ActionName = "OtpGenerateCommand",
                        TypeId = CreateErrorLogCommand.Type.Service,
                        SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                        RewardsDBContext = rewardsDBContext
                    }.Error();

                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Failed to update profile";
                    return apiResponseViewModel;
                }
            }
        }
    }

}
