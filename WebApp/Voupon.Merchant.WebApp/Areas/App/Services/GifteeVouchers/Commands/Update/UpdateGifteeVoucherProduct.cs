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
    public class UpdateGifteeVoucherProduct : IRequest<ApiResponseViewModel>
    {
        public string Id { get; set; }
        public string ProductName { get; set; }
        public string ProductExternalId { get; set; }
        public class UpdateGifteeVoucherProductHandler : IRequestHandler<UpdateGifteeVoucherProduct, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public UpdateGifteeVoucherProductHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }
            public async Task<ApiResponseViewModel> Handle(UpdateGifteeVoucherProduct request, CancellationToken cancellationToken)
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
                    var gifteeThirdPartyProduct = await rewardsDBContext.ThirdPartyProducts.Where(x => x.Id.ToString() == request.Id && x.ThirdPartyTypeId == gifteeThirdPartyType.Id).FirstOrDefaultAsync();
                    if (gifteeThirdPartyProduct == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Could not get Giftee Third Product type [002]";
                        return apiResponseViewModel;
                    }
                    gifteeThirdPartyProduct.Name = request.ProductName;
                    gifteeThirdPartyProduct.ExternalId = request.ProductExternalId;
                    rewardsDBContext.ThirdPartyProducts.Update(gifteeThirdPartyProduct);
                    rewardsDBContext.SaveChanges();
                    apiResponseViewModel.Successful = true;
                    //apiResponseViewModel.Data = gifteeProducts;
                    apiResponseViewModel.Message = "Successfully Update Giftee product";
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
