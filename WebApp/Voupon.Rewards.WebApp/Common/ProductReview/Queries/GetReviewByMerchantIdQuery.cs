using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Common.ProductReview.Queries
{
    
    public class GetReviewByMerchantIdQuery : IRequest<ApiResponseViewModel>
    {
       

        public int MerchantId { get; set; }
       

    }
    public class GetReviewByMerchantIdQueryHandler : IRequestHandler<GetReviewByMerchantIdQuery, ApiResponseViewModel>
    {
        private readonly VodusV2Context vodusV2Context;
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public GetReviewByMerchantIdQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
        {
            this.vodusV2Context = vodusV2Context;
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(GetReviewByMerchantIdQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                List<ProductReview> list = new List<ProductReview>();
                var item = await rewardsDBContext.ProductReview.Where(x => x.MerchantId == request.MerchantId).OrderByDescending(x=>x.CreatedAt).ToListAsync();
                if (item != null)
                {
                    foreach (var review in item)
                    {
                        ProductReview newReview = new ProductReview();
                        newReview.CreatedAt = review.CreatedAt;
                        newReview.Comment = review.Comment;
                        newReview.MemberName = String.IsNullOrEmpty(review.MemberName) ? "Vodus Customer" : review.MemberName;
                        newReview.ProductId = review.ProductId;
                        newReview.ProductTitle = review.ProductTitle;
                        newReview.Rating = review.Rating;
                        list.Add(newReview);
                    }
                    response.Data = list;
                    response.Successful = true;
                    response.Message = "Get Product Review Successfully";
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Get Product Review Failed";                  
                }
            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
