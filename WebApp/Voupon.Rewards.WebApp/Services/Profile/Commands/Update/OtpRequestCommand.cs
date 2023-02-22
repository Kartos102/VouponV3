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
    public class OtpRequestCommand : IRequest<ApiResponseViewModel>
    {

        public string MobileCountryCode { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string Email { get; set; }



        public class OtpRequestCommandHandler : IRequestHandler<OtpRequestCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;
            private readonly ISMSS360 smss360;


            public OtpRequestCommandHandler(ISMSS360 smss360, VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
                this.smss360 = smss360;
            }



            public async Task<ApiResponseViewModel> Handle(OtpRequestCommand request, CancellationToken cancellationToken)
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
                    if (master == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request";
                        return apiResponseViewModel;
                    }

                    //Back end validation with last generated time to check 30 seconds
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

                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Please wait 30 seconds.";
                            return apiResponseViewModel;
                        };
                    }


                    if (master.MobileNumber.Equals(request.MobileNumber) && master.MobileVerified.Equals("Y"))
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "This mobile number is already verified";
                        return apiResponseViewModel;
                    }


                    //New Requested Validation - Number can be associated with one profile only 
                    var mobileData = await vodusV2Context.MasterMemberProfiles.Include(x => x.User)
                                                                           .Where(x => x.MobileNumber == request.MobileNumber && x.MobileCountryCode == request.MobileCountryCode && x.User.Email != request.Email && x.MobileVerified  == "Y")
                                                                           .FirstOrDefaultAsync();

                    if (mobileData != null)
                    {

                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "This mobile number has already been used for another Vodus account. Please use another mobile number";
                        return apiResponseViewModel;
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


                    if (string.IsNullOrEmpty(request.MobileCountryCode))
                    {
                        apiResponseViewModel.Message = "Please Provide Country Code";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }

                    if (string.IsNullOrEmpty(request.MobileNumber))
                    {
                        apiResponseViewModel.Message = "Please Provide Mobile Number";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }



                    if (request.MobileCountryCode.Trim() != "60")
                    {
                        apiResponseViewModel.Message = "Only Malaysian Mobile Numbers allowed";
                        apiResponseViewModel.Successful = false;
                        return apiResponseViewModel;
                    }


                    master.LastGenDateTime = currentTimeSvr;


                    vodusV2Context.MasterMemberProfiles.Update(master);
                    var result = await vodusV2Context.SaveChangesAsync();



                    if (result > 0)
                    {

                        var message = string.Format(SMSS360.Templates.Template1, OTP);

                        var SmsNumber = "+" + request.MobileCountryCode + request.MobileNumber;

                        var smsResult = await smss360.SendMessage(SmsNumber, message, "xxxx");
                        if (smsResult != "0" && smsResult != "1606")
                        {
                            //  sms sending failed
                            apiResponseViewModel.Message = "Something wasnt right. Please try again later: " + smsResult;
                            apiResponseViewModel.Successful = false;
                            return apiResponseViewModel;
                        }

                        apiResponseViewModel.Successful = true;
                    }
                    else
                    {
                        apiResponseViewModel.Message = "Something wasnt right. Please try again later";
                        apiResponseViewModel.Successful = false;
                    }
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
