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
    public class CreateProductAdsCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }

    }

    public class CreateProductAdsCommandHandler : IRequestHandler<CreateProductAdsCommand, ApiResponseViewModel>
    {
        VodusV2Context vodusV2;
        public CreateProductAdsCommandHandler(VodusV2Context vodusV2)
        {
            this.vodusV2 = vodusV2;
        }

        public async Task<ApiResponseViewModel> Handle(CreateProductAdsCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var productAd = await vodusV2.ProductAds.Where(x => x.Id == request.ProductId).FirstOrDefaultAsync();
                if (productAd == null)
                {
                    await vodusV2.ProductAds.AddAsync(new Voupon.Database.Postgres.VodusEntities.ProductAds
                    {
                        ProductId = request.ProductId,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    });

                    await vodusV2.SaveChangesAsync();
                }
                else
                {
                    response.Message = "Product Ads already exist. Skip creation";
                }
                return response;
            }
            catch (Exception ex)
            {
                response.Message = ex.ToString();
                return response;
            }

        }
    }
}
