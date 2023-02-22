using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Identity.Commands.Create
{
    public class CreateAccountCommand : IRequest<ApiResponseViewModel>
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Token { get; set; }

        public string PartnerCode { get; set; }

        public string PreferredLanguage { get; set; }

        public HttpDTO HttpDTO { get; set; }

        public string LoginProvider { get; set; }

        public string ReqFrom { get; set; }

        public string DeviceId { get; set; }

        public Guid TempToken { get; set; }

        public string SyncToken { get; set; }


        public class CreateAccountResponseViewModel
        {
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public string Token { get; set; }
            public string PreferredLanguage { get; set; }
            public int Points { get; set; }
            public string RedirectUrl { get; set; }
        }


        public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, ApiResponseViewModel>
        {
            private readonly VodusV2Context vodusV2Context;
            private readonly UserManager<Users> userManager;
            private readonly IOptions<AppSettings> appSettings;
            private readonly SignInManager<Users> signInManager;

            private string GetRootDomain(string host)
            {
                var filterHost = host.Replace("http://", "").Replace("https://", "");
                return filterHost.Split('/')[0];
            }

            public CreateAccountCommandHandler(VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, UserManager<Users> userManager, SignInManager<Users> signInManager)
            {
                this.vodusV2Context = vodusV2Context;
                this.appSettings = appSettings;
                this.userManager = userManager;
                this.signInManager = signInManager;
            }

            public async Task<ApiResponseViewModel> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var createAccountResponseViewModel = new CreateAccountResponseViewModel();
                MemberProfiles memberProfileObject = null;


                try
                {
                    /*
                    if (string.IsNullOrEmpty(request.Token) && string.IsNullOrEmpty(request.DeviceId))
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid token/fingerprint";
                        apiResponseViewModel.Code = 2;
                        return apiResponseViewModel;
                    }
                    */

                    var user = await vodusV2Context.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Email already in use. Please login or use other email";
                        return apiResponseViewModel;
                    }

                    /*
                    if (!string.IsNullOrEmpty(request.DeviceId))
                    {
                        var device = await vodusV2Context.Devices.Where(x => x.Id == new Guid(request.DeviceId)).FirstOrDefaultAsync();
                        if (device == null)
                        {
                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Invalid fingerprint [001]";
                            return apiResponseViewModel;
                        }

                        if (device.MemberProfileId != 0)
                        {
                            memberProfileObject = await vodusV2Context.MemberProfiles.Where(x => x.Id == device.MemberProfileId).FirstOrDefaultAsync();
                        }
                    }
                    */

                    if(request.Token != null)
                    {
                        var userTokenObject = UserToken.FromTokenValue(request.Token,true);
                        if (userTokenObject != null)
                        {
                            memberProfileObject = await vodusV2Context.MemberProfiles.Where(x => x.Id == userTokenObject.MemberProfileId).FirstOrDefaultAsync();
                        }
                        else
                        {
                            var memberProfileDTO = new MemberProfiles()
                            {
                                Guid = Guid.NewGuid().ToString(),
                                CreatedAt = DateTime.Now,
                                CreatedAtCountryCode = "my",
                                CreatedAtPartnerWebsiteId = 0,
                                IsMasterProfile = false
                            };

                            vodusV2Context.MemberProfiles.Add(memberProfileDTO);
                            await vodusV2Context.SaveChangesAsync();

                            memberProfileObject = memberProfileDTO;
                        }
                    }
                    else
                    {
                        var memberProfileDTO = new MemberProfiles()
                        {
                            Guid = Guid.NewGuid().ToString(),
                            CreatedAt = DateTime.Now,
                            CreatedAtCountryCode = "my",
                            CreatedAtPartnerWebsiteId = 0,
                            IsMasterProfile = false
                        };

                        vodusV2Context.MemberProfiles.Add(memberProfileDTO);
                        await vodusV2Context.SaveChangesAsync();

                        memberProfileObject = memberProfileDTO;
                    }

                    if (request.TempToken != null)
                    {
                        var tempTokenObject = await vodusV2Context.TempTokens.Where(x => x.Id == request.TempToken).FirstOrDefaultAsync();
                        if(tempTokenObject != null)
                        {
                            if (tempTokenObject.MemberProfileId.HasValue)
                            {
                                if (tempTokenObject.MemberProfileId != 0)
                                {
                                    var tempMember = await vodusV2Context.MemberProfiles.Where(x => x.Id == tempTokenObject.MemberProfileId).FirstOrDefaultAsync();
                                    if(tempMember != null)
                                    {
                                        memberProfileObject = tempMember;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if(!string.IsNullOrEmpty(request.SyncToken))
                        {
                            var syncTokenObject =  UserToken.FromTokenValue(request.SyncToken);
                            if(syncTokenObject != null)
                            {
                                var tempMember = await vodusV2Context.MemberProfiles.Where(x => x.Id == syncTokenObject.MemberProfileId).FirstOrDefaultAsync();
                                if (tempMember != null)
                                {
                                    memberProfileObject = tempMember;
                                }
                            }
                        }
                    }

                    var newUser = new Users()
                    {
                        Id = Guid.NewGuid().ToString(),
                        ActivationCode = Guid.NewGuid().ToString(),
                        UserName = request.Email,
                        Email = request.Email,
                        NormalizedEmail = request.Email.Normalize(),
                        NormalizedUserName = request.Email.Normalize(),
                        EmailConfirmed = (string.IsNullOrEmpty(request.ReqFrom) ? false : (request.ReqFrom.ToLower() == "hut" ? true : false)),
                        PhoneNumberConfirmed = false,
                        CreatedAt = DateTime.Now
                    };

                    var cre = await userManager.CreateAsync(newUser, request.Password);
                    if (!cre.Succeeded)
                    {
                        apiResponseViewModel.Successful = false;
                        bool isPasswordError = false;
                        foreach(var error in cre.Errors)
                        {
                            //if (error.Code == "PasswordTooShort")
                            //{
                            //    error.Description = "one non alphanumeric characters (!, @, #, $, *)";
                            //    if (isPasswordError == false)
                            //        error.Description = "Password must have at least " + error.Description;
                            //    isPasswordError = true;
                            //}
                            if (error.Code == "PasswordRequiresNonAlphanumeric")
                            {
                                error.Description = "one non alphanumeric characters (!, @, #, $, *)";
                                if (isPasswordError == false)
                                    error.Description = "Password must have at least " + error.Description;
                                isPasswordError = true;
                            }
                            else if (error.Code == "PasswordRequiresLower")
                            {
                                error.Description = "one lowercase (a - z)";
                                if (isPasswordError == false)
                                    error.Description = "Password must have at least " + error.Description;
                                isPasswordError = true;
                            }
                            else if (error.Code == "PasswordRequiresUpper")
                            {
                                error.Description = "one uppercase (A - Z)";
                                if (isPasswordError == false)
                                    error.Description = "Password must have at least " + error.Description;
                                isPasswordError = true;
                            }
                            else if (error.Code == "PasswordRequiresDigit")
                            {
                                error.Description = "one digit (0 - 9)";
                                if (isPasswordError == false)
                                    error.Description = "Password must have at least " + error.Description;
                                isPasswordError = true;
                            }
                        }
                        
                        apiResponseViewModel.Message = string.Join(", ", cre.Errors.Select(x => x.Description));
                        return apiResponseViewModel;
                    }

                    await userManager.AddToRoleAsync(newUser, "Member");
                    user = newUser;
                    //var country = ip2NationService.GetByIpAddress(Request.UserHostAddress);
                    var masterMemberProfileDTO = new MasterMemberProfiles();
                    masterMemberProfileDTO.UserId = newUser.Id.ToString();
                    masterMemberProfileDTO.CreatedAt = DateTime.Now;
                    masterMemberProfileDTO.MemberProfileId = memberProfileObject.Id;
                    masterMemberProfileDTO.PreferLanguage = "en";

                    //Mobile Number not verified - 1754 
                    masterMemberProfileDTO.MobileVerified = "N";
                    masterMemberProfileDTO.AvailablePoints = memberProfileObject.AvailablePoints + memberProfileObject.DemographicPoints;


                    /*
                    masterMemberProfileDTO.MemberProfiles.Add(new MemberProfiles
                    {
                        Guid = Guid.NewGuid().ToString(),
                        IsMasterProfile = true,
                        CreatedAt = DateTime.Now,
                        CreatedAtCountryCode = "MY",
                    });
                    */

                    vodusV2Context.MasterMemberProfiles.Add(masterMemberProfileDTO);
                    await vodusV2Context.SaveChangesAsync();

                    //  Log user in if account is created
                    if (masterMemberProfileDTO != null)
                    {
                        try
                        {

                            var psychographicpoints = await vodusV2Context.MemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfileDTO.MemberProfileId).CountAsync();
                            var deletedResponsesPoints = await vodusV2Context.DeletedSurveyResponses.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfileDTO.MemberProfileId).SumAsync(x => x.PointsCollected);


                            memberProfileObject.MasterMemberProfileId = masterMemberProfileDTO.Id;
                            var result = await signInManager.PasswordSignInAsync(user.UserName,
                           request.Password, true, lockoutOnFailure: false);
                            if (!result.Succeeded)
                            {
                                apiResponseViewModel.Successful = false;
                                return apiResponseViewModel;
                            }

                            //  Create member from token if available
                            /*
                            var userTokenObject = UserToken.FromTokenValue(request.Token);
                            if (userTokenObject != null)
                            {
                                fingerPrint.MemberProfileId = userTokenObject.MemberProfileId;
                                vodusV2Context.Fingerprints.Update(fingerPrint);
                                await vodusV2Context.SaveChangesAsync();
                            }
                            */

                            await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set MasterMemberProfileId={0} where Id={1}", masterMemberProfileDTO.Id, memberProfileObject.Id));

                            vodusV2Context.Database.SetCommandTimeout(6000000);
                            //var memberProfileList = await vodusV2Context.MemberProfiles.FromSqlRaw(string.Format("Select top 1 * from memberprofiles WITH (NOLOCK) where mastermemberprofileid={0}", masterMemberProfiles.Id)).ToListAsync();
                            var memberProfileList = await vodusV2Context.MemberProfiles.FromSqlRaw(string.Format("Select * from memberprofiles WITH (NOLOCK) where mastermemberprofileid={0}", masterMemberProfileDTO.Id)).ToListAsync();

                            var idList = memberProfileList.Select(x => x.Id).ToList();
                            var responses = vodusV2Context.SurveyResponses.Where(x => idList.Contains(x.MemberProfileId));
                            var responseGroup = responses.GroupBy(x => x.MemberProfileId).Select(group =>
                                               new
                                               {
                                                   Id = group.Key,
                                                   Count = group.Count()
                                               }).OrderByDescending(x => x.Count).ToList();

                            var order = 1;
                            var orders = await vodusV2Context.Orders.Where(x => x.MasterMemberId == masterMemberProfileDTO.Id).ToListAsync();
                            var usedPoints = 0;
                            var bonusPoints = 0;

                            var bonusPointsList = await vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == masterMemberProfileDTO.Id && x.IsActive == true).ToListAsync();

                            if (bonusPointsList.Any())
                            {
                                bonusPoints = bonusPointsList.Sum(x => x.Points);
                            }

                            if (orders != null && orders.Any())
                            {
                                usedPoints = orders.Sum(x => x.TotalPoints);
                            }
                            if (responseGroup.Any())
                            {
                                long MasterId = 0;
                                foreach (var item in responseGroup)
                                {
                                    if (order == 1)
                                    {
                                        MasterId = item.Id;
                                        var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();
                                        //memberEntity.IsMasterProfile = true;
                                        //vodusV2Context.Update(memberEntity);
                                        await vodusV2Context.Database.ExecuteSqlRawAsync("Update MemberProfiles set IsMasterProfile=1 where Id=" + item.Id);

                                        var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints + psychographicpoints + deletedResponsesPoints) - usedPoints;

                                        await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MasterMemberProfiles set AvailablePoints={0} where Id={1}", availablePoints, masterMemberProfileDTO.Id));

                                        //  Update mastermember points
                                        //var masterEntity = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterMemberProfiles.Id).FirstOrDefaultAsync();
                                        //masterEntity.AvailablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;
                                        //vodusV2Context.Update(masterEntity);
                                        memberProfileObject = memberEntity;
                                    }
                                    else
                                    {

                                        await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set IsMasterProfile=0 where Id={0}", item.Id));
                                        //var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();
                                        //memberEntity.IsMasterProfile = false;
                                        //vodusV2Context.Update(memberEntity);
                                    }
                                    order++;
                                }

                                foreach (int id in idList)
                                {
                                    if (MasterId != 0 && MasterId != id)
                                        await vodusV2Context.Database.ExecuteSqlRawAsync("Update MemberProfiles set IsMasterProfile=0 where Id=" + id);
                                }
                            }
                            else
                            {
                                var memberListSorted = memberProfileList.OrderByDescending(x => x.AvailablePoints + x.DemographicPoints);
                                foreach (var item in memberListSorted)
                                {
                                    if (order == 1)
                                    {
                                        var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();

                                        await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set IsMasterProfile=1 where Id={0}", item.Id));
                                        //memberEntity.IsMasterProfile = true;
                                        //vodusV2Context.Update(memberEntity);

                                        var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints + psychographicpoints + deletedResponsesPoints) - usedPoints;

                                        await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MasterMemberProfiles set AvailablePoints={0} where Id={1}", availablePoints, masterMemberProfileDTO.Id));

                                        memberProfileObject = memberEntity;
                                        //  Update mastermember points
                                        //var masterEntity = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == masterMemberProfiles.Id).FirstOrDefaultAsync();
                                        //masterEntity.AvailablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;
                                        //memberProfileObject = memberEntity;
                                    }
                                    else
                                    {
                                        await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set IsMasterProfile=0 where Id={0}", item.Id));
                                        //var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();
                                        //memberEntity.IsMasterProfile = false;
                                        //vodusV2Context.Update(memberEntity);
                                    }
                                    order++;
                                }
                            }

                            UserToken u = new UserToken();
                            u.MemberProfileId = memberProfileObject.Id;
                            u.Email = request.Email;
                            u.MemberMasterId = memberProfileObject.MasterMemberProfileId.Value;
                            createAccountResponseViewModel.Token = u.ToTokenValue();

                            if (request.TempToken != null)
                            {
                                var tempToken = await vodusV2Context.TempTokens.Where(x => x.Id == request.TempToken).FirstOrDefaultAsync();
                            
                                if(tempToken != null)
                                {
                                    tempToken.MemberProfileId = memberProfileObject.Id;
                                    tempToken.Token = createAccountResponseViewModel.Token;

                                    vodusV2Context.TempTokens.Update(tempToken);
                                    await vodusV2Context.SaveChangesAsync();
                                }
                            }

                            if (!string.IsNullOrEmpty(request.SyncToken))
                            {
                                var syncObject = UserToken.FromTokenValue(request.SyncToken);

                                if(syncObject != null)
                                {
                                    var syncMember = await vodusV2Context.MemberProfiles.Where(x => x.Id == syncObject.MemberProfileId).FirstOrDefaultAsync();

                                    if (syncMember != null)
                                    {
                                        syncMember.SyncMemberProfileId = memberProfileObject.Id;
                                        syncMember.SyncMemberProfileToken = createAccountResponseViewModel.Token;

                                        vodusV2Context.MemberProfiles.Update(syncMember);
                                        await vodusV2Context.SaveChangesAsync();
                                    }
                                }
                            }

                            var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == u.MemberMasterId).FirstOrDefaultAsync();
                            if (master != null)
                            {
                                createAccountResponseViewModel.PreferredLanguage = master.PreferLanguage;
                                createAccountResponseViewModel.Email = user.Email;
                                createAccountResponseViewModel.Points = master.AvailablePoints;
                            }
                            apiResponseViewModel.Successful = true;
                            apiResponseViewModel.Message = "Account created. Please wait while we sync your points.";
                            createAccountResponseViewModel.RedirectUrl = "myaccount/index";
                            apiResponseViewModel.Data = createAccountResponseViewModel;
                            return apiResponseViewModel;
                        }
                        catch (Exception ex)
                        {
                            /*
                            var errorLogs = new Voupon.Database.Postgres.VodusEntities.ErrorLogs
                            {
                                ActionName = "CreateAccountCommand",
                                ActionRequest = JsonConvert.SerializeObject(request),
                                CreatedAt = DateTime.Now,
                                Errors = ex.ToString(),
                                Email = request.Email,
                                TypeId = CreateErrorLogCommand.Type.Service
                            };
                            */

                            //vodusV2Context.ErrorLogs.Add(errorLogs);
                            await vodusV2Context.SaveChangesAsync();

                            apiResponseViewModel.Successful = false;
                            apiResponseViewModel.Message = "Fail to login after account creation. Please try to relogin manually [900]";
                            return apiResponseViewModel;
                        }
                    }
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to create account";
                    return apiResponseViewModel;
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to create account [999] " + ex.ToString();
                    return apiResponseViewModel;
                }
            }


            private async Task<MemberProfiles> SetMaster(int memberMasterId)
            {
                var newMemberProfile = new MemberProfiles();
                var memberProfileList = await vodusV2Context.MemberProfiles.Where(x => x.MasterMemberProfileId == memberMasterId).ToListAsync();
                var idList = memberProfileList.Select(x => x.Id).ToList();


                var responses = await vodusV2Context.SurveyResponses.Where(x => idList.Contains(x.MemberProfileId)).ToListAsync();
                var responseGroup = responses.GroupBy(x => x.MemberProfileId).Select(group =>
                                   new
                                   {
                                       Id = group.Key,
                                       Count = group.Count()
                                   }).OrderByDescending(x => x.Count);

                var orders = await vodusV2Context.Orders.Where(x => x.MasterMemberId == memberMasterId).ToListAsync();
                var usedPoints = 0;
                var bonusPoints = 0;

                bonusPoints = vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == memberMasterId && x.IsActive == true).Sum(x => x.Points);

                if (orders != null && orders.Any())
                {
                    usedPoints = orders.Sum(x => x.TotalPoints);
                }
                if (responseGroup.Any())
                {
                    var item = responseGroup.First();
                    var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();
                    memberEntity.IsMasterProfile = true;
                    vodusV2Context.MemberProfiles.Update(memberEntity);
                    await vodusV2Context.SaveChangesAsync();

                    //  Update mastermember points
                    var masterEntity = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == memberMasterId).FirstOrDefaultAsync();
                    masterEntity.AvailablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;
                    vodusV2Context.MasterMemberProfiles.Update(masterEntity);
                    await vodusV2Context.SaveChangesAsync();

                    newMemberProfile = memberEntity;

                    if (responseGroup.Count() > 1)
                    {
                        //  Set other 
                        responseGroup.Where(x => x.Id != item.Id);
                    }
                }
                else
                {
                    var memberListSorted = memberProfileList.OrderByDescending(x => x.AvailablePoints + x.DemographicPoints);

                    if (memberListSorted.Any())
                    {
                        var item = memberListSorted.First();
                        var memberEntity = await vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();
                        memberEntity.IsMasterProfile = true;
                        vodusV2Context.Update(memberEntity);
                        await vodusV2Context.SaveChangesAsync();

                        newMemberProfile = memberEntity;

                        //  Update mastermember points
                        var masterEntity = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == memberMasterId).FirstOrDefaultAsync();
                        masterEntity.AvailablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints) - usedPoints;
                        vodusV2Context.Update(masterEntity);
                        await vodusV2Context.SaveChangesAsync();

                        if (memberListSorted.Count() > 1)
                        {
                            memberListSorted.Where(x => x.Id != item.Id);
                        }
                    }
                }
                return newMemberProfile;
            }
        }
    }

}
