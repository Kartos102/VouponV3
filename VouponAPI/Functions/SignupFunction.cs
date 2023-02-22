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
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.API.Util;
using Voupon.API.ViewModels;
using Microsoft.AspNetCore.Identity;
using Voupon.Common;
using System.Collections.Generic;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;

namespace Voupon.API.Functions
{
    public class SignupFunction
    {
        private readonly VodusV2Context _vodusV2Context;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IJwtEncoder _jwtEncoder;
        private readonly IJwtAlgorithm _algorithm;
        private readonly IJsonSerializer _serializer;
        private readonly IBase64UrlEncoder _base64Encoder;
        public SignupFunction(VodusV2Context vodusV2Context, UserManager<Users> userManager, SignInManager<Users> signInManager)
        {
            this._vodusV2Context = vodusV2Context;
            this._userManager = userManager;
            this._signInManager = signInManager;

            _algorithm = new HMACSHA256Algorithm();
            _serializer = new JsonNetSerializer();
            _base64Encoder = new JwtBase64UrlEncoder();
            _jwtEncoder = new JwtEncoder(_algorithm, _serializer, _base64Encoder);

        }

        [OpenApiOperation(operationId: "Sign Up", tags: new[] { "Identity" }, Description = "Sign Up", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(SignUpResponseModel), Summary = "Sign up")]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.BadRequest, Summary = "if unable to authenticate")]
        [OpenApiRequestBody("application/json", typeof(SignUpRequestModel), Description = "JSON request body ")]

        [FunctionName("SignUp")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "identity/signup")] HttpRequest req, ILogger log)
        {
            var responseModel = new SignUpResponseModelView();
            var request = HttpRequestHelper.DeserializeModel<SignUpRequestModel>(req);
            MemberProfiles memberProfileObject = null;
            try 
            {
                
                var user = await _vodusV2Context.Users.Where(x => x.Email == request.Email).FirstOrDefaultAsync();
                if (user != null)
                {
                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        ErrorMessage = "Email already in use. Please login or use other email"
                    });
                }

                if (request.Token != null)
                {
                    var userTokenObject = UserToken.FromTokenValue(request.Token, true);
                    if (userTokenObject != null)
                    {
                        memberProfileObject = await _vodusV2Context.MemberProfiles.Where(x => x.Id == userTokenObject.MemberProfileId).FirstOrDefaultAsync();
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

                        _vodusV2Context.MemberProfiles.Add(memberProfileDTO);
                        await _vodusV2Context.SaveChangesAsync();

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

                    _vodusV2Context.MemberProfiles.Add(memberProfileDTO);
                    await _vodusV2Context.SaveChangesAsync();

                    memberProfileObject = memberProfileDTO;
                }

                if (!string.IsNullOrEmpty(request.TempToken))
                {
                    var tempTokenObject = await _vodusV2Context.TempTokens.Where(x => x.Id == new Guid(request.TempToken)).FirstOrDefaultAsync();
                    if (tempTokenObject != null)
                    {
                        if (tempTokenObject.MemberProfileId.HasValue)
                        {
                            if (tempTokenObject.MemberProfileId != 0)
                            {
                                var tempMember = await _vodusV2Context.MemberProfiles.Where(x => x.Id == tempTokenObject.MemberProfileId).FirstOrDefaultAsync();
                                if (tempMember != null)
                                {
                                    memberProfileObject = tempMember;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(request.SyncToken))
                    {
                        var syncTokenObject = UserToken.FromTokenValue(request.SyncToken);
                        if (syncTokenObject != null)
                        {
                            var tempMember = await _vodusV2Context.MemberProfiles.Where(x => x.Id == syncTokenObject.MemberProfileId).FirstOrDefaultAsync();
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
                    CreatedAt = DateTime.UtcNow
                };
                var cre = await _userManager.CreateAsync(newUser, request.Password);
                if (!cre.Succeeded)
                {
                    bool isPasswordError = false;
                    foreach (var error in cre.Errors)
                    {
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

                    return new BadRequestObjectResult(new ApiResponseViewModel
                    {
                        ErrorMessage = string.Join(", ", cre.Errors.Select(x => x.Description))

                    });

                }
                await _userManager.AddToRoleAsync(newUser, "Member");
                user = newUser;

                //var country = ip2NationService.GetByIpAddress(Request.UserHostAddress);
                var masterMemberProfileDTO = new MasterMemberProfiles();
                masterMemberProfileDTO.UserId = newUser.Id;
                masterMemberProfileDTO.CreatedAt = DateTime.UtcNow;
                masterMemberProfileDTO.MemberProfileId = memberProfileObject.Id;
                masterMemberProfileDTO.PreferLanguage = "en";
                masterMemberProfileDTO.AvailablePoints = memberProfileObject.AvailablePoints + memberProfileObject.DemographicPoints;

                _vodusV2Context.MasterMemberProfiles.Add(masterMemberProfileDTO);
                await _vodusV2Context.SaveChangesAsync();

                if (masterMemberProfileDTO != null)
                {
                    try
                    {

                        var psychographicpoints = await _vodusV2Context.MemberPsychographics.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfileDTO.MemberProfileId).CountAsync();
                        var deletedResponsesPoints = await _vodusV2Context.DeletedSurveyResponses.AsNoTracking().Where(x => x.MemberProfileId == masterMemberProfileDTO.MemberProfileId).SumAsync(x => x.PointsCollected);


                        memberProfileObject.MasterMemberProfileId = masterMemberProfileDTO.Id;
                        var result = await _signInManager.PasswordSignInAsync(user.UserName,
                        request.Password, true, lockoutOnFailure: false);
                        if (!result.Succeeded)
                        {
                            return new BadRequestObjectResult(new ApiResponseViewModel
                            {
                            });
                        }

                        await _vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set MasterMemberProfileId={0} where Id={1}", masterMemberProfileDTO.Id, memberProfileObject.Id));

                        _vodusV2Context.Database.SetCommandTimeout(6000000);
                        var memberProfileList = await _vodusV2Context.MemberProfiles.FromSqlRaw(string.Format("Select * from memberprofiles WITH (NOLOCK) where mastermemberprofileid={0}", masterMemberProfileDTO.Id)).ToListAsync();

                        var idList = memberProfileList.Select(x => x.Id).ToList();
                        var responses = _vodusV2Context.SurveyResponses.Where(x => idList.Contains(x.MemberProfileId));
                        var responseGroup = responses.GroupBy(x => x.MemberProfileId).Select(group =>
                                           new
                                           {
                                               Id = group.Key,
                                               Count = group.Count()
                                           }).OrderByDescending(x => x.Count).ToList();

                        var order = 1;
                        var orders = await _vodusV2Context.Orders.Where(x => x.MasterMemberId == masterMemberProfileDTO.Id).ToListAsync();
                        var usedPoints = 0;
                        var bonusPoints = 0;

                        var bonusPointsList = await _vodusV2Context.BonusPoints.Where(x => x.MasterMemberProfileId == masterMemberProfileDTO.Id && x.IsActive == true).ToListAsync();

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
                                    var memberEntity = await _vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();

                                    await _vodusV2Context.Database.ExecuteSqlRawAsync("Update MemberProfiles set IsMasterProfile=1 where Id=" + item.Id);

                                    var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints + psychographicpoints + deletedResponsesPoints) - usedPoints;

                                    await _vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MasterMemberProfiles set AvailablePoints={0} where Id={1}", availablePoints, masterMemberProfileDTO.Id));

                                    memberProfileObject = memberEntity;
                                }
                                else
                                {
                                    await _vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set IsMasterProfile=0 where Id={0}", item.Id));
                                }
                                order++;
                            }

                            foreach (int id in idList)
                            {
                                if (MasterId != 0 && MasterId != id)
                                    await _vodusV2Context.Database.ExecuteSqlRawAsync("Update MemberProfiles set IsMasterProfile=0 where Id=" + id);
                            }
                        }
                        else
                        {
                            var memberListSorted = memberProfileList.OrderByDescending(x => x.AvailablePoints + x.DemographicPoints);
                            foreach (var item in memberListSorted)
                            {
                                if (order == 1)
                                {
                                    var memberEntity = await _vodusV2Context.MemberProfiles.Where(x => x.Id == item.Id).FirstOrDefaultAsync();

                                    await _vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set IsMasterProfile=1 where Id={0}", item.Id));

                                    var availablePoints = (memberEntity.AvailablePoints + memberEntity.DemographicPoints + bonusPoints + psychographicpoints + deletedResponsesPoints) - usedPoints;

                                    await _vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MasterMemberProfiles set AvailablePoints={0} where Id={1}", availablePoints, masterMemberProfileDTO.Id));

                                    memberProfileObject = memberEntity;

                                }
                                else
                                {
                                    await _vodusV2Context.Database.ExecuteSqlRawAsync(string.Format("Update MemberProfiles set IsMasterProfile=0 where Id={0}", item.Id));

                                }
                                order++;
                            }
                        }

                        UserToken u = new UserToken();
                        u.MemberProfileId = memberProfileObject.Id;
                        u.Email = request.Email;
                        u.MemberMasterId = memberProfileObject.MasterMemberProfileId.Value;
                        responseModel.Token = u.ToTokenValue();

                        if (!string.IsNullOrEmpty(request.TempToken))
                        {
                            var tempToken = await _vodusV2Context.TempTokens.Where(x => x.Id == new Guid(request.TempToken)).FirstOrDefaultAsync();

                            if (tempToken != null)
                            {
                                tempToken.MemberProfileId = memberProfileObject.Id;
                                tempToken.Token = responseModel.Token;

                                _vodusV2Context.TempTokens.Update(tempToken);
                                await _vodusV2Context.SaveChangesAsync();
                            }
                        }

                        if (!string.IsNullOrEmpty(request.SyncToken))
                        {
                            var syncObject = UserToken.FromTokenValue(request.SyncToken);

                            if (syncObject != null)
                            {
                                var syncMember = await _vodusV2Context.MemberProfiles.Where(x => x.Id == syncObject.MemberProfileId).FirstOrDefaultAsync();

                                if (syncMember != null)
                                {
                                    syncMember.SyncMemberProfileId = memberProfileObject.Id;
                                    syncMember.SyncMemberProfileToken = responseModel.Token;

                                    _vodusV2Context.MemberProfiles.Update(syncMember);
                                    await _vodusV2Context.SaveChangesAsync();
                                }
                            }
                        }

                        Dictionary<string, object> claims = new Dictionary<string, object>
                        {
                            { Authentication.Claim.USER_ID, user.Id },
                            { Authentication.Claim.MASTER_MEMBER_PROFILE_ID, u.MemberMasterId },
                            { Authentication.Claim.MEMBER_PROFILE_ID, u.MemberProfileId },
                            { Authentication.Claim.EMAIL,  memberProfileObject.Email },
                            { Authentication.Claim.FIRST_NAME, user.FirstName },
                            { Authentication.Claim.LAST_NAME, user.LastName },
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

                        var master = await _vodusV2Context.MasterMemberProfiles.Where(x => x.Id == u.MemberMasterId).FirstOrDefaultAsync();
                        if (master != null)
                        {
                            var userToken = new UserToken
                            {
                                MemberProfileId = memberProfileObject.Id,
                                Email = user.Email,
                                MemberMasterId = master.Id,
                                CreatedAt = DateTime.Now
                            };


                            responseModel.PreferredLanguage = master.PreferLanguage;
                            responseModel.Email = user.Email;
                            responseModel.Points = master.AvailablePoints;
                            responseModel.Jwt = token;
                            responseModel.CCToken = userToken.ToTokenValue();
                        }

                        
                    }
                    catch (Exception ex){
                        // var errorLogs = new Database.Postgres.VodusEntities.ErrorLogs
                        // {
                        //     ActionName = "CreateAccountCommand",
                        //     ActionRequest = JsonConvert.SerializeObject(request),
                        //     CreatedAt = DateTime.Now,
                        //     Errors = ex.ToString(),
                        //     Email = request.Email,
                        //     //TypeId = CreateErrorLogCommand.Type.Service
                        // };
                        //
                        // _vodusV2Context.ErrorLogs.Add(errorLogs);
                        // await _vodusV2Context.SaveChangesAsync();

                        return new BadRequestObjectResult(new ApiResponseViewModel
                        {
                            ErrorMessage = "Fail to login after account creation. Please try to relogin manually [900]"
                        });
                    }

                }
            }catch (Exception ex)
            {
                // var errorLogs = new Database.Postgres.VodusEntities.ErrorLogs
                // {
                //     ActionName = "CreateAccountCommand",
                //     ActionRequest = JsonConvert.SerializeObject(request),
                //     CreatedAt = DateTime.Now,
                //     Errors = ex.ToString(),
                //     Email = request.Email,
                //     //TypeId = CreateErrorLogCommand.Type.Service
                // };
                //
                // _vodusV2Context.ErrorLogs.Add(errorLogs);
                // await _vodusV2Context.SaveChangesAsync();

                return new BadRequestObjectResult(new ApiResponseViewModel
                {
                    ErrorMessage = "Fail to create account [999] " + ex.ToString()
                });
            }


            var response = new SignUpResponseModel();
            response.Code = 0;
            response.Data = new SignupData();
            response.Data.Signup = responseModel;
            return new OkObjectResult(response);

        }

        protected class SignUpRequestModel 
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
        protected class SignUpResponseModelView
        {
            public int MasterMemberProfileId { get; set; }
            public string Email { get; set; }
            public string Token { get; set; }
            public string PreferredLanguage { get; set; }
            public int Points { get; set; }
            public string RedirectUrl { get; set; }
            public string CCToken { get; set; }
            public string Jwt { get; set; }
        }
        protected class SignUpResponseModel : ApiResponseViewModel
        {
            public SignupData Data { get; set; }
        }

        protected class SignupData
        {
            public SignUpResponseModelView Signup { get; set; }
        }


        protected class LoginModel 
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
        }
    }
}
