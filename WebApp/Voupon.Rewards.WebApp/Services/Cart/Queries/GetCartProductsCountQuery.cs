using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using Products = Voupon.Database.Postgres.RewardsEntities.Products;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.Services.Cart.Models;

namespace Voupon.Rewards.WebApp.Services.Cart.Queries
{
    public class GetCartProductsCountQuery : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
    }
    public class GetCartProductsCountQueryHandler : IRequestHandler<GetCartProductsCountQuery, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;


        public GetCartProductsCountQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;

        }
        public async Task<ApiResponseViewModel> Handle(GetCartProductsCountQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                var cartProductsCount=  rewardsDBContext.CartProducts.AsNoTracking().Count(x => x.MasterMemberProfileId == request.MasterMemberProfileId);
                var externalCartProductsCount = rewardsDBContext.CartProductExternal.AsNoTracking().Count(x => x.MasterMemberProfileId == request.MasterMemberProfileId);

                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Get Cart Products count successfully";
                apiResponseViewModel.Data = cartProductsCount + externalCartProductsCount;
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Data = 0;
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Fail to Get Cart Products count";
                return apiResponseViewModel;

            }
        }
    }
}
