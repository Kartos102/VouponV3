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

namespace Voupon.Merchant.WebApp.Common.Services.CrawlerMerchant.Queries
{
    public class CrawlerMerchantListQuery : IRequest<ApiResponseViewModel>
    {
        public int limit { get; set; }
        public int offset { get; set; }
        public string searchValue { get; set; }
    }
    public class CrawlerMerchantListQueryHandler : IRequestHandler<CrawlerMerchantListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CrawlerMerchantListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CrawlerMerchantListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var query = rewardsDBContext.ConsoleMerchantToCrawl.AsQueryable();

                if (!string.IsNullOrEmpty(request.searchValue)) {
                    query = query.Where(x => x.MerchantName.ToLower().Contains(request.searchValue) || x.Url.Contains(request.searchValue));
                }
                var count = query.Count();
                var crawlerMerchant = query.Skip(request.limit).Take(request.offset).ToList();

                List<CrawlerMerchantModel> list = new List<CrawlerMerchantModel>();
                foreach (var item in crawlerMerchant)
                {
                    CrawlerMerchantModel newItem = new CrawlerMerchantModel();
                    newItem.Id = item.Id;
                    newItem.MerchantName = item.MerchantName;
                    newItem.Url = item.Url;
                    newItem.StatusId = item.StatusId;
                    newItem.Remark = item.Remark;
                    newItem.LastUpdateAt = item.LastUpdatedAt;

                    list.Add(newItem);
                }
                var totalPage = (int)Math.Ceiling((double)count / request.offset);
                CrawledMerchantPaginationModel data = new CrawledMerchantPaginationModel();
                data.Crawled = list;
                data.Pagination = new Pagination()
                {
                    Limit = request.limit,
                    TotalData = count,
                    TotalPage = totalPage
                };
                response.Successful = true;
                response.Message = "Get Crawler Merchants List Successfully";
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
