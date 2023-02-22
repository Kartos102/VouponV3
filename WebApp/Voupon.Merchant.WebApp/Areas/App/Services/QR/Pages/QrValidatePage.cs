using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.App.Services.QR.Pages
{
    public class QRValidatePage : IRequest<ApiResponseViewModel>
    {
        public string Token { get; set; }
        public int MerchantId { get; set; }

        public class QRValidatePageHandler : IRequestHandler<QRValidatePage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public QRValidatePageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(QRValidatePage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();

                if (string.IsNullOrEmpty(request.Token))
                {
                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Invalid request[0001]";
                    return apiResponseViewModel;
                }

                var token = await rewardsDBContext.InStoreRedemptionTokens.Include(x => x.Merchant).Include(x => x.OrderItem).ThenInclude(x => x.Product).ThenInclude(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Where(x => x.Token == request.Token).FirstOrDefaultAsync();
                if (token == null)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Invalid request[0002]";
                    return apiResponseViewModel;
                }

                if (token.MerchantId != request.MerchantId)
                {
                    //  apiResponseViewModel.Successful = false;
                    //  apiResponseViewModel.Message = "Invalid request[0003]";
                    //  return apiResponseViewModel;
                }

                if (token.IsRedeemed)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "This item have been redeemed before";
                    return apiResponseViewModel;
                }

                dynamic productDetail = JsonConvert.DeserializeObject<dynamic>(token.OrderItem.ProductDetail);

                var price = productDetail.price.Value;

                var viewModel = new QRValidatePageViewModel
                {
                    Id = token.Id,
                    Token = token.Token,
                    ProductTitle = token.ProductTitle,
                    ProductImageUrl = token.OrderItem.ProductImageFolderUrl,
                    Points = token.OrderItem.Points,
                    Price = (decimal)productDetail.price.Value,
                    DiscountedPrice = (decimal)productDetail.discountedPrice.Value,
                    DiscountRate = (int)productDetail.discountRate.Value,
                    AvailableOutlets = token.OrderItem.Product.ProductOutlets.Select(x => new AvailableOutlets
                    {
                        Id = x.Outlet.Id,
                        Name = x.Outlet.Name

                    }).ToList(),
                Merchant = new DetailPageMerchant
                {
                    Id = token.Merchant.Id,
                    Code = token.Merchant.Code,
                    DisplayName = token.Merchant.DisplayName,
                    LogoUrl = token.Merchant.LogoUrl
                },
                };

                apiResponseViewModel.Data = viewModel;
                apiResponseViewModel.Successful = true;
                return apiResponseViewModel;
            }

        }

        public class QRValidatePageViewModel
        {
            public int Id { get; set; }
            public string Token { get; set; }
            public string ProductTitle { get; set; }

            public string ProductImageUrl { get; set; }

            public decimal Price { get; set; }

            public decimal DiscountedPrice { get; set; }

            public int DiscountRate { get; set; }

            public int Points { get; set; }

            public List<AvailableOutlets> AvailableOutlets { get; set; }
            public DetailPageMerchant Merchant { get; set; }
        }
        public class DetailPageMerchant
        {
            public int Id { get; set; }
            public string Code { get; set; }
            public string DisplayName { get; set; }
            public string LogoUrl { get; set; }
        }

        public class AvailableOutlets
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }


}
