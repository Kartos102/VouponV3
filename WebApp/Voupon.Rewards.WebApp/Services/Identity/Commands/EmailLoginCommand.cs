
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Infrastructures.Helpers;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Identity.Commands
{

    public class HttpDTO
    {
        public string UserAgent { get; set; }
        public string IpAddress { get; set; }
        public string Host { get; set; }
        public string Origin { get; set; }
        public string Referer { get; set; }
        public bool IsTest { get; set; } = false;

        public string DeviceType { get; set; }

        public bool IsMobile { get; set; }
    }

    public class EmailLoginCommand : IRequest<ApiResponseViewModel>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        public string PartnerCode { get; set; }

        public string PreferredLanguage { get; set; }

        public HttpDTO HttpDTO { get; set; }

        public string LoginProvider { get; set; }

        public string ThirdPartyToken { get; set; }

        public string DeviceId { get; set; }

        public bool RedirectToCheckOut { get; set; }

        public string TempToken { get; set; }
        public string SyncToken { get; set; }

        public class LoginResponseViewModel
        {
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public string Token { get; set; }
            public string PreferredLanguage { get; set; }
            public int Points { get; set; }
            public string RedirectUrl { get; set; }
        }

        public class EmailLoginCommandHandler : IRequestHandler<EmailLoginCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            private readonly VodusV2Context vodusV2Context;
            private readonly UserManager<Voupon.Database.Postgres.VodusEntities.Users> userManager;
            private readonly IOptions<AppSettings> appSettings;
            private readonly SignInManager<Voupon.Database.Postgres.VodusEntities.Users> signInManager;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public EmailLoginCommandHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, UserManager<Voupon.Database.Postgres.VodusEntities.Users> userManager, SignInManager<Voupon.Database.Postgres.VodusEntities.Users> signInManager, RewardsDBContext rewardsDBContext)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(EmailLoginCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var isFingerPrintUpdated = false;
                long existingMemberProfileId = 0;

                var fbId = "";
                var fbEmail = "";
                var fbName = "";
                try
                {
                    if (request.LoginProvider == "facebook")
                    {
                        var httpClient = new HttpClient();
                        var fbResult = await httpClient.GetAsync($"https://graph.facebook.com/v13.0/me?access_token={request.ThirdPartyToken}&fields=id,name,email");
                        if (fbResult.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Invalid request, please try again [001]";
                            return apiResponseViewModel;
                        }

                        dynamic fbData = JsonConvert.DeserializeObject(await fbResult.Content.ReadAsStringAsync());
                        fbId = fbData.id;
                        fbEmail = fbData.email;
                        fbName = fbData.name;

                        if (string.IsNullOrEmpty(fbId) || string.IsNullOrEmpty(fbEmail) || string.IsNullOrEmpty(fbName))
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Fail to login with Facebook. Please try again later or contact admin. Make sure your Facebook acocunt is verified";
                            return apiResponseViewModel;
                        }
                    }
                    else if (request.LoginProvider == "google")
                    {
                        var validPayload = await GoogleJsonWebSignature.ValidateAsync(request.ThirdPartyToken);
                        request.Email = validPayload.Email;
                        /*
                        if (validPayload.Email != request.Email)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Invalid Google login [001]";
                            return apiResponseViewModel;
                        }
                        */

                    }

                    //banned user list Backlog - 1756
                    var bannedUser = await this.vodusV2Context.BannedUsers.AnyAsync(m => m.Email == request.Email);
                    if (bannedUser)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Login failed. This account is suspended";
                        return apiResponseViewModel;
                    }

                    //throw new Exception("Some test");
                    if (string.IsNullOrEmpty(request.Email))
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Email is required";
                        return apiResponseViewModel;
                    }

                    if (request.LoginProvider == "email")
                    {
                        if (string.IsNullOrEmpty(request.Password))
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Password is required";
                            return apiResponseViewModel;
                        }
                    }


                    PartnerWebsites partnerWebsite = null;
                    var loginResponseViewModel = new LoginResponseViewModel();
                    MemberProfiles memberProfileObject = null;
                    var device = new Devices();

                    var masterMemberProfiles = new MasterMemberProfiles();
                    var userTokenObject = UserToken.FromTokenValue(request.Token);

                    //  Check email exist
                    var user = await userManager.FindByEmailAsync(request.Email);

                    if (user != null)
                    {
                        if (string.IsNullOrEmpty(request.LoginProvider))
                        {
                            request.LoginProvider = "Email";
                        }

                        if (request.LoginProvider.ToUpper() == "FaceBook".ToUpper() || request.LoginProvider.ToUpper() == "Google".ToUpper())
                        {

                        }
                        else
                        {
                            var passwordResult = userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                            if (passwordResult == PasswordVerificationResult.Success || passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
                            {
                                //   Passed. Rehash?

                            }
                            else
                            {
                                apiResponseViewModel.Successful = false;
                                apiResponseViewModel.Message = "Invalid email/password";
                                return apiResponseViewModel;
                            }
                        }
                    }
                    else
                    {
                        if (request.LoginProvider.ToLower() == "email")
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Invalid email/password";
                            return apiResponseViewModel;
                        }

                        //  No account exist for thirdparty login facebook/google
                        //  Create a new account
                        if (request.LoginProvider.ToLower() == "google")
                        {
                            //  Verify google token
                            try
                            {

                                var newUser = new Voupon.Database.Postgres.VodusEntities.Users()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ActivationCode = Guid.NewGuid().ToString(),
                                    UserName = request.Email,
                                    Email = request.Email,
                                    NormalizedEmail = request.Email.Normalize(),
                                    NormalizedUserName = request.Email.Normalize(),
                                    EmailConfirmed = true,
                                    PhoneNumberConfirmed = false,
                                    CreatedAt = DateTime.UtcNow
                                };

                                var randomString = Guid.NewGuid().ToString("n").Substring(0, 8);
                                var cre = await userManager.CreateAsync(newUser, "Vodus1" + randomString + "#");
                                if (!cre.Succeeded)
                                {
                                    apiResponseViewModel.Successful = false;
                                    apiResponseViewModel.Message = string.Join(" , ", cre.Errors.Select(x => x.Description));
                                    return apiResponseViewModel;
                                }

                                await userManager.AddToRoleAsync(newUser, "Member");
                                user = newUser;
                                var masterMemberProfileDTO = new MasterMemberProfiles();
                                masterMemberProfileDTO.UserId = newUser.Id;
                                masterMemberProfileDTO.CreatedAt = DateTime.UtcNow;
                                //masterMemberProfileDTO.MemberProfiles = new List<MemberProfiles>();
                                masterMemberProfileDTO.PreferLanguage = "en";

                                /*
                                masterMemberProfileDTO.MemberProfiles.Add(new MemberProfiles
                                {
                                    Guid = Guid.NewGuid().ToString(),
                                    IsMasterProfile = true,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedAtCountryCode = "MY",
                                });
                                */

                                vodusV2Context.MasterMemberProfiles.Add(masterMemberProfileDTO);
                                await vodusV2Context.SaveChangesAsync();

                                //  Create member from token if available

                                if (userTokenObject != null)
                                {
                                    var memberProfile = await vodusV2Context.MemberProfiles.Where(x => x.Id == userTokenObject.MemberProfileId).FirstOrDefaultAsync();

                                    if (memberProfile != null)
                                    {

                                        memberProfile.MasterMemberProfileId = masterMemberProfileDTO.Id;
                                        vodusV2Context.MemberProfiles.Update(memberProfile);

                                        var masterToUpdate = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterMemberProfileDTO.Id).FirstOrDefaultAsync();
                                        if (masterToUpdate != null)
                                        {
                                            masterToUpdate.MemberProfileId = memberProfile.Id;
                                        }
                                        vodusV2Context.MasterMemberProfiles.Update(masterToUpdate);
                                        await vodusV2Context.SaveChangesAsync();

                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                apiResponseViewModel.Successful = false;
                                apiResponseViewModel.Message = "Invalid Google login [002]";
                                return apiResponseViewModel;
                            }
                        }
                        else if (request.LoginProvider.ToLower() == "facebook")
                        {
                            //  Verify facebook token
                            try
                            {

                                var newUser = new Voupon.Database.Postgres.VodusEntities.Users()
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    ActivationCode = Guid.NewGuid().ToString(),
                                    UserName = request.Email,
                                    Email = request.Email,
                                    NormalizedEmail = request.Email.Normalize(),
                                    NormalizedUserName = request.Email.Normalize(),
                                    EmailConfirmed = true,
                                    PhoneNumberConfirmed = false,
                                    CreatedAt = DateTime.UtcNow,
                                    FirstName = fbName
                                };

                                var randomString = Guid.NewGuid().ToString("n").Substring(0, 8);
                                var cre = await userManager.CreateAsync(newUser, "Vodus1" + randomString + "#");
                                if (!cre.Succeeded)
                                {
                                    apiResponseViewModel.Successful = false;
                                    apiResponseViewModel.Message = string.Join(" , ", cre.Errors.Select(x => x.Description));
                                    return apiResponseViewModel;
                                }

                                await userManager.AddToRoleAsync(newUser, "Member");
                                user = newUser;
                                var masterMemberProfileDTO = new MasterMemberProfiles();
                                masterMemberProfileDTO.UserId = newUser.Id;
                                masterMemberProfileDTO.CreatedAt = DateTime.UtcNow;
                                //masterMemberProfileDTO.MemberProfiles = new List<MemberProfiles>();
                                masterMemberProfileDTO.PreferLanguage = "en";

                                /*
                                masterMemberProfileDTO.MemberProfiles.Add(new MemberProfiles
                                {
                                    Guid = Guid.NewGuid().ToString(),
                                    IsMasterProfile = true,
                                    CreatedAt = DateTime.UtcNow,
                                    CreatedAtCountryCode = "MY",
                                });
                                */

                                vodusV2Context.MasterMemberProfiles.Add(masterMemberProfileDTO);
                                await vodusV2Context.SaveChangesAsync();

                                //  Create member from token if available

                                if (userTokenObject != null)
                                {
                                    var memberProfile = await vodusV2Context.MemberProfiles.Where(x => x.Id == userTokenObject.MemberProfileId).FirstOrDefaultAsync();

                                    if (memberProfile != null)
                                    {

                                        memberProfile.MasterMemberProfileId = masterMemberProfileDTO.Id;
                                        vodusV2Context.MemberProfiles.Update(memberProfile);

                                        var masterToUpdate = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterMemberProfileDTO.Id).FirstOrDefaultAsync();
                                        if (masterToUpdate != null)
                                        {
                                            masterToUpdate.MemberProfileId = memberProfile.Id;
                                        }
                                        vodusV2Context.MasterMemberProfiles.Update(masterToUpdate);
                                        await vodusV2Context.SaveChangesAsync();

                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                apiResponseViewModel.Successful = false;
                                apiResponseViewModel.Message = "Invalid Facebook login [002]";
                                return apiResponseViewModel;
                            }
                        }
                    }

                    if (request.TempToken == "null")
                    {
                        request.TempToken = null;
                    }

                    //  Check if user exist in master
                    var userDTO = vodusV2Context.Users.Include(x => x.UserLogins).Include(x => x.MasterMemberProfiles).Where(x => x.Email == request.Email).FirstOrDefault();
                    if (userDTO != null)
                    {
                        if (userDTO.UserLogins.Where(x => x.LoginProvider.ToLower() == "email").FirstOrDefault() == null)
                        {
                            //apiResponse.Successful = false;
                            //apiResponse.Message = "Incorrect login details";
                            //return Ok(apiResponse);
                        }

                        if (!userDTO.MasterMemberProfiles.Any())
                        {
                            var masterMemberDTO = new MasterMemberProfiles();
                            masterMemberDTO.CreatedAt = DateTime.UtcNow;
                            masterMemberDTO.UserId = userDTO.Id;
                            masterMemberDTO.AvailablePoints = 0;

                            await vodusV2Context.AddAsync(masterMemberDTO);
                            await vodusV2Context.SaveChangesAsync();

                            masterMemberProfiles = masterMemberDTO;
                        }
                        else
                        {
                            masterMemberProfiles = userDTO.MasterMemberProfiles.First();
                        }



                        if (!string.IsNullOrEmpty(request.DeviceId))
                        {
                            device = await vodusV2Context.Devices.Where(x => x.Id == new Guid(request.DeviceId)).FirstOrDefaultAsync();
                            if (device == null)
                            {
                                apiResponseViewModel.Successful = false;
                                apiResponseViewModel.Message = "Invalid device login [001]";
                                return apiResponseViewModel;
                            }

                            if (device.MemberProfileId != 0 && masterMemberProfiles.Id != 0)
                            {
                                var existingMemberProfile = await vodusV2Context.MemberProfiles.Where(x => x.Id == device.MemberProfileId).FirstOrDefaultAsync();
                                existingMemberProfile.MasterMemberProfileId = masterMemberProfiles.Id;
                                existingMemberProfileId = device.MemberProfileId;
                                isFingerPrintUpdated = true;
                                vodusV2Context.MemberProfiles.Update(existingMemberProfile);
                                await vodusV2Context.SaveChangesAsync();
                            }
                        }

                        //  Update last logged in
                        //await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update users set lastloggedinat=GETDATE() where Id={0}", userDTO.Id));

                        if (userTokenObject != null)
                        {
                            var profile = await vodusV2Context.MemberProfiles.Where(x => x.Id == userTokenObject.MemberProfileId).FirstOrDefaultAsync();
                            if (profile != null)
                            {
                                profile.MasterMemberProfileId = masterMemberProfiles.Id;
                                vodusV2Context.MemberProfiles.Update(profile);
                                await vodusV2Context.SaveChangesAsync();
                            }

                        }


                        var memberProfileList = new List<MemberProfiles>();

                        //  Reset member master id 
                        //var memberProfileList = await vodusV2Context.MemberProfiles.FromSqlRaw(string.Format("Select top 1 * from memberprofiles WITH (NOLOCK) where mastermemberprofileid={0}", masterMemberProfiles.Id)).ToListAsync();
                        //var memberProfileList = await vodusV2Context.MemberProfiles.FromSqlRaw(string.Format("Select * from memberprofiles WITH (NOLOCK) where mastermemberprofileid={0}", masterMemberProfiles.Id)).ToListAsync();
                        /*
                        if (masterMemberProfiles.MemberProfileId.HasValue)
                        {
                            memberProfileList = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == masterMemberProfiles.MemberProfileId.Value).Select(x => new MemberProfiles
                            {
                                Id = x.Id,
                                AvailablePoints = x.AvailablePoints,
                                DemographicPoints = x.DemographicPoints
                            }).ToListAsync();
                        }
                        else
                        {
                            memberProfileList = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.MasterMemberProfileId == masterMemberProfiles.Id).Select(x => new MemberProfiles
                            {
                                Id = x.Id,
                                AvailablePoints = x.AvailablePoints,
                                DemographicPoints = x.DemographicPoints
                            }).ToListAsync();
                        }
                        */

                        memberProfileList = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.MasterMemberProfileId == masterMemberProfiles.Id).Select(x => new MemberProfiles
                        {
                            Id = x.Id,
                            AvailablePoints = x.AvailablePoints,
                            DemographicPoints = x.DemographicPoints
                        }).ToListAsync();


                        if (memberProfileList == null || memberProfileList.Count == 0)
                        {
                            var memberProfileDTO = new MemberProfiles()
                            {
                                Guid = Guid.NewGuid().ToString(),
                                CreatedAt = DateTime.Now,
                                CreatedAtCountryCode = "my",
                                IsMasterProfile = true
                            };

                            vodusV2Context.MemberProfiles.Add(memberProfileDTO);
                            await vodusV2Context.SaveChangesAsync();

                            memberProfileList.Add(memberProfileDTO);

                        }

                        //  Add fingerprint member to the list,new member might have more points 
                        /*
                        var newMemberProfileList = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == existingMemberProfileId).Select(x => new MemberProfiles
                        {
                            Id = x.Id,
                            AvailablePoints = x.AvailablePoints,
                            DemographicPoints = x.DemographicPoints
                        }).ToListAsync();

                        if (newMemberProfileList != null && newMemberProfileList.Any())
                        {
                            memberProfileList.AddRange(newMemberProfileList);
                        }
                        */

                        var idList = memberProfileList.Select(x => x.Id).ToList();
                        var responses = await vodusV2Context.SurveyResponses.Where(x => idList.Contains(x.MemberProfileId)).ToListAsync();
                        var responseGroup = responses.GroupBy(x => x.MemberProfileId).Select(group =>
                                           new
                                           {
                                               Id = group.Key,
                                               Count = group.Count(),
                                               Points = group.Sum(x => x.PointsCollected)
                                           }).OrderByDescending(x => x.Points).ToList();

                        //var orders = await vodusV2Context.Orders.Where(x => x.MasterMemberId == masterMemberProfiles.Id).ToListAsync();
                        var ordersFromRewards = await rewardsDBContext.Orders.Where(x => x.OrderStatus == 2 && x.MasterMemberProfileId == masterMemberProfiles.Id).ToListAsync();
                        
                        var usedPoints = 0;
                        var bonusPoints = 0;
                        var refundedPoints = 0;
                        var psychographicpoints = 0;
                        var deletedPsychographicpoints = 0;
                        var deletedResponsesPoints = 0;
                        
                        
                        //Survey points calculation 
                        var SurveyResponses = 0;

                        SurveyResponses += await vodusV2Context.SurveyResponses.Where(x => x.MemberProfileId == masterMemberProfiles.MemberProfileId).SumAsync(x => x.PointsCollected);

                        //var refunds = await rewardsDBContext.Refunds.Where(x => x.Status == 2 && x.MasterMemberProfileId == masterMemberProfiles.Id).ToListAsync();

                        //Caputure Internal items refunds total points 
                        var refunds = await rewardsDBContext.OrderItems
                        .Include(x => x.Order)
                        .Where(x => x.Status == 5 && x.Order.MasterMemberProfileId == masterMemberProfiles.Id)
                        .AsNoTracking()
                        .ToListAsync();

                        if (refunds != null && refunds.Any())
                        {
                            refundedPoints = refunds.Sum(x => x.Points);
                        }

                        //  External refunds
                        var externalRefunds = await rewardsDBContext.RefundsExternalOrderItems.Where(x => x.MasterMemberProfileId == masterMemberProfiles.Id).AsNoTracking().ToListAsync();
                        if (externalRefunds != null && externalRefunds.Any())
                        {
                            refundedPoints += externalRefunds.Where(x => x.Status == 2).Sum(x => x.PointsRefunded);
                        }


                        var bonusPointsList = await vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == masterMemberProfiles.Id && x.IsActive == true).ToListAsync();

                        if (bonusPointsList.Any())
                        {
                            bonusPoints = bonusPointsList.Sum(x => x.Points);
                        }

                        //if (orders != null && orders.Any())
                        //{
                        //    usedPoints = orders.Sum(x => x.TotalPoints);
                        //}

                        if (ordersFromRewards != null && ordersFromRewards.Any())
                        {
                            usedPoints += ordersFromRewards.Sum(x => x.TotalPoints);
                        }

                        psychographicpoints = await vodusV2Context.MemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfiles.MemberProfileId).CountAsync();
                        deletedPsychographicpoints = await vodusV2Context.DeletedMemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfiles.MemberProfileId).CountAsync();

                        deletedResponsesPoints = await vodusV2Context.DeletedSurveyResponses.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfiles.MemberProfileId).SumAsync(x => x.PointsCollected);

                        if (responseGroup != null && responseGroup.Any())
                        {
                            var item = responseGroup.First();

                            var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).Select(x => new MemberProfiles
                            {
                                Id = x.Id,
                                AvailablePoints = item.Points,
                                DemographicPoints = x.DemographicPoints
                            }).FirstOrDefaultAsync();

                            //var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints + refundedPoints + psychographicpoints + deletedResponsesPoints + deletedPsychographicpoints) - usedPoints;
                            var availablePoints = (SurveyResponses + memberEntity.DemographicPoints + bonusPoints + refundedPoints + psychographicpoints + deletedResponsesPoints + deletedPsychographicpoints) - usedPoints;


                            await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set IsMasterProfile=1,MasterMemberProfileId={1},AvailablePoints={2} where Id={0}", item.Id, masterMemberProfiles.Id, memberEntity.AvailablePoints));
                            await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MasterMemberProfiles set AvailablePoints={0}, MemberProfileid={2} where Id={1}", availablePoints, masterMemberProfiles.Id, item.Id));

                            memberProfileObject = memberEntity;
                            loginResponseViewModel.Points = availablePoints;
                        }
                        else
                        {
                            var memberListSorted = memberProfileList.OrderByDescending(x => x.AvailablePoints + x.DemographicPoints);
                            if (memberListSorted != null && memberListSorted.Any())
                            {
                                var item = memberListSorted.First();

                                var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).Select(x => new MemberProfiles
                                {
                                    Id = x.Id,
                                    AvailablePoints = item.AvailablePoints,
                                    DemographicPoints = x.DemographicPoints
                                }).FirstOrDefaultAsync();

                                var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints + refundedPoints + psychographicpoints + deletedResponsesPoints) - usedPoints;

                                await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set IsMasterProfile=1,MasterMemberProfileId={1} where Id={0}", item.Id, masterMemberProfiles.Id));
                                await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MasterMemberProfiles set AvailablePoints={0}, MemberProfileId={2} where Id={1}", availablePoints, masterMemberProfiles.Id, item.Id));

                                loginResponseViewModel.Points = availablePoints;
                                memberProfileObject = memberEntity;
                            }
                            else
                            {
                                memberProfileObject = memberProfileList.First();
                            }

                        }

                        foreach (var member in memberProfileList)
                        {
                            if (member.Id == memberProfileObject.Id)
                            {
                                continue;
                            }
                            vodusV2Context.Database.ExecuteSqlRaw(string.Format("Update MemberProfiles set SyncMemberProfileId={0} where Id={1}", memberProfileObject.Id, member.Id));
                        }

                        //  Update others to non master
                        /*
                        foreach (var member in memberProfileList)
                        {
                            if (member.Id == memberProfileObject.Id)
                            {
                                continue;
                            }
                            vodusV2Context.Database.ExecuteSqlRaw(string.Format("Update MemberProfiles set IsMasterProfile=0,MasterMemberProfileId=null where Id={0}", member.Id));
                        }
                        */
                        await vodusV2Context.SaveChangesAsync();

                        var userToken = new UserToken
                        {
                            MemberProfileId = memberProfileObject.Id,
                            Email = userDTO.Email,
                            MemberMasterId = masterMemberProfiles.Id,
                            CreatedAt = DateTime.Now
                        };

                        if (!string.IsNullOrEmpty(request.DeviceId))
                        {
                            device = await vodusV2Context.Devices.Where(x => x.Id == new Guid(request.DeviceId)).FirstOrDefaultAsync();
                            device.MemberProfileId = memberProfileObject.Id;
                            vodusV2Context.Devices.Update(device);

                            var fingerPrints = await vodusV2Context.Fingerprints.FromSqlRaw($"Select * from Fingerprints WITH (NOLOCK) where DeviceId='{device.Id}'").ToListAsync();
                            if (fingerPrints != null)
                            {
                                foreach (var item in fingerPrints)
                                {
                                    await vodusV2Context.Database.ExecuteSqlRawAsync($"Update Fingerprints set MemberProfileId={memberProfileObject.Id}, LastUpdatedAt=GETDATE() where Id='{item.Id}'");

                                }

                                var devices = fingerPrints.Select(x => x.DeviceId).ToList();
                                foreach (var item in devices)
                                {
                                    await vodusV2Context.Database.ExecuteSqlRawAsync($"Update Devices set MemberProfileId={memberProfileObject.Id}, LastUpdatedAt=GETDATE() where Id='{item}'");
                                }
                            }
                            await vodusV2Context.SaveChangesAsync();
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(request.TempToken) && string.IsNullOrEmpty(request.SyncToken))
                            {

                            }
                            else
                            {
                                var requiredUpdate = false;
                                var generatedToken = userToken.ToTokenValue();
                                if (!string.IsNullOrEmpty(request.TempToken))
                                {
                                    await vodusV2Context.Database.ExecuteSqlRawAsync($"Update TempTokens set MemberProfileId={memberProfileObject.Id},Token='{generatedToken}', LastUpdatedAt=GETDATE() where Id='{request.TempToken}'");
                                    requiredUpdate = true;
                                }
                                if (!string.IsNullOrEmpty(request.SyncToken))
                                {
                                    var syncTokenObject = UserToken.FromTokenValue(request.SyncToken);
                                    if (syncTokenObject != null)
                                    {
                                        await vodusV2Context.Database.ExecuteSqlRawAsync($"Update MemberProfiles set SyncMemberProfileId={memberProfileObject.Id},SyncMemberProfileToken='{generatedToken}' where Id={syncTokenObject.MemberProfileId}");
                                        requiredUpdate = true;
                                    }
                                }
                                if (requiredUpdate)
                                {
                                    await vodusV2Context.SaveChangesAsync();
                                }
                            }

                            if (userTokenObject != null)
                            {
                                if (memberProfileObject.Id == userTokenObject.MemberProfileId)
                                {
                                    loginResponseViewModel.PreferredLanguage = request.PreferredLanguage;
                                }
                                else
                                {
                                    loginResponseViewModel.PreferredLanguage = masterMemberProfiles.PreferLanguage;
                                }
                            }
                        }



                        await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("MasterMemberProfileId", masterMemberProfiles.Id.ToString()));


                        if (!string.IsNullOrEmpty(request.ThirdPartyToken))
                        {
                            await signInManager.SignInAsync(user, true);
                            loginResponseViewModel.MasterMemberProfileId = masterMemberProfiles.Id;
                            loginResponseViewModel.Email = userDTO.Email;
                            loginResponseViewModel.Token = userToken.ToTokenValue();
                            loginResponseViewModel.Points = masterMemberProfiles.AvailablePoints;
                            apiResponseViewModel.Successful = true;
                            apiResponseViewModel.Data = loginResponseViewModel;
                        }
                        else
                        {
                            var result = await signInManager.PasswordSignInAsync(user.NormalizedUserName,
                    request.Password, true, lockoutOnFailure: false);
                            if (!result.Succeeded)
                            {
                                apiResponseViewModel.Successful = false;
                                apiResponseViewModel.Message = "Invalid email/password [001]";
                                return apiResponseViewModel;
                            }

                            loginResponseViewModel.MasterMemberProfileId = masterMemberProfiles.Id;
                            loginResponseViewModel.Email = userDTO.Email;
                            loginResponseViewModel.Token = userToken.ToTokenValue();
                            loginResponseViewModel.Points = masterMemberProfiles.AvailablePoints;
                            apiResponseViewModel.Successful = true;
                            apiResponseViewModel.Data = loginResponseViewModel;
                        }


                        return apiResponseViewModel;
                    }
                    else
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Incorrect login details";
                        return apiResponseViewModel;
                    }

                }
                catch (Exception ex)
                {
                    await new Logs
                    {
                        Description = ex.ToString(),
                        Email = request.Email,
                        JsonData = JsonConvert.SerializeObject(request),
                        ActionName = "EmailLoginCommand",
                        TypeId = CreateErrorLogCommand.Type.Service,
                        SendgridAPIKey = appSettings.Value.Mailer.Sendgrid.APIKey,
                        RewardsDBContext = rewardsDBContext
                    }.Error();

                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Something is not right...";
                    return apiResponseViewModel;
                }
            }


        }
    }

}
