using MediatR;
using Microsoft.EntityFrameworkCore;
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
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;

namespace Voupon.Rewards.WebApp.Common.Products.Queries
{
    public class ProductListSearchQuery : IRequest<ApiResponseViewModel>
    {
        public string SearchText { get; set; }
        public int PageNumber { get; set; }

        public int[] SubCategory { get; set; }

        public bool IsCategory { get; set; }

        public int ProductTypeId { get; set; }

        public bool IsPriceFilter { get; set; }

        public int MinPrice { get; set; }

        public int MaxPrice { get; set; }

        public int[] Location { get; set; }
    }
    public class ProductListSearchQueryHandler : IRequestHandler<ProductListSearchQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        VodusV2Context vodusV2Context;
        private IOptions<AppSettings> appSettings;

        public ProductListSearchQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(ProductListSearchQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var provinces = await rewardsDBContext.Provinces.AsNoTracking().ToListAsync();

                var queryable = rewardsDBContext.Products.AsNoTracking()
                    .Include(x => x.ProductOutlets).
                    ThenInclude(x => x.Outlet).
                    Include(x => x.Merchant).
                    Include(x => x.ProductDiscounts).
                    Include(x => x.ProductSubCategory).
                    Include(x => x.ProductCategory).
                    Include(x => x.StatusType).
                    Include(x => x.DealType).
                    Include(x => x.DealExpirations).
                    Where((x => x.IsPublished == true && x.IsActivated == true));

                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    queryable = queryable.Where(x => x.Title.Contains(request.SearchText)
                                                || x.ProductSubCategory.Name.Contains(request.SearchText)
                                                || x.ProductCategory.Name.Contains(request.SearchText));
                }

                if (request.SubCategory.Length > 0 && request.IsCategory)
                {
                    queryable = queryable.Where(x => request.SubCategory.Contains(x.ProductSubCategoryId.Value));
                }

                if (request.IsCategory && request.ProductTypeId == 0)
                {
                    queryable = queryable.Where(x => request.SubCategory.Contains(x.ProductCategoryId.Value));
                }
                if (request.ProductTypeId != 0)
                {
                    queryable = request.ProductTypeId != 8 ? queryable.Where(x => x.ProductCategoryId == request.ProductTypeId).Take(100) : queryable.Where(x => x.ProductCategoryId == request.ProductTypeId || x.ProductCategoryId == null).Take(100); //Added by shanuka, 2023-02-15
                }

                if (request.IsPriceFilter)
                {
                    queryable = queryable.Where(x => x.DiscountedPrice.Value >= request.MinPrice && x.DiscountedPrice.Value <= request.MaxPrice);
                }

                if (request.Location.Length > 0)
                {
                    if (!request.Location.Contains(20) || !(request.Location.Contains(18) && request.Location.Contains(17)))
                    {
                        var locationFilter = GetListLocation(request.Location);
                        List<int> filteredIds = new List<int>();
                        if (locationFilter != null)
                        {
                            queryable = queryable.Where(x => locationFilter.Contains((int)x.ProductOutlets.Select(y => y.Outlet.ProvinceId.Value).FirstOrDefault()));
                        }
                    }
                }

                //Get product per merchant = 6 and Maximum product per page = 30

                var _getProductCount = appSettings.Value.ProductSearchSettings.PerMerchant;
                var _perPage = appSettings.Value.ProductSearchSettings.PerPage; ;
                var _totalTakeCount = 0;

                List<int> merchantIds = await queryable.Select(x => x.MerchantId).Distinct().ToListAsync();
                merchantIds.Sort();

                var PIds = queryable.Select(x => new ProductPagingModel
                {
                    Id = x.Id,
                    MerchantId = x.MerchantId,
                    PageNumber = 0
                }).OrderBy(x => x.MerchantId).ThenBy(x => x.Id).ToList();

                for (int i = 1; i <= request.PageNumber; i++)
                {
                    _totalTakeCount = 0;
                    reTakeProduct:
                    foreach (var id in merchantIds)
                    {
                        var takeCount = _getProductCount;
                        var productCount = PIds.Where(x => x.MerchantId == id && x.PageNumber == 0).Take(_getProductCount).Count();
                        if (productCount > 0)
                        {
                            _totalTakeCount += productCount;
                            if (_totalTakeCount > _perPage)
                                takeCount = _perPage - (_totalTakeCount - productCount);

                            foreach (var pItem in PIds.Where(x => x.MerchantId == id && x.PageNumber == 0).Take(takeCount).ToList())
                                pItem.PageNumber = i;

                            if (_totalTakeCount >= _perPage)
                                break;
                        }
                    }
                    if (PIds.Where(x => x.PageNumber == 0).Count() != 0 && _perPage > _totalTakeCount)
                        goto reTakeProduct;
                }


                //var offset = 30;
                //var limit = (request.PageNumber - 1) * offset;
                //var _limitMerchant = (request.PageNumber - 1) * _maxMerchant;

                //int merchantCount = merchantIds.Count();

                //if (merchantCount > _maxMerchant)
                //{
                //    merchantIds = merchantIds.OrderBy(x => x).Skip(_limitMerchant).Take(_maxMerchant).ToList();
                //}
                //if (merchantCount == 0)
                //{
                //    merchantCount = 1;
                //}

                //if (merchantCount > _maxMerchant)
                //{

                //    merchantCount = _maxMerchant;
                //}
                //List<Database.Postgres.RewardsEntities.Products> items = new List<Database.Postgres.RewardsEntities.Products>();

                //int _offset = (int)Math.Floor(offset / (decimal)merchantCount);

                //var _limit = (request.PageNumber - 1) * _offset;

                //Dictionary<int, int> listCount = new Dictionary<int, int>();
                //foreach (var id in merchantIds)
                //{

                //    int count = await queryable.Where(x => x.MerchantId == id).CountAsync();
                //    int reminder = _offset - count;
                //    listCount.Add(id, reminder);
                //}

                //int sum = listCount.Where(x => x.Value > 0).Sum(x => x.Value);
                //foreach (var dict in listCount.OrderByDescending(x => x.Value))
                //{
                //    int rem = listCount.Where(x => x.Key == dict.Key).FirstOrDefault().Value;
                //    if (rem < 0)
                //    {
                //        _offset += sum;
                //        _limit = _limit == 0 ? 0 : _limit + sum;

                //    }

                //    var item = await queryable.Where(x => x.MerchantId == dict.Key).Skip(_limit).Take(_offset).ToListAsync();
                //    items.AddRange(item);
                //}

                var _PIds = PIds.Where(x => x.PageNumber == request.PageNumber).Select(x => x.Id).ToList();
                List<Voupon.Database.Postgres.RewardsEntities.Products> items = new List<Voupon.Database.Postgres.RewardsEntities.Products>();
                var _item = await queryable.Where(x => _PIds.Contains(x.Id)).ToListAsync();
                items.AddRange(_item);

                List<ProductModel> list = new List<ProductModel>();
                foreach (var item in items)
                {

                    ProductModel newItem = new ProductModel();
                    newItem.Id = item.Id;
                    newItem.MerchantId = item.MerchantId;
                    newItem.MerchantCode = item.Merchant.Code;
                    newItem.MerchantName = item.Merchant.DisplayName;
                    newItem.Title = item.Title;
                    newItem.Subtitle = item.Subtitle;
                    newItem.Description = item.Description;
                    newItem.AdditionInfo = item.AdditionInfo;
                    newItem.FinePrintInfo = item.FinePrintInfo;
                    newItem.RedemptionInfo = item.RedemptionInfo;
                    newItem.ImageFolderUrl = new List<string>();
                    newItem.ProductCategoryId = item.ProductCategoryId;
                    newItem.ProductCategory = item.ProductCategory != null ? item.ProductCategory.Name : "";
                    newItem.ProductSubCategoryId = item.ProductSubCategoryId;
                    newItem.ProductSubCategory = item.ProductSubCategory != null ? item.ProductSubCategory.Name : "";
                    newItem.DealTypeId = item.DealTypeId;
                    newItem.DealType = item.DealType != null ? item.DealType.Name : "";
                    newItem.StartDate = item.StartDate;
                    newItem.EndDate = item.EndDate;
                    newItem.Price = item.Price;
                    newItem.DiscountedPrice = item.DiscountedPrice;
                    newItem.DiscountRate = item.DiscountRate;
                    newItem.PointsRequired = item.PointsRequired;
                    newItem.AvailableQuantity = item.AvailableQuantity;
                    newItem.DealExpirationId = item.DealExpirationId;
                    newItem.LuckyDrawId = item.LuckyDrawId;
                    newItem.StatusTypeId = item.StatusTypeId;
                    newItem.StatusType = item.StatusType.Name;
                    newItem.IsActivated = item.IsActivated;
                    newItem.CreatedAt = item.CreatedAt;
                    newItem.CreatedByUserId = item.CreatedByUserId;
                    newItem.IsActivated = item.IsActivated;
                    newItem.LastUpdatedAt = item.LastUpdatedAt;
                    newItem.LastUpdatedByUser = item.LastUpdatedByUser;
                    newItem.TotalBought = item.TotalBought;
                    newItem.Remarks = item.Remarks;
                    newItem.Rating = item.Rating;
                    newItem.IsPublished = item.IsPublished;
                    newItem.ThirdPartyProductId = item.ThirdPartyProductId;
                    newItem.ThirdPartyTypeId = item.ThirdPartyTypeId;
                    newItem.ExternalId = item.ExternalId;
                    newItem.ExternalTypeId = (item.ExternalTypeId.HasValue ? (byte)item.ExternalTypeId.Value : (byte)0);
                    newItem.ExternalMerchantId = item.ExternalMerchantId;
                    if (!item.Merchant.ProvinceId.HasValue || item.Merchant.ProvinceId == null)
                    {
                        newItem.OutletLocation = "Global";

                    }
                    else
                    {
                        var merchantProvince = provinces.Where(x => x.Id == item.Merchant.ProvinceId.Value).FirstOrDefault();
                        if (merchantProvince != null)
                        {
                            newItem.OutletLocation = merchantProvince.Name;
                        }
                        else
                        {
                            newItem.OutletLocation = "Global";
                        }
                    }

                    if (item.DealExpiration != null)
                    {
                        newItem.ExpirationTypeId = item.DealExpiration.ExpirationTypeId;
                    }
                    else
                    {
                        newItem.ExpirationTypeId = 5;
                    }
                    //check for null values
                    if (newItem.Price == null || newItem.DiscountRate == null || newItem.DiscountedPrice == null)
                    {
                        continue;
                    }
                    // Adding the Higher discount value to the discount rate alonge with the difference between price and discounted price
                    if (item.ProductDiscounts != null && item.ProductDiscounts.Count > 0)
                    {
                        var higherDiscount = item.ProductDiscounts.OrderByDescending(x => x.PointRequired).ToList().Where(x => x.IsActivated == true).FirstOrDefault();

                        if (higherDiscount != null)
                        {

                            if (newItem.Price != 0 && newItem.DiscountedPrice != 0)
                            {
                                if (higherDiscount.DiscountTypeId == 1)
                                {
                                    var totalDiscountPrice = ((newItem.DiscountedPrice * higherDiscount.PercentageValue) / 100) + (newItem.Price - newItem.DiscountedPrice);
                                    newItem.DiscountRate = (int)(totalDiscountPrice * 100 / newItem.Price);
                                    newItem.DiscountedPrice = newItem.Price - totalDiscountPrice;
                                }
                                else
                                {
                                    var totalDiscountPrice = (higherDiscount.PriceValue) + (newItem.Price - newItem.DiscountedPrice);
                                    newItem.DiscountRate = (int)(totalDiscountPrice * 100 / newItem.Price);
                                    newItem.DiscountedPrice = newItem.Price - totalDiscountPrice;
                                }
                            }
                        }

                    }

                    newItem.OutletName = item.ProductOutlets.Select(x => x.Outlet.Name).ToList();
                    newItem.OutletProvince = item.ProductOutlets.Select(x => x.Outlet.ProvinceId).ToList();
                    newItem.TotalOutlets = item.ProductOutlets.Count();


                    list.Add(newItem);
                }

                response.Successful = true;
                response.Message = "Get Product List Successfully";
                response.Data = list;
            }
            catch (Exception ex)
            {
                var errorLogs = new Voupon.Database.Postgres.RewardsEntities.ErrorLogs
                {
                    TypeId = CreateErrorLogCommand.Type.Service,
                    ActionName = "ProductListQuery",
                    ActionRequest = JsonConvert.SerializeObject(request),
                    CreatedAt = DateTime.Now,
                    Errors = ex.ToString()
                };

                await rewardsDBContext.ErrorLogs.AddAsync(errorLogs);
                await rewardsDBContext.SaveChangesAsync();

                response.Successful = false;
                response.Message = "Fail to get products";
            }

            return response;
        }
        private List<int> GetListLocation(int[] filterLocationList)
        {
            List<ProductModel> filteredLocationList = new List<ProductModel>();
            List<int> filterLocations = new List<int>();
            foreach (var filter in filterLocationList)
            {
                if (filter == 17)
                {
                    if (!filterLocationList.Contains(15))
                        filterLocations.Add(15);
                    if (!filterLocationList.Contains(11))
                        filterLocations.Add(11);
                    if (!filterLocationList.Contains(10))
                        filterLocations.Add(10);
                }
                if (filter == 18)
                {
                    if (!filterLocationList.Contains(1))
                        filterLocations.Add(1);
                    if (!filterLocationList.Contains(2))
                        filterLocations.Add(2);
                    if (!filterLocationList.Contains(3))
                        filterLocations.Add(3);
                    if (!filterLocationList.Contains(4))
                        filterLocations.Add(4);
                    if (!filterLocationList.Contains(5))
                        filterLocations.Add(5);
                    if (!filterLocationList.Contains(6))
                        filterLocations.Add(6);
                    if (!filterLocationList.Contains(7))
                        filterLocations.Add(7);
                    if (!filterLocationList.Contains(8))
                        filterLocations.Add(8);
                    if (!filterLocationList.Contains(9))
                        filterLocations.Add(9);
                    if (!filterLocationList.Contains(12))
                        filterLocations.Add(12);
                    if (!filterLocationList.Contains(13))
                        filterLocations.Add(13);
                    if (!filterLocationList.Contains(14))
                        filterLocations.Add(14);
                    if (!filterLocationList.Contains(16))
                        filterLocations.Add(16);
                }
                else
                {
                    if (!filterLocations.Contains(filter))
                        filterLocations.Add(filter);
                }
            }
            return filterLocations;
        }
    }

}
