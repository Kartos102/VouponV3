using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.CrawledProduct.Models;
using Voupon.Merchant.WebApp.Common.Services.CrawlerMerchant.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.CrawledProduct.Queries
{
    public class CrawledProductListQuery : IRequest<ApiResponseViewModel>
    {
        public int limit { get; set; }
        public int offset { get; set; }
        public string searchValue { get; set; }
    }


    public class CrawledProductListQueryHandler : IRequestHandler<CrawledProductListQuery, ApiResponseViewModel>
    {

        RewardsDBContext rewardsDBContext;
        public CrawledProductListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CrawledProductListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var query = rewardsDBContext.ConsoleProductJSON.AsQueryable();

                if (!string.IsNullOrEmpty(request.searchValue))
                {
                    query = query.Where(x => x.ItemName.ToLower().Contains(request.searchValue));
                }
                var count = query.Count();
                var crawledProduct = query.Skip(request.limit).Take(request.offset).ToList();
                
                var ids = crawledProduct.Select(x => x.ExternalMerchantId).ToList();

                var merchants = await rewardsDBContext.ConsoleMerchantJSON.Where(x => ids.Contains(x.ExternalId)).OrderByDescending(x=> x.CreatedAt).ToListAsync();
                List<CrawledProductModel> list = new List<CrawledProductModel>();
                foreach (var item in crawledProduct)
                {
                    CrawledProductModel newItem = new CrawledProductModel();
                    newItem.Id = item.Id;
                    newItem.PageUrl = item.PageUrl;
                    newItem.ExternalMerchantId = item.ExternalMerchantId;
                    newItem.Url = item.URL;
                    newItem.StatusId = item.StatusId;
                    newItem.ItemName = item.ItemName;
                    newItem.LastUpdateAt = item.LastUpdatedAt;
                    var merchant = merchants.Where(x=> x.ExternalId == item.ExternalMerchantId).FirstOrDefault();
                    if (merchant != null) {
                        newItem.MerchantName = merchant.MerchantUsername;
                    }

                    list.Add(newItem);
                }
                var totalPage = (int)Math.Ceiling((double)count / request.offset);
                CrawledProductPaginationModel data = new CrawledProductPaginationModel();
                data.Crawled = list;
                data.Pagination = new Pagination() { 
                    Limit = request.limit,
                    TotalData = count,
                    TotalPage = totalPage
                };
                response.Successful = true;
                response.Message = "Get Crawled Product List Successfully";
                response.Data = data;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
