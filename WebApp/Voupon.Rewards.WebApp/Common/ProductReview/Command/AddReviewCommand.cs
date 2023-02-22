using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

namespace Voupon.Rewards.WebApp.Common.ProductReview.Command
{

    public class AddReviewCommand : IRequest<ApiResponseViewModel>
    {
        public Guid OrderItemId { get; set; }
        public int Rating { get; set; }

        public int ProductId { get; set; }
        public int MerchantId { get; set; }
        public int MasterMemberProfileId { get; set; }

        public string Comment { get; set; }

    }
    public class AddReviewCommandHandler : IRequestHandler<AddReviewCommand, ApiResponseViewModel>
    {
        private readonly VodusV2Context vodusV2Context;
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public AddReviewCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
        {
            this.vodusV2Context = vodusV2Context;
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(AddReviewCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = await rewardsDBContext.ProductReview.FirstOrDefaultAsync(x => x.OrderItemId == request.OrderItemId);
                if (item != null)
                {
                    response.Successful = false;
                    response.Message = "Order Item already reviewed";
                }
                else
                {
                    var member = await vodusV2Context.MasterMemberProfiles.FirstOrDefaultAsync(x => x.Id == request.MasterMemberProfileId);
                    var user = await vodusV2Context.Users.FirstOrDefaultAsync(x => x.Id == member.UserId);
                    var product = await rewardsDBContext.Products.FirstOrDefaultAsync(x => x.Id == request.ProductId);
                    var newReview = new Voupon.Database.Postgres.RewardsEntities.ProductReview();
                    newReview.Id = Guid.NewGuid();
                    newReview.Comment = request.Comment;
                    if (newReview.Comment == null)
                        newReview.Comment = "";
                    newReview.MasterMemberProfileId = request.MerchantId;
                    newReview.MerchantId = request.MerchantId;
                    newReview.OrderItemId = request.OrderItemId;
                    newReview.ProductId = request.ProductId;
                    newReview.Rating = request.Rating;
                    newReview.CreatedAt = DateTime.Now;
                    newReview.ProductTitle = product.Title;
                    newReview.MemberName = (String.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName + " ") + (String.IsNullOrEmpty(user.LastName) ? "" : user.LastName);
                    await rewardsDBContext.ProductReview.AddAsync(newReview);
                    await rewardsDBContext.SaveChangesAsync();
                    var productReviewList = await rewardsDBContext.ProductReview.Where(x => x.ProductId == request.ProductId).ToListAsync();
                    var averageRating = productReviewList.Sum(x => x.Rating) / productReviewList.Count;

                    product.Rating = averageRating;
                    product.TotalRatingCount = product.TotalRatingCount + 1;

                    var merchant = await rewardsDBContext.Merchants.FirstOrDefaultAsync(x => x.Id == request.MerchantId);
                    productReviewList = await rewardsDBContext.ProductReview.Where(x => x.MerchantId == request.MerchantId).ToListAsync();
                    averageRating = productReviewList.Sum(x => x.Rating) / productReviewList.Count;
                    merchant.Rating = averageRating;
                    merchant.TotalRatingCount = merchant.TotalRatingCount + 1;


                    await rewardsDBContext.SaveChangesAsync();
                    response.Successful = true;
                    response.Message = "Order Item reviewed";
                }
            }
            catch (Exception ex)
            {
                rewardsDBContext = new RewardsDBContext();
                var errorLogs = new Voupon.Database.Postgres.RewardsEntities.ErrorLogs
                {
                    ActionName = "AddReviewCommand",
                    TypeId = 2,
                    CreatedAt = DateTime.Now,
                    ActionRequest = JsonConvert.SerializeObject(request),
                    Errors = ex.ToString()
                };

                rewardsDBContext.ErrorLogs.Add(errorLogs);
                await rewardsDBContext.SaveChangesAsync();


                response.Successful = false;
                response.Message = "Fail to submit review";
            }

            return response;
        }
    }
}
