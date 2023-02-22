using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Merchant.WebApp.Areas.App.ViewModels.Products;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Products.Pages
{
    public class EditProductPage : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        private class AddUserPageHandler : IRequestHandler<EditProductPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public AddUserPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(EditProductPage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var appConfig = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();

                var viewModel = new EditProductViewModel
                {
                    ProductId = request.ProductId
                };

                var product = await rewardsDBContext.Products.Where(x => x.Id == request.ProductId).FirstOrDefaultAsync();
                if (product == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid product [001]";
                    return apiResponseViewModel;
                }


                viewModel.OutletModel = new OutletViewModel();
                viewModel.OutletModel.RinggitPerVpoints = appConfig.RinggitPerVpoints;
                viewModel.OutletModel.MerchantId = product.MerchantId;
                var thirdPartyType = await rewardsDBContext.ThirdPartyTypes.ToListAsync();

                var thirdPartyTypeList = new List<SelectListItem>();
                thirdPartyTypeList.Add(new SelectListItem
                {
                    Text = "None",
                    Value = ""
                });

                if (thirdPartyType != null && thirdPartyType.Any())
                {
                    foreach (var item in thirdPartyType)
                    {
                        thirdPartyTypeList.Add(new SelectListItem
                        {
                            Value = item.Id.ToString(),
                            Text = item.Name
                        });

                    }
                }

                var thirdPartyProducts = await rewardsDBContext.ThirdPartyProducts.ToListAsync();

                viewModel.OutletModel.ThirdPartyProducts = thirdPartyProducts.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString(),
                }).ToList();

                viewModel.OutletModel.ThirdPartyType = thirdPartyTypeList;
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Data = viewModel;
                return apiResponseViewModel;
            }
        }

    }
}
