using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Logger
{
    public class CreateErrorLogCommand : IRequest<ApiResponseViewModel>
    {
        public static class Type
        {
            public static readonly byte Controller = 1;
            public static readonly byte Service = 2;
        }


        public byte TypeId { get; set; }
        public string ActionName { get; set; }
        public string ActionRequest { get; set; }
        public string Errors { get; set; }

        public string Email { get; set; }
        public int? MemberProfileId { get; set; }
        public int? MasterProfileId { get; set; }
        public string Token { get; set; }

        private class CreateErrorLogCommandHandler : IRequestHandler<CreateErrorLogCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;
            public CreateErrorLogCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(CreateErrorLogCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                var errorLogs = new ErrorLogs
                {
                    ActionName = request.ActionName,
                    ActionRequest = request.ActionRequest,
                    Errors = request.Errors,
                    TypeId = request.TypeId,
                    CreatedAt = DateTime.Now,
                };

                if (!string.IsNullOrEmpty(request.Token))
                {
                    errorLogs.Token = request.Token;
                }

                if (!string.IsNullOrEmpty(request.Email))
                {
                    errorLogs.Email = request.Email;
                }

                if (request.MasterProfileId.HasValue)
                {
                    errorLogs.MasterProfileId = request.MasterProfileId.Value;
                }

                if (request.MemberProfileId.HasValue)
                {
                    errorLogs.MemberProfileId = request.MemberProfileId.Value;
                }

                rewardsDBContext.ErrorLogs.Add(errorLogs);
                rewardsDBContext.SaveChanges();

                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;

            }
        }

    }

}
