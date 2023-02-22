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
    public class DeleteGifteeVoucherProduct : IRequest<ApiResponseViewModel>
    {
        public string ProductId { get; set; }
        public class DeleteGifteeVoucherProductHandler : IRequestHandler<DeleteGifteeVoucherProduct, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public DeleteGifteeVoucherProductHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }
            public async Task<ApiResponseViewModel> Handle(DeleteGifteeVoucherProduct request, CancellationToken cancellationToken)
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
                    var gifteeThirdPartyProduct = await rewardsDBContext.ThirdPartyProducts.Where(x => x.Id.ToString() == request.ProductId && x.ThirdPartyTypeId == gifteeThirdPartyType.Id).FirstOrDefaultAsync();
                    if (gifteeThirdPartyProduct == null)
                    {
                        apiResponseViewModel.Successful = false;
                        apiResponseViewModel.Message = "Could not get Giftee Third Product type [002]";
                        return apiResponseViewModel;
                    }
                    rewardsDBContext.ThirdPartyProducts.Remove(gifteeThirdPartyProduct);
                    rewardsDBContext.SaveChanges();
                    apiResponseViewModel.Successful = true;
                    //apiResponseViewModel.Data = gifteeProducts;
                    apiResponseViewModel.Message = "Successfully Deleted Giftee product";
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
