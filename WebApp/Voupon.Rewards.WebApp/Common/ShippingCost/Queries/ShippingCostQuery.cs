using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.Common.Products.Models;
using Voupon.Rewards.WebApp.ViewModels;
using Voupon.Rewards.WebApp.Common.ProductCategories.Models;
using Voupon.Rewards.WebApp.Common.ShippingCost.Models;
using Microsoft.Extensions.Options;

namespace Voupon.Rewards.WebApp.Common.ProductCategories.Queries
{
    public class ShippingCostQuery : IRequest<ApiResponseViewModel>
    {
        public int MasterMemberProfileId { get; set; }
        public int[] ProductIds { get; set; }
        public List<ProductIdAndVariationIdModel> ProductVariationIds { get; set; }
        public int ProvinceId { get; set; }

    }

    public class ShippingCostItemModel 
    {
        public int ProductId;

        public decimal Cost;

        public int Quantity;

        public int MerchantId;

        public bool IsSharingShippingDifferentItem;

        public int? VariationId;
    }
    public class ShippingCostQueryHandler : IRequestHandler<ShippingCostQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        private readonly IOptions<AppSettings> appSettings;
        public ShippingCostQueryHandler(RewardsDBContext rewardsDBContext, IOptions<AppSettings> appSettings)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.appSettings = appSettings;
        }

        public async Task<ApiResponseViewModel> Handle(ShippingCostQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var items = await rewardsDBContext.ShippingCost.AsNoTracking().Include(x => x.ProductShipping).ThenInclude(x => x.Product).ThenInclude(x => x.Merchant).Where(x => x.ProvinceId == request.ProvinceId && request.ProductIds.Contains(x.ProductShipping.ProductId)).ToListAsync();
                var groupedProducts = items.GroupBy(x => x.ProductShipping.Product.MerchantId);
                decimal totalShippingCost = 0;

                OrderShippingCostsModel orderShippingCostsModel = new OrderShippingCostsModel();
                List<OrderShippingCostForPoductIdAndVariationIdModel> orderShippingCosts = new List<OrderShippingCostForPoductIdAndVariationIdModel>();
               
                var cartProduct = rewardsDBContext.CartProducts.Include(x => x.Product).Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && request.ProductIds.Contains(x.Product.Id)).ToList();
                List<ShippingCostItemModel> shippingCostItem = new List<ShippingCostItemModel>();

                foreach (var product in cartProduct) 
                {
                    var shippingObj = items.Where(x => x.ProductShipping.Product.Id == product.ProductId && x.ProductShipping.Product.MerchantId == product.MerchantId).FirstOrDefault();
                    ShippingCostItemModel item = new ShippingCostItemModel();
                    item.ProductId = product.ProductId;
                    item.MerchantId = product.MerchantId;
                    item.Quantity = product.OrderQuantity;
                    item.VariationId = product.VariationId;
                    
                    if(shippingObj != null)
                    {
                        var shippingCost = shippingObj.Cost;
                        item.Cost = shippingCost;
                        if (product.Product.ShareShippingCostSameItem != 0)
                        {
                            item.Cost = product.OrderQuantity <= product.Product.ShareShippingCostSameItem ? shippingCost : shippingCost * Math.Ceiling((decimal)product.OrderQuantity / product.Product.ShareShippingCostSameItem);
                        }

                        if (shippingObj.ProductShipping.ShippingTypeId == 3)
                        {
                            if(item.VariationId != null)
                            {
                                //Old algorithm
                                var variationList = request.ProductVariationIds.Where(x => x.ProductId == product.ProductId).Select(x => x.VariationId).ToList();

                                var cartVariant = cartProduct.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.ProductId == product.ProductId && variationList.Contains(x.VariationId.Value)).ToList();
                                var totalCostForAllQuantity = cartVariant.Sum(x => x.SubTotal);
                                if (totalCostForAllQuantity > 0 && shippingObj.ProductShipping.ConditionalShippingCost < totalCostForAllQuantity)
                                {
                                    item.Cost = 0;
                                }
                            }
                            else
                            {
                                if(product.Product.ShareShippingCostSameItem > 0)
                                {
                                    item.Cost = product.OrderQuantity <= product.Product.ShareShippingCostSameItem ? shippingCost : shippingCost * Math.Ceiling((decimal)product.OrderQuantity / product.Product.ShareShippingCostSameItem);
                                }

                                else
                                {
                                    //Bug fix - Shipping cost for products without variations

                                    var cartVariant = cartProduct.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.ProductId == product.ProductId);
                                    var totalCostForAllQuantity = cartVariant.Sum(x => x.SubTotal);
                                    if (totalCostForAllQuantity > 0 && shippingObj.ProductShipping.ConditionalShippingCost < totalCostForAllQuantity)
                                    {
                                        item.Cost = 0;
                                    }
                                }



                                //New Shipping cost calculation
                            }
                            
                        }
                        item.IsSharingShippingDifferentItem = product.Product.IsShareShippingDifferentItem;
                    }
                    else
                    {
                        item.Cost = 0;
                    }
                    
                    shippingCostItem.Add(item);
                }


               // var cartProductExternal = rewardsDBContext.CartProductExternal.Include(x => x.Product).Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && request.ProductIds.Contains(x.Product.Id)).ToList();


                //Extenal Products shipping cost calcualtion


                var groupedByMerchant = shippingCostItem.GroupBy(x => x.MerchantId);

                foreach (var itemsMerchant in groupedByMerchant) {
                   
                    var countItem = itemsMerchant.Count();
                    decimal maxShippingCost = 0;

                    
                    if (countItem > 1)
                    {
                        foreach (var item in itemsMerchant)
                        {     
                            if (!item.IsSharingShippingDifferentItem)
                            {
                                maxShippingCost += item.Cost;
                            }
                            else 
                            {
                                maxShippingCost = Math.Max(maxShippingCost, item.Cost);
                            }
                            
                        }
                    }else
                    {
                        maxShippingCost = itemsMerchant.FirstOrDefault().Cost;
                    }
                    OrderShippingCostForPoductIdAndVariationIdModel orderShippingCost = new OrderShippingCostForPoductIdAndVariationIdModel()
                    {
                        ProductId = itemsMerchant.First().ProductId,
                        VariationId = (itemsMerchant.First().VariationId.HasValue ? itemsMerchant.First().VariationId.Value : 0),
                        OrderShippingCost = Math.Max(maxShippingCost, 0)
                    };
                    orderShippingCosts.Add(orderShippingCost);
                    totalShippingCost += maxShippingCost;
                }

                /*foreach (var merchantProducts in groupedProducts)
                {
                    var merchantProductIdWithShippingCost = 0;
                    var merchantProductVarationIdWithShippingCost = 0;
                    var currentMerchantId = merchantProducts.Key;
                    decimal maxShippingCost = 0;

                    var count = merchantProducts.Count();
                    if (count > 1) 
                    {
                        foreach (var Product in merchantProducts)
                        {
                            if (Product.Cost > maxShippingCost)
                            {
                                var share = Product.ProductShipping.Product.ShareShippingCostSameItem;
                                if (Product.ProductShipping.ShippingTypeId == 3)
                                {
                                    var variationList = request.ProductVariationIds.Where(x => x.ProductId == Product.ProductShipping.ProductId).Select(x => x.VariationId).ToList();
                                    var cartProduct = rewardsDBContext.CartProducts.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.ProductId == Product.ProductShipping.ProductId).FirstOrDefault();

                                    if (cartProduct != null)
                                    {
                                        if (cartProduct.IsVariationProduct && variationList.Count > 0)
                                        {
                                            var cartProducts = rewardsDBContext.CartProducts.Where(x => x.MasterMemberProfileId == request.MasterMemberProfileId && x.ProductId == Product.ProductShipping.ProductId && variationList.Contains(x.VariationId.Value)).ToList();
                                            var totalCostForAllQuantity = cartProducts.Sum(x => x.SubTotal);
                                            if (totalCostForAllQuantity > 0 && Product.ProductShipping.ConditionalShippingCost < totalCostForAllQuantity)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            var totalCostForAllQuantity = cartProduct.SubTotal;
                                            if (totalCostForAllQuantity > 0 && Product.ProductShipping.ConditionalShippingCost < totalCostForAllQuantity)
                                            {
                                                continue;
                                            }
                                        }
                                    }
                        
                                maxShippingCost = Product.Cost;
                                merchantProductIdWithShippingCost = Product.ProductShipping.ProductId;
                                merchantProductVarationIdWithShippingCost = request.ProductVariationIds.Where(x => x.ProductId == Product.ProductShipping.ProductId).Select(x => x.VariationId).FirstOrDefault();
                                }
                            }
                    }


                    OrderShippingCostForPoductIdAndVariationIdModel orderShippingCost = new OrderShippingCostForPoductIdAndVariationIdModel()
                    {
                        ProductId = merchantProductIdWithShippingCost,
                        VariationId = merchantProductVarationIdWithShippingCost,
                        OrderShippingCost = Math.Max(maxShippingCost, 0)
                    };
                    orderShippingCosts.Add(orderShippingCost);
                    totalShippingCost += orderShippingCost.OrderShippingCost;
                }*/

                //  Add external item shipping costs
                orderShippingCostsModel.OrderShippingCosts = orderShippingCosts;
                orderShippingCostsModel.TotalShippingCost = totalShippingCost;

                response.Successful = true;
                response.Message = "Get Product Category List Successfully";
                response.Data = orderShippingCostsModel;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
