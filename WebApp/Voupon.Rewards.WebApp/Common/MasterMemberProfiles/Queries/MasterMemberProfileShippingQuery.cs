using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.MasterMemberProfiles.Queries
{
    public class MasterMemberProfileShippingQuery : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
    }
    public class MasterMemberProfileShippingQueryHandler : IRequestHandler<MasterMemberProfileShippingQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        VodusV2Context vodusV2Context;
        public MasterMemberProfileShippingQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
        }

        public async Task<ApiResponseViewModel> Handle(MasterMemberProfileShippingQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                var master = await vodusV2Context.MasterMemberProfiles.Where(x => x.Id == request.MasterMemberProfileId).FirstOrDefaultAsync();

                if (master == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Login failed. Please relogin.";
                    return apiResponseViewModel;
                }

                var user = await vodusV2Context.Users.Where(x => x.Id == master.UserId).FirstOrDefaultAsync();
                var member = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == master.MemberProfileId).FirstOrDefaultAsync();
                var shippingAddress = await vodusV2Context.MasterMemberShippingAddress.Where(x => x.MasterMemberProfileId == master.Id).ToListAsync();
                if (shippingAddress == null)
                {
                    var viewModel = new MasterMemberProfileViewModel
                    {
                        Id = master.Id,
                        MobileCountryCode = master.MobileCountryCode,
                        MobileNumber = master.MobileNumber,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        AddressLine1 = master.AddressLine1,
                        AddressLine2 = master.AddressLine2,
                        State = master.State,
                        Postcode = master.Postcode,
                        City = master.City,

                        //Mobile Number not verified - 1754 
                        MobileVerified = master.MobileVerified,

                        LastGenOtp = master.LastGenOtp

                    };
                    var today = DateTime.Now.DayOfWeek.ToString();
                    if (today.ToUpper() == "WEDNESDAY")
                    {
                        var promoCode = "EveryWednesday2XDiscount";
                        var promo = await rewardsDBContext.PromoCodes.Where(x => x.PromoCode == promoCode).FirstOrDefaultAsync();
                        if (promo != null)
                        {
                            viewModel.PromoCode = promo.PromoCode;
                        }
                    }
                    if (member != null)
                    {
                        if (member.DemographicStateId.HasValue)
                        {
                            viewModel.StateId = member.DemographicStateId.Value;
                            if (viewModel.StateId != 0)
                            {
                                var state = rewardsDBContext.Provinces.Where(x => x.DemographicId == viewModel.StateId).FirstOrDefault();
                                if (state != null)
                                    viewModel.StateId = state.Id;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(viewModel.State))
                    {
                        if (member != null)
                        {
                            var state = await vodusV2Context.DemographicValues.Where(x => x.Id == member.DemographicStateId).FirstOrDefaultAsync();
                            if (state != null)
                            {
                                var province = await rewardsDBContext.Provinces.Where(x => x.IsActivated == true && x.CountryId == 1 && x.Name.ToLower() == state.DisplayValue.ToLower()).FirstOrDefaultAsync();
                                if (province != null)
                                {
                                    viewModel.StateId = state.Id;
                                }
                            }
                        }
                    }
                    else
                    {
                        var state = await rewardsDBContext.Provinces.Where(x => x.IsActivated == true && x.CountryId == 1 && x.Name.ToLower() == viewModel.State.ToLower()).FirstOrDefaultAsync();
                        if (state != null)
                        {
                            viewModel.StateId = state.Id;
                        }
                    }
                    apiResponseViewModel.Data = viewModel;
                }
                else
                {
                    var viewModel = new MasterMemberProfileViewModel
                    {
                        Id = master.Id,
                        MobileCountryCode = master.MobileCountryCode,
                        MobileNumber = master.MobileNumber,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        AddressLine1 = master.AddressLine1,
                        AddressLine2 = master.AddressLine2,
                        State = master.State,
                        Postcode = master.Postcode,
                      
                        City = master.City,

                        //Mobile Number not verified - 1754 
                        MobileVerified = master.MobileVerified,

                        LastGenOtp = master.LastGenOtp

                    };

                    if (string.IsNullOrEmpty(viewModel.State))
                    {
                        if (member != null)
                        {
                            var state = await vodusV2Context.DemographicValues.Where(x => x.Id == member.DemographicStateId).FirstOrDefaultAsync();
                            if (state != null)
                            {
                                var province = await rewardsDBContext.Provinces.Where(x => x.IsActivated == true && x.CountryId == 1 && x.Name.ToLower() == state.DisplayValue.ToLower()).FirstOrDefaultAsync();
                                if (province != null)
                                {
                                    viewModel.StateId = state.Id;
                                }
                            }
                        }
                    }
                    else
                    {
                        var state = await rewardsDBContext.Provinces.Where(x => x.IsActivated == true && x.CountryId == 1 && x.Name.ToLower() == viewModel.State.ToLower()).FirstOrDefaultAsync();
                        if (state != null)
                        {
                            viewModel.StateId = state.Id;
                        }
                    }
                    apiResponseViewModel.Data = viewModel;
                }
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Message = ex.Message;
            }
            apiResponseViewModel.Successful = true;
            apiResponseViewModel.Message = "Get Profile Successfully";

            return apiResponseViewModel;
        }
        public class MasterMemberProfileViewModel
        {
            public int Id { get; set; }
            public string UserId { get; set; }
            public DateTime CreatedAt { get; set; }
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

            //Mobile Number not verified - 1754 
            public string MobileVerified { get; set; }

            //Last Generated OTP
            public string LastGenOtp { get; set; }
            

            public string PreferLanguage { get; set; }
            public DateTime? DateOfBirth { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string CountryName { get; set; }

            public string PromoCode { get; set; }
        }
    }
}
