using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Database.Postgres.RewardsEntities;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Voupon.Merchant.WebApp.Areas.Admin.ViewModels.ProductAds;
using Microsoft.Extensions.Options;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.ProductAds.Queries
{
    public class GetServerlessUrlQuery : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
    }
    public class GetServerlessUrlQueryHandler : IRequestHandler<GetServerlessUrlQuery, ApiResponseViewModel>
    {
        private readonly IOptions<AppSettings> appSettings;
        public GetServerlessUrlQueryHandler(IOptions<AppSettings> appSettings)
        {
            this.appSettings = appSettings;
        }
        public async Task<ApiResponseViewModel> Handle(GetServerlessUrlQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                string serverlessUrl = appSettings.Value.App.ServerlessUrl;
                if (serverlessUrl != null)
                {
                    response.Successful = true;
                    response.Message = "Got Serverless Url Successfully";
                    response.Data = serverlessUrl;
                }
                else
                {
                    response.Message = "Fail to Get Got Serverless Url";
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
