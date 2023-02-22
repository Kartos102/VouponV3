using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using Voupon.Rewards.WebApp.Services.Cart.Models;

namespace Voupon.Rewards.WebApp.Services.Cart.Commands
{
    public class DeleteFromCartExternalCommand : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }

        public Guid Id { get; set; }
    }
    public class DeleteFromCartExternalCommandHandler : IRequestHandler<DeleteFromCartExternalCommand, ApiResponseViewModel>
    {
        private readonly RewardsDBContext rewardsDBContext;

        public DeleteFromCartExternalCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings, IAzureBlobStorage azureBlobStorage)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(DeleteFromCartExternalCommand request, CancellationToken cancellationToken)
        {
            var cartProductExternal = new CartProductExternal();
            var apiResponseViewModel = new ApiResponseViewModel();

            try
            {
                var externalCartProducts = await rewardsDBContext.CartProductExternal.Where(x => x.Id == request.Id && x.MasterMemberProfileId == request.MasterMemberProfileId).FirstOrDefaultAsync();

                if (externalCartProducts == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to remove item from the cart";
                    return apiResponseViewModel;
                }

                rewardsDBContext.CartProductExternal.Remove(externalCartProducts);
                await rewardsDBContext.SaveChangesAsync();

                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }
            catch (Exception ex)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Fail to Add to Cart";
                return apiResponseViewModel;

            }
        }
    }
}
