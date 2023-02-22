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
using Microsoft.AspNetCore.Identity;
using Voupon.Common;
using JWT.Algorithms;
using JWT;
using JWT.Serializers;
using Google.Apis.Auth;
using JWT.Builder;

namespace Voupon.API.Functions
{
    public class LoginFunction
    {
        private readonly UserManager<Database.Postgres.VodusEntities.Users> userManager;
        private readonly RewardsDBContext rewardsDBContext;
        private readonly VodusV2Context vodusV2Context;
        private readonly IConnectionMultiplexer connectionMultiplexer;
        private readonly IAzureBlobStorage azureBlobStorage;
        private readonly SignInManager<Database.Postgres.VodusEntities.Users> signInManager;
        private readonly IJwtAlgorithm _algorithm;
        private readonly IJsonSerializer _serializer;
        private readonly IBase64UrlEncoder _base64Encoder;
        private readonly IJwtEncoder _jwtEncoder;



        public LoginFunction(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IConnectionMultiplexer connectionMultiplexer,
            SignInManager<Database.Postgres.VodusEntities.Users> signInManager, UserManager<Database.Postgres.VodusEntities.Users> userManager, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.connectionMultiplexer = connectionMultiplexer;
            this.azureBlobStorage = azureBlobStorage;
            this.signInManager = signInManager;
            this.userManager = userManager;

            // JWT specific initialization.
            // https://github.com/jwt-dotnet/jwt
            _algorithm = new HMACSHA256Algorithm();
            _serializer = new JsonNetSerializer();
            _base64Encoder = new JwtBase64UrlEncoder();
            _jwtEncoder = new JwtEncoder(_algorithm, _serializer, _base64Encoder);
        }

        [OpenApiOperation(operationId: "Login", tags: new[] { "Identity" }, Description = "Login", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(LoginResponseModel), Summary = "Logged in user data and JWT")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to authenticate")]
        [OpenApiRequestBody("application/json", typeof(LoginRequestModel), Description = "JSON request body ")]

        [FunctionName("LoginFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "identity/login")] HttpRequest req, ILogger log)
        {
            var fbId = "";
            var fbEmail = "";
            var fbName = "";
            var data = req;
            var login = new Login();
            try
            {
              
                var requestModel = HttpRequestHelper.DeserializeModel<LoginRequestModel>(req);

                if(string.IsNullOrEmpty(requestModel.LoginProvider))
                {
                    requestModel.LoginProvider = "email";
                }    

                if (requestModel.LoginProvider == "facebook")
                {
                    var httpClient = new HttpClient();
                    var fbResult = await httpClient.GetAsync($"https://graph.facebook.com/v13.0/me?access_token={requestModel.ThirdPartyToken}&fields=id,name,email");
                    if (fbResult.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            ErrorMessage = "Invalid request, please try again [001]"
                        });
                    }

                    dynamic fbData = JsonConvert.DeserializeObject(await fbResult.Content.ReadAsStringAsync());
                    
                    login.Email = fbData.email;
                    string[] names = fbData.name.Split(' ');
                    login.FirstName = names[0];
                    login.LastName = names[1];
                    requestModel.Email = fbData.email;
                }
                else if (requestModel.LoginProvider == "google")
                {
                    var validPayload = await GoogleJsonWebSignature.ValidateAsync(requestModel.ThirdPartyToken);
                    requestModel.Email = validPayload.Email;
                    login.Email = validPayload.Email;
                    login.FirstName = validPayload.GivenName;
                    login.LastName = validPayload.FamilyName;
                    if(!string.IsNullOrEmpty(validPayload.Picture))
                    {
                        login.ProfilePhotoUrl = validPayload.Picture;
                    }
                    /*
                    if (validPayload.Email != requestModel.Email)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid Google login [001]";
                        return apiResponseViewModel;
                    }
                    */

                }

                //throw new Exception("Some test");
                if (string.IsNullOrEmpty(requestModel.Email))
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        ErrorMessage = "Email is required"
                    });
                }

                if (requestModel.LoginProvider == "email")
                {
                    if (string.IsNullOrEmpty(requestModel.Password))
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            ErrorMessage = "Password is required"
                        });
                    }
                }


                PartnerWebsites partnerWebsite = null;
                
                MemberProfiles memberProfileObject = null;
                var device = new Devices();

                var masterMemberProfiles = new MasterMemberProfiles();
                UserToken userTokenObject = null;
                if (string.IsNullOrEmpty(requestModel.Token))
                {
                   userTokenObject = UserToken.FromTokenValue(requestModel.Token);
                }


                //  Check email exist
                var user = await userManager.FindByEmailAsync(requestModel.Email);

                if (user != null)
                {
                    if (string.IsNullOrEmpty(requestModel.LoginProvider))
                    {
                        requestModel.LoginProvider = "Email";
                    }

                    if (requestModel.LoginProvider.ToUpper() == "FaceBook".ToUpper() || requestModel.LoginProvider.ToUpper() == "Google".ToUpper())
                    {

                    }
                    else
                    {
                        var passwordResult = userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, requestModel.Password);
                        if (passwordResult == PasswordVerificationResult.Success || passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
                        {
                            //   Passed. Rehash?

                        }
                        else
                        {
                            return new BadRequestObjectResult(new ApiResponseViewModel
                            {
                                ErrorMessage = "Invalid email/password"
                            });
                        }
                    }
                }
                else
                {
                    if (requestModel.LoginProvider.ToLower() == "email")
                    {
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            ErrorMessage = "Invalid email/password"
                        });
                    }

                    //  No account exist for thirdparty login facebook/google
                    //  Create a new account
                    if (requestModel.LoginProvider.ToLower() == "google")
                    {
                        //  Verify google token
                        try
                        {

                            var newUser = new Database.Postgres.VodusEntities.Users()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ActivationCode = Guid.NewGuid().ToString(),
                                UserName = requestModel.Email,
                                Email = requestModel.Email,
                                NormalizedEmail = requestModel.Email.Normalize(),
                                NormalizedUserName = requestModel.Email.Normalize(),
                                EmailConfirmed = true,
                                PhoneNumberConfirmed = false,
                                CreatedAt = DateTime.UtcNow
                            };

                            var randomString = Guid.NewGuid().ToString("n").Substring(0, 8);
                            var cre = await userManager.CreateAsync(newUser, "Vodus1" + randomString + "#");
                            if (!cre.Succeeded)
                            {
                                return new BadRequestObjectResult(new ApiResponseViewModel
                                {
                                    ErrorMessage = string.Join(" , ", cre.Errors.Select(x => x.Description))
                                });
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
                            return new BadRequestObjectResult(new ApiResponseViewModel
                            {
                                ErrorMessage = "Invalid Google login [002]"
                            });
                        }
                    }
                    else if (requestModel.LoginProvider.ToLower() == "facebook")
                    {
                        //  Verify facebook token
                        try
                        {

                            var newUser = new Database.Postgres.VodusEntities.Users()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ActivationCode = Guid.NewGuid().ToString(),
                                UserName = requestModel.Email,
                                Email = requestModel.Email,
                                NormalizedEmail = requestModel.Email.Normalize(),
                                NormalizedUserName = requestModel.Email.Normalize(),
                                EmailConfirmed = true,
                                PhoneNumberConfirmed = false,
                                CreatedAt = DateTime.UtcNow,
                                FirstName = fbName
                            };

                            var randomString = Guid.NewGuid().ToString("n").Substring(0, 8);
                            var cre = await userManager.CreateAsync(newUser, "Vodus1" + randomString + "#");
                            if (!cre.Succeeded)
                            {
                                return new BadRequestObjectResult(new ApiResponseViewModel
                                {
                                    ErrorMessage = string.Join(" , ", cre.Errors.Select(x => x.Description))
                                });
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
                            return new BadRequestObjectResult(new ApiResponseViewModel
                            {
                                ErrorMessage = "Invalid Facebook login [002]"
                            });
                        }
                    }
                }


                //  Check if user exist in master
                var userDTO = vodusV2Context.Users.Include(x => x.UserLogins).Include(x => x.MasterMemberProfiles).Where(x => x.Email == requestModel.Email).FirstOrDefault();
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

                    //  Update last logged in
                    //await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update users set lastloggedinat=GETDATE() where Id={0}", userDTO.Id));

                    if (userTokenObject != null)
                    {
                        var profile = await vodusV2Context.MemberProfiles.Where(x => x.Id == userTokenObject.MemberProfileId).FirstOrDefaultAsync();
                        profile.MasterMemberProfileId = masterMemberProfiles.Id;
                        vodusV2Context.MemberProfiles.Update(profile);
                        await vodusV2Context.SaveChangesAsync();
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

                    var memberProfiles = await vodusV2Context.MemberProfiles.Where(x => x.Id == masterMemberProfiles.MemberProfileId).ToListAsync();
                    /*.Select(x => new MemberProfiles
                     {
                         Id = x.Id,
                         State = x.State,
                         StateId = x.StateId,
                         AvailablePoints = x.AvailablePoints,
                         DemographicStateId = x.DemographicStateId,
                         DemographicPoints = x.DemographicPoints
                     })*/
                    memberProfileList = memberProfiles.Select(x => new MemberProfiles
                    {
                        Id = x.Id,
                        //State = x.State,
                        StateId = x.StateId,
                        AvailablePoints = x.AvailablePoints,
                        DemographicStateId = x.DemographicStateId,
                        DemographicPoints = x.DemographicPoints
                    }).ToList();


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

                    var orders = await vodusV2Context.Orders.Where(x => x.MasterMemberId == masterMemberProfiles.Id).ToListAsync();
                    var ordersFromRewards = await rewardsDBContext.Orders.Where(x => x.OrderStatus == 2 && x.MasterMemberProfileId == masterMemberProfiles.Id).ToListAsync();
                    var usedPoints = 0;
                    var bonusPoints = 0;
                    var refundedPoints = 0;
                    var psychographicpoints = 0;
                    var deletedPsychographicpoints = 0;
                    var deletedResponsesPoints = 0;

                    var refunds = await rewardsDBContext.Refunds.Where(x => x.Status == 2 && x.MasterMemberProfileId == masterMemberProfiles.Id).ToListAsync();
                    if (refunds != null && refunds.Any())
                    {
                        refundedPoints = refunds.Sum(x => x.PointsRefunded);
                    }

                    var bonusPointsList = await vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == masterMemberProfiles.Id && x.IsActive == true).ToListAsync();

                    if (bonusPointsList.Any())
                    {
                        bonusPoints = bonusPointsList.Sum(x => x.Points);
                    }

                    if (orders != null && orders.Any())
                    {
                        usedPoints = orders.Sum(x => x.TotalPoints);
                    }

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

                        var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints + refundedPoints + psychographicpoints + deletedResponsesPoints + deletedPsychographicpoints) - usedPoints;


                        await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set IsMasterProfile=1,MasterMemberProfileId={1},AvailablePoints={2} where Id={0}", item.Id, masterMemberProfiles.Id, memberEntity.AvailablePoints));
                        await vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MasterMemberProfiles set AvailablePoints={0}, MemberProfileid={2} where Id={1}", availablePoints, masterMemberProfiles.Id, item.Id));

                        memberProfileObject = memberEntity;
                        login.Points = availablePoints;
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

                            login.Points = availablePoints;
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

                    if (string.IsNullOrEmpty(requestModel.TempToken) && string.IsNullOrEmpty(requestModel.SyncToken))
                    {

                    }
                    else
                    {
                        var requiredUpdate = false;
                        var generatedToken = userToken.ToTokenValue();
                        if (!string.IsNullOrEmpty(requestModel.TempToken))
                        {
                            await vodusV2Context.Database.ExecuteSqlRawAsync($"Update TempTokens set MemberProfileId={memberProfileObject.Id},Token='{generatedToken}', LastUpdatedAt=GETDATE() where Id='{requestModel.TempToken}'");
                            requiredUpdate = true;
                        }
                        if (!string.IsNullOrEmpty(requestModel.SyncToken))
                        {
                            var syncTokenObject = UserToken.FromTokenValue(requestModel.SyncToken);
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
                            login.PreferredLanguage = requestModel.Locale;
                        }
                        else
                        {
                            login.PreferredLanguage = masterMemberProfiles.PreferLanguage;
                        }
                    }

                    await userManager.AddClaimAsync(user, new System.Security.Claims.Claim("MasterMemberProfileId", masterMemberProfiles.Id.ToString()));


                    if (!string.IsNullOrEmpty(requestModel.ThirdPartyToken))
                    {
                        await signInManager.SignInAsync(user, true);

                    }
                    else
                    {
                        var result = await signInManager.PasswordSignInAsync(user.NormalizedUserName,
                requestModel.Password, true, lockoutOnFailure: false);
                        if (!result.Succeeded)
                        {
                            return new BadRequestObjectResult(new ApiResponseViewModel
                            {
                                ErrorMessage = "Invalid email/password [001]"
                            });
                        }
                    }

                    login.Email = userDTO.Email;
                    login.CCToken = userToken.ToTokenValue();
                    login.Points = masterMemberProfiles.AvailablePoints;

                    if(string.IsNullOrEmpty(login.FirstName))
                    {
                        login.FirstName = userDTO.FirstName;
                    }

                    if (string.IsNullOrEmpty(login.LastName))
                    {
                        login.LastName = userDTO.LastName;
                    }

                    if (string.IsNullOrEmpty(login.ProfilePhotoUrl))
                    {
                        login.ProfilePhotoUrl = "https://vodus.my/images/default-profile-photo.jpg";
                    }

                    login.PreferredLanguage = (string.IsNullOrEmpty(masterMemberProfiles.PreferLanguage) ? "en" : masterMemberProfiles.PreferLanguage);
                    
                    login.LoginProvider = requestModel.LoginProvider;

                    Dictionary<string, object> claims = new Dictionary<string, object>
                        {
                            { Authentication.Claim.USER_ID, masterMemberProfiles.UserId },
                            { Authentication.Claim.MASTER_MEMBER_PROFILE_ID, masterMemberProfiles.Id },
                            { Authentication.Claim.MEMBER_PROFILE_ID, masterMemberProfiles.MemberProfileId },
                            { Authentication.Claim.EMAIL,  login.Email },
                            { Authentication.Claim.FIRST_NAME, login.FirstName },
                            { Authentication.Claim.LAST_NAME, login.LastName },
                            { Authentication.Claim.LOGGED_IN_AT, DateTime.Now },
                            { Authentication.Claim.EXP, DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds() }
                        };

                    string token = _jwtEncoder.Encode(claims, Environment.GetEnvironmentVariable(EnvironmentKey.JWT_SECRET));

                    if (string.IsNullOrEmpty(token))
                    {
                        //  @TODO log
                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            ErrorMessage = "Fail to generate token"
                        });
                    }

                    login.JWT = token;

                    var responseModel = new LoginResponseModel();

                    responseModel.Data = new LoginData();
                    responseModel.Data.Login = login;

                    responseModel.Data.Profile = new Profile
                    {
                        Id = masterMemberProfiles.Id,
                        AvailablePoints = masterMemberProfiles.AvailablePoints,
                        MobileCountryCode = masterMemberProfiles.MobileCountryCode,
                        MobileNumber = masterMemberProfiles.MobileNumber,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        AddressLine1 = masterMemberProfiles.AddressLine1,
                        AddressLine2 = masterMemberProfiles.AddressLine2,
                        State = masterMemberProfiles.State,
                        Postcode = masterMemberProfiles.Postcode,
                        City = masterMemberProfiles.City,
                        CountryId = masterMemberProfiles.CountryId,
                        CountryName = "Malaysia"
                    };
                    var memberP = memberProfileList.FirstOrDefault();
                    if (memberP != null)
                    {
                        if (memberP.DemographicStateId.HasValue)
                        {
                            responseModel.Data.Profile.StateId = memberP.DemographicStateId.Value;
                            if (responseModel.Data.Profile.StateId != 0)
                            {
                                var state = rewardsDBContext.Provinces.Where(x => x.DemographicId == responseModel.Data.Profile.StateId).FirstOrDefault();
                                if (state != null)
                                    responseModel.Data.Profile.StateId = state.Id;
                            }
                        }
                    }
                    responseModel.Data.Profile.AlternateShippingAddress = await vodusV2Context.MasterMemberShippingAddress.
                        Where(x => x.MasterMemberProfileId == masterMemberProfiles.Id).Select(x => new AlternateShippingAddress
                    {
                        Id = x.Id,
                        AddressLine1 = x.AddressLine1,
                        AddressLine2 = x.AddressLine2,
                        City = x.City,
                        CountryId = x.CountryId,
                        FirstName = x.FirstName,
                        LastName = x.LastName,
                        isLastSelected = x.IsLastSelected,
                        MasterMemberProfileId = x.MasterMemberProfileId,
                        Postcode = x.Postcode,
                        State = x.State,
                        CreatedAt = x.CreatedAt
                    }).ToListAsync();

                    return new OkObjectResult(responseModel);
                }
                else
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        ErrorMessage = "Invalid email/password"
                    });
                }

            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    ErrorMessage = "Something is not right.. Please try again later. " + JsonConvert.SerializeObject(data)
                });;
            }
        }

        protected class LoginResponseModel : ApiResponseViewModel
        {
            public LoginData Data { get; set; }
        }

        protected class LoginData
        {
            public Login Login { get; set; }

            public Profile Profile { get; set; }

            
        }

        public class AlternateShippingAddress
        {
            public int Id { get; set; }
            public int MasterMemberProfileId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Postcode { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public short CountryId { get; set; }
            public string CountryName { get; set; }
            public System.DateTime CreatedAt { get; set; }

            public bool isLastSelected { get; set; }
        }

        protected class Profile
        {
            public int Id { get; set; }
            public int AvailablePoints { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string Postcode { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public int StateId { get; set; }
            public short? CountryId { get; set; }
            public string MobileCountryCode { get; set; }
            public string MobileNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string CountryName { get; set; }
            public string PromoCode { get; set; }

            public List<AlternateShippingAddress> AlternateShippingAddress { get; set; }
        }

        protected class Login
        {
            public string Email { get; set; }
            public string CCToken { get; set; }
            public string PreferredLanguage { get; set; }
            public int Points { get; set; }
            public string RedirectUrl { get; set; }
            public string JWT { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string ProfilePhotoUrl { get; set; }
            public string LoginProvider { get; set; }
        }

        protected class LoginRequestModel
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string Token { get; set; }
            public string PartnerCode { get; set; }
            public string Locale { get; set; }
            //public string PreferredLanguage { get; set; }
            //public HttpDTO HttpDTO { get; set; }
            public string LoginProvider { get; set; }
            public string ThirdPartyToken { get; set; }
            public bool RedirectToCheckOut { get; set; }
            public string TempToken { get; set; }
            public string SyncToken { get; set; }
        }

        protected class HttpDTO
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
    }
}