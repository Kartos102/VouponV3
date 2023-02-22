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
    public class UpdateProductAdsStatusCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductAdId { get; set; }
        public bool Status { get; set; }

    }

    public class UpdateProductAdsStatusCommandHandler : IRequestHandler<UpdateProductAdsStatusCommand, ApiResponseViewModel>
    {
        VodusV2Context vodusV2;
        public UpdateProductAdsStatusCommandHandler(VodusV2Context vodusV2)
        {
            this.vodusV2 = vodusV2;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductAdsStatusCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();            
            var productAd = await vodusV2.ProductAds.FirstAsync(x => x.Id == request.ProductAdId);
            if(productAd != null)
            {

                productAd.IsActive = request.Status;
                vodusV2.SaveChanges();
                response.Successful = true;
                response.Message = "Update Product Ad Status Successfully";
            }
            else
            {
                response.Message = "Product Ad not found";
            }         
            return response;
        }
    }
}
