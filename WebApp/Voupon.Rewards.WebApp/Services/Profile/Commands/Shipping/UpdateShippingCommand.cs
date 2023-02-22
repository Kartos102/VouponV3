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
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Profile.Commands.Update
{
    public class UpdateShippingCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public System.DateTime CreatedAt { get; set; }
        public int AvailablePoints { get; set; }
        [Required]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        [Required]
        public string Postcode { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        public Nullable<short> CountryId { get; set; }
        [Required]
        public string MobileCountryCode { get; set; }
        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }


        public class LoginResponseViewModel
        {
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public string Token { get; set; }
            public string PreferredLanguage { get; set; }
            public int Points { get; set; }
            public string RedirectUrl { get; set; }
        }

        public class UpdateShippingCommandHandler : IRequestHandler<UpdateShippingCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly RewardsDBContext rewardsDBContext;
            private readonly IOptions<AppSettings> appSettings;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public UpdateShippingCommandHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
                this.appSettings = appSettings;
            }

            public async Task<ApiResponseViewModel> Handle(UpdateShippingCommand request, CancellationToken cancellationToken)
            {

                var apiResponseViewModel = new ApiResponseViewModel();
                var loginResponseViewModel = new LoginResponseViewModel();

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
                    user.FirstName = request.FirstName;
                    user.LastName = request.LastName;
                    user.Email = request.Email;

                    vodusV2Context.Update(user);
                    await vodusV2Context.SaveChangesAsync();

                    var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterprofileId).FirstOrDefaultAsync();
                    if (master == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request";
                        return apiResponseViewModel;
                    }


                    var fullMobile = request.MobileCountryCode + request.MobileNumber;
                    var fullCurrentNumber = master.MobileCountryCode + master.MobileNumber;
            
                    if(fullMobile.Equals(fullCurrentNumber))
                    {
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Message = "Successfully updated profile";

                    }

                    else
                    {
                        master.MobileVerified = "N";
                        apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Message = "Successfully updated profile with different mobile number.";
                    }

                    master.AddressLine1 = request.AddressLine1;
                    master.AddressLine2 = request.AddressLine2;
                    master.City = request.City;
                    master.State = request.State;
                    
                    //master.MobileCountryCode = request.MobileCountryCode;
                    //master.MobileNumber = request.MobileNumber;

                    vodusV2Context.MasterMemberProfiles.Update(master);
                    await vodusV2Context.SaveChangesAsync();

                    return apiResponseViewModel;

                }
                catch (Exception ex)
                {
                    await new Logs
                    {
                        Description = ex.ToString(),
                        Email = request.Email,
                        JsonData = JsonConvert.SerializeObject(request),
                        ActionName = "UpdateShippingCommand",
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
