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
    public class UpdateGifteeVoucherProductStatus : IRequest<ApiResponseViewModel>
    {
        public string Id { get; set; }
        public bool Status { get; set; }
        public class UpdateGifteeVoucherProductStatusHandler : IRequestHandler<UpdateGifteeVoucherProductStatus, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public UpdateGifteeVoucherProductStatusHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }
            public async Task<ApiResponseViewModel> Handle(UpdateGifteeVoucherProductStatus request, CancellationToken cancellationToken)
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
                    gifteeThirdPartyProduct.Status = Convert.ToByte(request.Status);
                    rewardsDBContext.ThirdPartyProducts.Update(gifteeThirdPartyProduct);
                    rewardsDBContext.SaveChanges();
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully Update Giftee product Status";
                    //apiResponseViewModel.Data = gifteeProducts;
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
