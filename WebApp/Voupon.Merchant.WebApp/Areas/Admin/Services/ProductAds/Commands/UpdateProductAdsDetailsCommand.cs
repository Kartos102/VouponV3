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
    public class UpdateProductAdsDetailsCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductAdId { get; set; }
        public int AdImpressionCount { get; set; }
        public int AdClickCount { get; set; }
        public decimal CTR { get; set; }

    }

    public class UpdateProductAdsDetailsCommandHandler : IRequestHandler<UpdateProductAdsDetailsCommand, ApiResponseViewModel>
    {
        VodusV2Context vodusV2;
        public UpdateProductAdsDetailsCommandHandler(VodusV2Context vodusV2)
        {
            this.vodusV2 = vodusV2;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductAdsDetailsCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            var productAd = await vodusV2.ProductAds.FirstAsync(x => x.Id == request.ProductAdId);
            if (productAd != null)
            {

                productAd.AdImpressionCount = request.AdImpressionCount;
                productAd.CTR = request.CTR;
                productAd.AdClickCount = request.AdClickCount;
                vodusV2.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Ad details Successfully";
            }
            else
            {
                response.Message = "Fail to update Product Ad details";
            }
            return response;
        }
    }
}
