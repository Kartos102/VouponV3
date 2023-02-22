using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.SignUp.Commands.Update
{
    public class SetupPasswordCommand : SetupPasswordCommandRequestViewModel, IRequest<ApiResponseViewModel>
    {
        private class SetupPasswordCommandHandler : IRequestHandler<SetupPasswordCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;

            private readonly UserManager<Users> userManager;
            private readonly SignInManager<Users> signInManager;
            public SetupPasswordCommandHandler(RewardsDBContext rewardsDBContext, UserManager<Users> userManager, SignInManager<Users> signInManager)
            {
                this.rewardsDBContext = rewardsDBContext;
                this.userManager = userManager;
                this.signInManager = signInManager;
            }

            public async Task<ApiResponseViewModel> Handle(SetupPasswordCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                try
                {
                    rewardsDBContext.Database.BeginTransaction();

                    var tempUserEntity = await rewardsDBContext.TempUsers.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                    var config = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                    decimal commission = 0;
                    if (config != null)
                    {
                        commission = config.DefaultCommission;
                    }
                    else
                    {
                        commission = 3.0M;
                    }
                    if (tempUserEntity == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Invalid request[1]";
                        return apiResponseViewModel;
                    }

                    var userEntity = await rewardsDBContext.Users.Where(x => x.Email == tempUserEntity.Email).FirstOrDefaultAsync();
                    if (userEntity != null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Account already exists. Try to login instead.";
                        return apiResponseViewModel;
                    }

                    if (!tempUserEntity.TACVerifiedAt.HasValue)
                    {
                        apiResponseViewModel.Code = 99;
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Account not verified yet. Unable to setup password";
                        return apiResponseViewModel;
                    }

                    if (request.Password != request.ConfirmPassword)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Password does not match Confirm Password.";
                        return apiResponseViewModel;
                    }

                    var newUser = new Users()
                    {
                        Id = Guid.NewGuid(),
                        UserName = tempUserEntity.Email,
                        Email = tempUserEntity.Email,
                        NormalizedEmail = tempUserEntity.Email.Normalize(),
                        NormalizedUserName = tempUserEntity.Email.Normalize(),
                        EmailConfirmed = false,
                        PhoneNumber = tempUserEntity.MobileNumber,
                        PhoneNumberConfirmed = true,
                        CreatedAt = DateTime.Now
                    };

                    var cre = await userManager.CreateAsync(newUser, request.Password);
                    if (cre.Succeeded)
                    {

                        await userManager.AddToRoleAsync(newUser, "Merchant");
                        var merchant = new Merchants()
                        {
                            DisplayName = tempUserEntity.BusinessName,
                            CountryId = tempUserEntity.CountryId,
                            CompanyContact = tempUserEntity.MobileNumber,
                            StatusTypeId = Voupon.Common.Enum.StatusTypeEnum.DRAFT,
                            CreatedAt = DateTime.Now,
                            IsPublished = false,
                            CreatedByUserId = newUser.Id,
                            DefaultCommission = commission
                        };
                        var newmerchant = await rewardsDBContext.Merchants.AddAsync(merchant);
                        await rewardsDBContext.SaveChangesAsync();
                        var tempMerchant = await rewardsDBContext.Merchants.FirstOrDefaultAsync(x => x.Id == newmerchant.Entity.Id);
                        tempMerchant.Code = "MY" + tempMerchant.Id.ToString("D5");
                        var bankAccount = new BankAccounts()
                        {
                            MerchantId = newmerchant.Entity.Id,
                            StatusTypeId = Voupon.Common.Enum.StatusTypeEnum.DRAFT,
                            CreatedAt = DateTime.Now,
                            CreatedByUserId = newUser.Id
                        };
                        await rewardsDBContext.BankAccounts.AddAsync(bankAccount);

                        var personInCharge = new PersonInCharges()
                        {
                            MerchantId = newmerchant.Entity.Id,
                            StatusTypeId = Voupon.Common.Enum.StatusTypeEnum.DRAFT,
                            CreatedAt = DateTime.Now,
                            CreatedByUserId = newUser.Id
                        };
                        await rewardsDBContext.PersonInCharges.AddAsync(personInCharge);

                        await rewardsDBContext.UserClaims.AddAsync(new UserClaims() { UserId = newUser.Id, ClaimType = "MerchantId", ClaimValue = newmerchant.Entity.Id.ToString() });
                        var role = await rewardsDBContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == newUser.Id);
                        role.MerchantId = newmerchant.Entity.Id;
                        rewardsDBContext.TempUsers.Remove(tempUserEntity);

                        //  Remove other related user details

                        var tempAccountList = await rewardsDBContext.TempUsers.Where(x => x.Email == tempUserEntity.Email || x.MobileNumber == tempUserEntity.MobileNumber).ToListAsync();

                        if(tempAccountList != null && tempAccountList.Any())
                        {
                            rewardsDBContext.TempUsers.RemoveRange(tempAccountList);
                        }

                        await rewardsDBContext.SaveChangesAsync();
                        rewardsDBContext.Database.CommitTransaction();
                        apiResponseViewModel.Successful = true;
                    }
                    else
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = string.Join(" , ", cre.Errors.Select(x => x.Description));
                        return apiResponseViewModel;
                    }
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Message = "Fail to complete password creation. Please contact support team.";
                    apiResponseViewModel.Successful = false;
                }

                return apiResponseViewModel;
            }
        }
    }

    public class SetupPasswordCommandRequestViewModel
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public Guid Id { get; set; }

    }

    public class SetupPasswordCommandResponseViewModel
    {
        public string Id { get; set; }
    }

}
