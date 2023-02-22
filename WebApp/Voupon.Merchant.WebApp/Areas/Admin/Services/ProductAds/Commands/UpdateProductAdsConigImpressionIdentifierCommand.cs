using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.ProductAds.Commands
{
    public class UpdateProductAdsConigImpressionIdentifierCommand : IRequest<ApiResponseViewModel>
    {
        public int ImpressionCountIdentifier { get; set; }
        public Guid UpdatedBy { get; set; }

    }

    public class UpdateProductAdsConigImpressionIdentifierCommandHandler : IRequestHandler<UpdateProductAdsConigImpressionIdentifierCommand, ApiResponseViewModel>
    {
        VodusV2Context vodusV2;
        public UpdateProductAdsConigImpressionIdentifierCommandHandler(VodusV2Context vodusV2)
        {
            this.vodusV2 = vodusV2;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductAdsConigImpressionIdentifierCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            var productAd = await vodusV2.ProductAdsConfig.FirstOrDefaultAsync();
            if (productAd != null)
            {

                productAd.ImpressionCountIdentifier = request.ImpressionCountIdentifier;
                productAd.LastUpdatedAt = DateTime.Now;
                productAd.LastUpdatedBy = request.UpdatedBy;
                vodusV2.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Ads Impression Identifier Successfully";
            }
            else
            {
                response.Message = "Fail to update";
            }
            return response;
        }
    }
}