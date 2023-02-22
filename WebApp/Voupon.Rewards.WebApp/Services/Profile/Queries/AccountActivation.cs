using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Database.Postgres.VodusEntities;
using System.Threading;
using Microsoft.Extensions.Options;

namespace Voupon.Rewards.WebApp.Services.Profile.Queries
{
    public class AccountActivation : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }

        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class AccountActivationHandler : IRequestHandler<AccountActivation, ApiResponseViewModel>
    {
        VodusV2Context VodusV2Context;
        private IOptions<AppSettings> appSettings;


        public AccountActivationHandler(VodusV2Context vodusV2, IOptions<AppSettings> appSettings)
        {
            this.VodusV2Context = vodusV2;
            this.appSettings = appSettings;

        }

        public async Task<ApiResponseViewModel> Handle(AccountActivation request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();

            var user = VodusV2Context.Users.Where(x => x.ActivationCode == request.Code).FirstOrDefault();

         
            if (user == null)
            {
                apiResponseViewModel.Message = "Invalid activation";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }
            if (user.EmailConfirmed)
            {
                apiResponseViewModel.Message = "Your email is already activated";
                apiResponseViewModel.Successful = false;
                return apiResponseViewModel;
            }

            if (user.NewPendingVerificationEmail.ToLower() == request.Email.ToLower())
            {
                user.EmailConfirmed = true;
                user.ActivatedAt = DateTime.Now;
                user.Email = user.NewPendingVerificationEmail.ToLower();
                user.UserName = user.NewPendingVerificationEmail.ToLower();
                VodusV2Context.Users.Update(user);
                VodusV2Context.SaveChanges();
            }

            apiResponseViewModel.Message = "Email activated successfully";
            apiResponseViewModel.Successful = true;
            apiResponseViewModel.Data = appSettings.Value.App.BaseUrl;
            return apiResponseViewModel;
        }
    }

   
   
}
