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

namespace Voupon.Rewards.WebApp.Services.Cart.Commands
{
    public class DelateFromCartCommand : IRequest<ApiResponseViewModel>
    {
        public int CartProductId { get; set; }
    }
    public class DelateFromCartCommandHandler : IRequestHandler<DelateFromCartCommand, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        private readonly IAzureBlobStorage azureBlobStorage;

        public DelateFromCartCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
            this.azureBlobStorage = azureBlobStorage;
        }

        public async Task<ApiResponseViewModel> Handle(DelateFromCartCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel apiResponseView = new ApiResponseViewModel();
            try { 
            var cartProduct = await rewardsDBContext.CartProducts.Where(x => x.Id == request.CartProductId).FirstOrDefaultAsync();
            if(cartProduct != null)
            {
                rewardsDBContext.CartProducts.Remove(cartProduct);
                rewardsDBContext.SaveChanges();
            }
            }
            catch(Exception ex)
            {
                apiResponseView.Successful = false;
                apiResponseView.Message = "Fail to remove product from cart";
                return apiResponseView;
            }
            apiResponseView.Successful = true;
            apiResponseView.Message = "successfully removed product from cart";
            return apiResponseView;
        }
    }
}
