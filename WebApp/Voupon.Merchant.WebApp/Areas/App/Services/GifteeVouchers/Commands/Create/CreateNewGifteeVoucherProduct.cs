using MediatR;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Services.GifteeVouchers.Commands.Create
{
    public class CreateNewGifteeVoucherProduct : IRequest<ApiResponseViewModel>
    {
        public string ProductName { get; set; }
        public string ProductExternalId { get; set; }
        public class CreateNewGifteeVoucherProductHandler : IRequestHandler<CreateNewGifteeVoucherProduct, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public CreateNewGifteeVoucherProductHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }
            public async Task<ApiResponseViewModel> Handle(CreateNewGifteeVoucherProduct request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var gifteeThirdPartyType = await rewardsDBContext.ThirdPartyTypes.Where(x => x.Name == "Giftee").FirstOrDefaultAsync();
                    if (gifteeThirdPartyType == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Could not get Giftee Third party type [001]";
                        return apiResponseViewModel;
                    }
                    ThirdPartyProducts thirdPartyProduct = new ThirdPartyProducts()
                    {
                        Id = Guid.NewGuid(),
                        Name = request.ProductName,
                        ExternalId = request.ProductExternalId,
                        ThirdPartyTypeId = gifteeThirdPartyType.Id,
                        Status = 1
                    };
                    rewardsDBContext.ThirdPartyProducts.Add(thirdPartyProduct);
                    rewardsDBContext.SaveChanges();
                    apiResponseViewModel.Successful = true;
                    //apiResponseViewModel.Data = gifteeProducts;
                    apiResponseViewModel.Message = "Successfully Create Giftee product";
                    return apiResponseViewModel;
                }
                catch (Exception ex) {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;
                    return apiResponseViewModel;
                }

            }
        }
    }
}
