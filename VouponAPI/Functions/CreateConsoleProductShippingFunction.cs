using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Voupon.Common.Azure.Blob;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.API.ViewModels;
using Voupon.API.Util;
using Newtonsoft.Json.Converters;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace Voupon.API.Functions.Blog
{
    public class CreateConsoleProductShippingFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;

        public CreateConsoleProductShippingFunction(IHttpClientFactory httpClientFactory, RewardsDBContext rewardsDBContext)
        {
            _rewardsDBContext = rewardsDBContext;
        }

        [FunctionName("CreateConsoleProductShippingFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "console/product/shipping")] HttpRequest req, ILogger log)
        {
            var response = new ApiResponseViewModel
            {
            };

            try
            {
                var request = HttpRequestHelper.DeserializeModel<ProductShippingRequestModel>(req);
                const int CHECK_COUNT = 10;
                var ADMIN_USER_ID = Guid.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.ADMIN_USER_ID));

                for (var check = 0; check < CHECK_COUNT; check++)
                {
                    var product = await _rewardsDBContext.Products.Where(x => x.ExternalId == request.ExternalId && x.ExternalTypeId == request.ExternalTypeId).FirstOrDefaultAsync();
                    if (product != null)
                    {
                        var provinces = await _rewardsDBContext.Provinces.ToListAsync();

                        var shipping = await _rewardsDBContext.ProductShippingCost.Where(x => x.ProductId == product.Id).FirstOrDefaultAsync();
                        if (shipping != null)
                        {
                            var shippingCost = await _rewardsDBContext.ShippingCost.Where(x => x.ProductShippingId == shipping.Id).ToListAsync();
                            if (shippingCost != null && shippingCost.Any())
                            {
                                _rewardsDBContext.ShippingCost.RemoveRange(shippingCost);
                            }

                            _rewardsDBContext.ProductShippingCost.Remove(shipping);
                            await _rewardsDBContext.SaveChangesAsync();
                        }


                        //Type 2 = fixed shipping cost
                        var newProductShippingCost = new ProductShippingCost
                        {
                            ConditionalShippingCost = 0,
                            CreatedAt = DateTime.Now,
                            CreatedByUserId = ADMIN_USER_ID,
                            ProductId = product.Id,
                            ShippingTypeId = 2,
                            ShippingCost = new List<ShippingCost>()
                        };


                        foreach (var province in provinces)
                        {
                            //Shopee pricing is in cents. Need to divide by 100000
                            newProductShippingCost.ShippingCost.Add(new ShippingCost
                            {
                                ProvinceId = province.Id,
                                Cost = request.ShippingCost / 100000,
                                CreatedAt = DateTime.Now,
                                CreatedByUserId = ADMIN_USER_ID,
                            });
                        }

                        await _rewardsDBContext.ProductShippingCost.AddAsync(newProductShippingCost);
                        await _rewardsDBContext.SaveChangesAsync();
                        break;
                    }

                    System.Threading.Thread.Sleep(1000);
                }

                response.Code = 0;

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                response.ErrorMessage = "Fail to create product shipping json" + ex.ToString();
                return new BadRequestObjectResult(response);

            }
        }

        private class ProductShippingRequestModel
        {
            public Guid Id { get; set; }
            public string PageUrl { get; set; }
            public string ItemName { get; set; }
            public string URL { get; set; }
            public string ExternalMerchantId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalId { get; set; }
            public byte StatusId { get; set; }
            public string JSON { get; set; }
            public decimal ShippingCost { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime LastUpdatedAt { get; set; }

        }

    }
}
