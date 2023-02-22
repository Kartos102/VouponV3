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
    public class MasterMemberProfileDetailsQuery : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
    }
    public class MasterMemberProfileDetailsQueryHandler : IRequestHandler<MasterMemberProfileDetailsQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        VodusV2Context vodusV2Context;
        public MasterMemberProfileDetailsQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
        }

        public async Task<ApiResponseViewModel> Handle(MasterMemberProfileDetailsQuery request, CancellationToken cancellationToken)
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
                //var member = await vodusV2Context.MemberProfiles.Where(x => x.IsMasterProfile == true && x.MasterMemberProfileId == request.MasterMemberProfileId).FirstOrDefaultAsync();
                var member = await vodusV2Context.MemberProfiles.AsNoTracking().Where(x => x.Id == master.MemberProfileId).FirstOrDefaultAsync();

                //var member = memberList.Where(x => x.IsMasterProfile == true).FirstOrDefault();

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
                    City = master.City
                };
                var today = DateTime.Now.DayOfWeek.ToString();
                if (today.ToUpper() == "WEDNESDAY")
                {
                    var promoCode = "EveryWednesday2XDiscount";
                    var promo = await rewardsDBContext.PromoCodes.Where(x=> x.PromoCode == promoCode).FirstOrDefaultAsync();
                    if (promo != null)
                    {
                        viewModel.PromoCode = promo.PromoCode;
                    }
                }
                if (member != null)
                {
                    if(member.DemographicStateId.HasValue)
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
                            viewModel.State = state.DisplayValue;
                            viewModel.StateId = state.Id;

                        }
                    }
                }

                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Get Profile Successfully";
                apiResponseViewModel.Data = viewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Message = ex.Message;
            }

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
            public string PreferLanguage { get; set; }
            public DateTime? DateOfBirth { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string CountryName { get; set; }

            public string PromoCode { get; set; }
        }
    }
}
