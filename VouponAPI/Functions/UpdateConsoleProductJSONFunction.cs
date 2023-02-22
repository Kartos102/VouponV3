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
using static Voupon.API.Functions.Blog.CreateConsoleMerchantJSONFunction;
using Microsoft.CodeAnalysis.Options;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;
using System.Drawing.Imaging;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Voupon.API.Functions.Blog
{
    public class UpdateConsoleProductJSONFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly IAzureBlobStorage _azureBlobStorage;

        public UpdateConsoleProductJSONFunction(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            _rewardsDBContext = rewardsDBContext;
            _azureBlobStorage = azureBlobStorage;
        }

        [FunctionName("UpdateConsoleProductJSONFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "console/product-json")] HttpRequest req, ILogger log)
        {
            var response = new ApiResponseViewModel
            {
            };

            await _rewardsDBContext.Database.BeginTransactionAsync();

            try
            {
                var request = HttpRequestHelper.DeserializeModel<UpdateProductRequestModel>(req);

                var existingProductJSON = await _rewardsDBContext.ConsoleProductJSON.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (existingProductJSON == null)
                {
                    response.ErrorMessage = "Invalid product";
                    return new BadRequestObjectResult(response);

                }
                else

                if (!string.IsNullOrEmpty(request.JSON))
                {
                    existingProductJSON.JSON = request.JSON;
                }


                existingProductJSON.StatusId = 2;
                existingProductJSON.LastUpdatedAt = DateTime.Now;
                _rewardsDBContext.ConsoleProductJSON.Update(existingProductJSON);
                await _rewardsDBContext.SaveChangesAsync();

                var merchant = await _rewardsDBContext.Merchants.Where(x => x.IsExternal == true && x.ExternalId == existingProductJSON.ExternalMerchantId).FirstOrDefaultAsync();

                if (merchant == null)
                {
                    response.ErrorMessage = "Product not created. Merchant is not created yet";
                    return new BadRequestObjectResult(response);
                }

                var productResult = JsonConvert.DeserializeObject<ShopeeProductModel>(request.JSON);

                var existingProduct = await _rewardsDBContext.Products.Where(x => x.IsExternal == true && x.ExternalId == existingProductJSON.ExternalId).FirstOrDefaultAsync();

                var createdByUserId = Guid.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.ADMIN_USER_ID));

                var productCategory = 1;
                var productSubCategory = 1;

                var productCategoryList = await _rewardsDBContext.ProductCategories.ToListAsync();
                var productSubCategoryList = await _rewardsDBContext.ProductSubCategories.ToListAsync();


                var shopeeProductCategory = productResult.Data.Categories.First().DisplayName.Trim().ToLower();

                if (shopeeProductCategory == "automotive")
                {
                    productCategory = 3;
                }
                else if (shopeeProductCategory == "baby & toys")
                {
                    productCategory = 4;
                }
                else if (shopeeProductCategory == "camera & drones")
                {
                    productCategory = 3;
                }
                else if (shopeeProductCategory == "computer & accessories")
                {
                    productCategory = 3;
                }
                else if (shopeeProductCategory == "fashion Acessories")
                {
                    productCategory = 1;
                }
                else if (shopeeProductCategory == "games, books & hobbies")
                {
                    productCategory = 5;
                }
                else if (shopeeProductCategory == "gaming & consoles")
                {
                    productCategory = 5;
                }
                else if (shopeeProductCategory == "groceries & pets")
                {
                    productCategory = 6;
                }
                else if (shopeeProductCategory == "health & beauty")
                {
                    productCategory = 6;

                }
                else if (shopeeProductCategory == "home & living")
                {
                    productCategory = 4;
                }
                else if (shopeeProductCategory == "home appliances")
                {
                    productCategory = 4;
                }
                else if (shopeeProductCategory == "men clothes")
                {
                    productCategory = 2;
                }
                else if (shopeeProductCategory == "men shoes")
                {
                    productCategory = 2;
                }
                else if (shopeeProductCategory == "men's bag & wallet")
                {
                    productCategory = 2;
                }
                else if (shopeeProductCategory == "mobile & accessories")
                {
                    productCategory = 3;
                }
                else if (shopeeProductCategory == "muslim fashion")
                {
                    productCategory = 1;
                }
                else if (shopeeProductCategory == "others")
                {
                    productCategory = 8;
                }
                else if (shopeeProductCategory == "sports & outdoor")
                {
                    productCategory = 7;
                }
                else if (shopeeProductCategory == "tickets & vouchers")
                {
                    productCategory = 8;
                }
                else if (shopeeProductCategory == "travel & luggage")
                {
                    productCategory = 4;
                }
                else if (shopeeProductCategory == "watches")
                {
                    productCategory = 3;
                }
                else if (shopeeProductCategory == "women clothes")
                {
                    productCategory = 1;
                }
                else if (shopeeProductCategory == "women shoes")
                {
                    productCategory = 1;
                }
                else if (shopeeProductCategory == "women's bag")
                {
                    productCategory = 1;
                }
                else
                {
                    productCategory = 8;
                }

                var category = productCategoryList.Where(x => x.Name.ToLower() == productResult.Data.Categories.First().DisplayName.ToLower().Replace("&", "and")).FirstOrDefault();
                if (category != null)
                {
                    productCategory = category.Id;
                }

                if (productResult.Data.Categories.Count() > 1)
                {
                    var productSubCategoryItem = productSubCategoryList.Where(x => x.Name.ToLower() == productResult.Data.Categories.ElementAt(1).DisplayName.Replace("&", "and").ToLower()).FirstOrDefault();

                    if (productSubCategoryItem == null)
                    {
                        var subCategory = new ProductSubCategories
                        {
                            Name = productResult.Data.Categories.ElementAt(1).DisplayName.Replace("&", "and").ToLower(),
                            IsActivated = true,
                            CategoryId = productCategory,
                            CreatedAt = DateTime.Now,
                            CreatedByUserId = Guid.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.ADMIN_USER_ID))
                        };
                        await _rewardsDBContext.ProductSubCategories.AddAsync(subCategory);
                        await _rewardsDBContext.SaveChangesAsync();

                        productSubCategory = subCategory.Id;

                    }
                    else
                    {
                        productSubCategory = productSubCategoryItem.Id;
                    }
                }


                var variations = productResult.Data.TierVariations;

                var product = new Voupon.Database.Postgres.RewardsEntities.Products();

                //  Handle updates?
                if (existingProduct != null)
                {

                    existingProduct.Description = productResult.Data.Description;
                    existingProduct.Title = productResult.Data.Name;
                    existingProduct.Price = (decimal)(productResult.Data.Price * 0.00001);
                    existingProduct.DiscountedPrice = (decimal)(productResult.Data.Price * 0.00001);
                    existingProduct.AvailableQuantity = (productResult.Data.Stock.HasValue ? (int)productResult.Data.Stock : 0);
                    existingProduct.ProductCategoryId = productCategory;
                    existingProduct.ProductSubCategoryId = productSubCategory;
                    existingProduct.IsProductVariationEnabled = (variations != null && variations.Count() > 0 ? true : false);


                    _rewardsDBContext.Products.Update(existingProduct);
                    await _rewardsDBContext.SaveChangesAsync();

                    product = existingProduct;
                }
                else
                {
                    product = new Voupon.Database.Postgres.RewardsEntities.Products
                    {
                        MerchantId = merchant.Id,
                        Description = productResult.Data.Description,
                        IsPublished = true,
                        Title = productResult.Data.Name,
                        Price = (decimal)(productResult.Data.Price * 0.00001),
                        DiscountedPrice = (decimal)(productResult.Data.Price * 0.00001),
                        CreatedAt = DateTime.Now,
                        CreatedByUserId = createdByUserId,
                        AvailableQuantity = (productResult.Data.Stock.HasValue ? (int)productResult.Data.Stock : 0),
                        Rating = (productResult.Data.ItemRating.RatingStar.HasValue ? (decimal)productResult.Data.ItemRating.RatingStar.Value : 0),
                        DefaultCommission = 0,
                        StatusTypeId = 4,
                        DealTypeId = 2,
                        IsActivated = true,
                        ProductCategoryId = productCategory,
                        ProductSubCategoryId = productSubCategory,
                        StartDate = DateTime.Now.AddDays(-1),
                        EndDate = DateTime.Now.AddYears(5),
                        IsExternal = true,
                        ExternalId = existingProductJSON.ExternalId,
                        ExternalTypeId = 1,
                        ExternalMerchantId = existingProductJSON.ExternalMerchantId,
                        ExternalUrl = existingProductJSON.PageUrl,
                        DiscountRate = 0,
                        PointsRequired = 0,
                        IsProductVariationEnabled = (variations != null && variations.Count() > 0 ? true : false)

                    };

                    await _rewardsDBContext.Products.AddAsync(product);
                    await _rewardsDBContext.SaveChangesAsync();
                }

                //  Update merchant province

                if (!merchant.ProvinceId.HasValue || merchant.ProvinceId == 0)
                {
                    if (!string.IsNullOrEmpty(productResult.Data.ShopLocation))
                    {
                        var provinces = await _rewardsDBContext.Provinces.AsNoTracking().ToListAsync();

                        if (provinces != null && provinces.Count() > 0)
                        {
                            var itemProvince = provinces.Where(x => x.Name.ToLower() == productResult.Data.ShopLocation.ToLower()).FirstOrDefault();

                            if (itemProvince != null)
                            {

                                merchant.ProvinceId = itemProvince.Id;
                                _rewardsDBContext.Merchants.Update(merchant);
                                await _rewardsDBContext.SaveChangesAsync();
                            }
                        }
                    }
                }

                //  Handle variation if any
                var variationList = new List<Variations>();


                if (variations != null)
                {
                    var variationOptionList = new List<VariationOptions>();

                    var isVariationHasChanges = false;

                    //  Check if variation is different. If different, recreate them

                    var newVariationName = string.Join(",", variations.Select(x => x.Name.ToLower()));
                    var newVariationOptions = string.Join(",", variations.SelectMany(x => x.Options).Select(z => z.ToLower().ToString()));

                    var checkExistingVariation = await _rewardsDBContext.Variations.AsNoTracking().Where(x => x.ProductId == product.Id).ToListAsync();
                    var checkExistingVariationOptions = await _rewardsDBContext.VariationOptions.AsNoTracking().Where(x => checkExistingVariation.Select(x => x.Id).Contains(x.VariationId)).ToListAsync();

                    if (newVariationName == string.Join(",", checkExistingVariation.Select(x => x.Name.ToLower())))
                    {
                        if (checkExistingVariationOptions != null)
                        {

                            if (newVariationOptions != string.Join(",", checkExistingVariationOptions.Select(x => x.Name.ToLower())))
                            {
                                isVariationHasChanges = true;
                            }
                        }
                        else
                        {
                            isVariationHasChanges = true;
                        }
                    }
                    else
                    {
                        isVariationHasChanges = true;
                    }

                    if (isVariationHasChanges)
                    {
                        //  Delete any existing variations
                        var existingProductVariation = await _rewardsDBContext.ProductVariation.Where(x => x.ProductId == product.Id).ToListAsync();

                        var existingVariation = await _rewardsDBContext.Variations.Where(x => x.ProductId == product.Id).ToListAsync();

                        var existingVariationOptions = await _rewardsDBContext.VariationOptions.Where(x => existingVariation.Select(x => x.Id).Contains(x.VariationId)).ToListAsync();

                        var existingVaritionId = existingProductVariation.Select(x => x.Id).ToList();

                        var existingCombination = await _rewardsDBContext.VariationCombination.Where(x => existingVaritionId.Contains(x.OptionOneId)).ToListAsync();

                        if (existingCombination != null && existingCombination.Count() > 0)
                        {
                            _rewardsDBContext.RemoveRange(existingCombination);
                        }

                        if (existingProductVariation != null && existingProductVariation.Count() > 0)
                        {
                            _rewardsDBContext.RemoveRange(existingProductVariation);
                        }

                        if (existingVariationOptions != null && existingVariationOptions.Count() > 0)
                        {
                            _rewardsDBContext.RemoveRange(existingVariationOptions);
                        }
                        if (existingVariation != null && existingVariation.Count() > 0)
                        {
                            _rewardsDBContext.RemoveRange(existingVariation);
                        }

                        var isFirstVariation = true;
                        var hasVariation = false;
                        foreach (var item in variations)
                        {
                            var variation = new Variations
                            {
                                Name = item.Name,
                                CreatedAt = DateTime.Now,
                                CreatedByUserId = createdByUserId,
                                IsFirstVariation = isFirstVariation,
                                VariationOptions = new List<VariationOptions>(),
                                ProductId = product.Id,
                            };
                            var orderNumber = 1;
                            for (var option = 0; option < item.Options.Count(); option++)
                            {
                                var newImageUrl = "";
                                if (item.Images != null)
                                {
                                    if (item.Images[option] != null)
                                    {
                                        using (System.Net.WebClient webClient = new System.Net.WebClient())
                                        {
                                            byte[] imageByte = webClient.DownloadData($"https://cf.shopee.com.my/file/{item.Images[option]}");
                                            Stream stream = new MemoryStream(imageByte);
                                            var contentType = "jpeg";
                                            var name = "";
                                            name = Guid.NewGuid().ToString() + "." + contentType;
                                            stream.Position = 0;
                                            var blob = await _azureBlobStorage.UploadBlobAsync(stream, product.Id + "/" +
                                               FilePathEnum.Products_Variation_Images + "/" + "normal_" + name, contentType,
                                              ContainerNameEnum.Products, true);

                                            newImageUrl = blob.StorageUri.PrimaryUri.ToString();
                                        }
                                    }
                                }

                                variation.VariationOptions.Add(new VariationOptions
                                {
                                    Name = item.Options[option],
                                    ImageUrl = newImageUrl,
                                    CreatedAt = DateTime.Now,
                                    CreatedBy = createdByUserId,
                                    Order = orderNumber
                                });
                                orderNumber++;
                            }

                            isFirstVariation = false;
                            variationList.Add(variation);
                            hasVariation = true;

                        }

                        if (hasVariation)
                        {
                            await _rewardsDBContext.Variations.AddRangeAsync(variationList);
                            await _rewardsDBContext.SaveChangesAsync();
                        }

                        var firstVariation = variationList.Where(x => x.IsFirstVariation == true).SelectMany(x => x.VariationOptions);
                        var secondVariation = variationList.Where(x => x.IsFirstVariation == false).SelectMany(x => x.VariationOptions);

                        if (firstVariation != null && firstVariation.Any())
                        {
                            var variationCombinationList = new List<VariationCombination>();
                            if (secondVariation != null && secondVariation.Any())
                            {
                                foreach (var first in firstVariation)
                                {
                                    foreach (var second in secondVariation)
                                    {
                                        var newVariationCombination = new VariationCombination
                                        {
                                            OptionOneId = first.Id,
                                            OptionTwoId = second.Id,
                                            CreatedAt = DateTime.Now,
                                            CreatedByUserId = createdByUserId
                                        };
                                        variationCombinationList.Add(newVariationCombination);

                                    }
                                }
                            }
                            else
                            {
                                foreach (var first in firstVariation)
                                {
                                    var newVariationCombination = new VariationCombination
                                    {
                                        OptionOneId = first.Id,
                                        CreatedAt = DateTime.Now,
                                        CreatedByUserId = createdByUserId
                                    };
                                    variationCombinationList.Add(newVariationCombination);
                                }
                            }

                            await _rewardsDBContext.VariationCombination.AddRangeAsync(variationCombinationList);
                            await _rewardsDBContext.SaveChangesAsync();

                            if (productResult.Data.Models != null && productResult.Data.Models.Any())
                            {
                                foreach (var model in productResult.Data.Models)
                                {
                                    var tiers = model.Extinfo.TierIndex.Count();
                                    if (tiers == 1)
                                    {
                                        var tierVariation = firstVariation.ElementAt((int)model.Extinfo.TierIndex.ElementAt(0));

                                        var option = variationCombinationList.Where(x => x.OptionOneId == tierVariation.Id).FirstOrDefault();

                                        var productVariation = new ProductVariation
                                        {
                                            ProductId = product.Id,
                                            Price = (decimal)(model.Price * 0.00001),
                                            DiscountedPrice = (decimal)(model.Price * 0.00001),
                                            IsDiscountedPriceEnabled = false,
                                            VariationCombinationId = option.Id,
                                            SKU = tierVariation.Name,
                                            AvailableQuantity = (int)model.Stock.Value,
                                            CreatedAt = DateTime.Now,
                                            CreatedByUserId = createdByUserId
                                        };
                                        await _rewardsDBContext.ProductVariation.AddAsync(productVariation);

                                    }
                                    else
                                    {
                                        var tierVariation = firstVariation.ElementAt((int)model.Extinfo.TierIndex.ElementAt(0));
                                        var secondTierVariation = secondVariation.ElementAt((int)model.Extinfo.TierIndex.ElementAt(1));

                                        var option = variationCombinationList.Where(x => x.OptionOneId == tierVariation.Id && x.OptionTwoId == secondTierVariation.Id).FirstOrDefault();

                                        var productVariation = new ProductVariation
                                        {
                                            ProductId = product.Id,
                                            Price = (decimal)(model.Price * 0.00001),
                                            DiscountedPrice = (decimal)(model.Price * 0.00001),
                                            IsDiscountedPriceEnabled = false,
                                            VariationCombinationId = option.Id,
                                            SKU = tierVariation.Name,
                                            AvailableQuantity = (int)model.Stock.Value,
                                            CreatedAt = DateTime.Now,
                                            CreatedByUserId = createdByUserId
                                        };
                                        await _rewardsDBContext.ProductVariation.AddAsync(productVariation);
                                    }
                                }
                            }
                            await _rewardsDBContext.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    //  Delete any existing variations
                    var existingProductVariation = await _rewardsDBContext.ProductVariation.Where(x => x.ProductId == product.Id).ToListAsync();

                    var existingVariation = await _rewardsDBContext.Variations.Where(x => x.ProductId == product.Id).ToListAsync();

                    var existingVariationOptions = _rewardsDBContext.VariationOptions.Where(x => existingVariation.Select(x => x.Id).Contains(x.VariationId)).ToListAsync();

                    var existingVaritionId = existingProductVariation.Select(x => x.Id).ToList();

                    var existingCombination = await _rewardsDBContext.VariationCombination.Where(x => existingVaritionId.Contains(x.OptionOneId)).ToListAsync();

                    _rewardsDBContext.RemoveRange(existingCombination);
                    _rewardsDBContext.RemoveRange(existingProductVariation);
                    _rewardsDBContext.RemoveRange(existingVariationOptions);
                    _rewardsDBContext.RemoveRange(existingVariation);
                }

                _rewardsDBContext.Database.CommitTransaction();

                //  Handle images
                var images = productResult.Data.Images;

                // delete existing images
                _azureBlobStorage.DeleteContainerFiles(ContainerNameEnum.Products, product.Id + "/" + FilePathEnum.Products_Images);

                if (images != null && images.Count() > 0)
                {
                    int smallImageHeight = Int16.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.PRODUCT_IMAGE.SIZE.SMALL.HEIGHT));
                    int normalImageHeight = Int16.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.PRODUCT_IMAGE.SIZE.NORMAL.HEIGHT));
                    int bigImageHeight = Int16.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.PRODUCT_IMAGE.SIZE.BIG.HEIGHT));

                    int smallImageWidth = Int16.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.PRODUCT_IMAGE.SIZE.SMALL.WIDTH));
                    int normalImageWidth = Int16.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.PRODUCT_IMAGE.SIZE.NORMAL.WIDTH));
                    int bigImageWidth = Int16.Parse(Environment.GetEnvironmentVariable(EnvironmentKey.PRODUCT_IMAGE.SIZE.BIG.WIDTH));


                    var count = 0;
                    foreach (var image in images)
                    {
                        using (System.Net.WebClient webClient = new System.Net.WebClient())
                        {
                            byte[] imageByte = webClient.DownloadData($"https://cf.shopee.com.my/file/{image}");
                            Stream stream = new MemoryStream(imageByte);
                            var contentType = "jpeg";
                            var name = "";
                            name = count + "_" + Guid.NewGuid().ToString() + "." + contentType;

                            var result = await AddImageWithSizeWithRatio(stream, name, product.Id, FilePathEnum.Products_Images, contentType, ContainerNameEnum.Products, "small_", smallImageWidth, smallImageHeight);


                            result = await AddImageWithSizeWithRatio(stream, name, product.Id, FilePathEnum.Products_Images, contentType, ContainerNameEnum.Products, "normal_", normalImageWidth, normalImageHeight);


                            result = await AddImageWithSizeWithRatio(stream, name, product.Id, FilePathEnum.Products_Images, contentType, ContainerNameEnum.Products, "big_", bigImageWidth, bigImageHeight);

                            stream.Position = 0;
                            var blob = await _azureBlobStorage.UploadBlobAsync(stream, product.Id + "/" +
                               FilePathEnum.Products_Temporary_Images + "/" + "org_" + name, contentType,
                              ContainerNameEnum.Products, true);

                            count++;
                            //folderUrl = blob.StorageUri.PrimaryUri.OriginalString.Replace(filename, "");
                        }
                    }
                }


                // delete existing description images
                _azureBlobStorage.DeleteContainerFiles(ContainerNameEnum.Products, product.Id + "/" + FilePathEnum.PRODUCT_DESCRIPTION_IMAGES);

                //  Get rich text/poster images
                var imageUrl = "";
                var description = productResult.Data.Description;
                if (productResult.Data.RichTextDescription != null)
                {
                    if (productResult.Data.RichTextDescription.ParagraphList != null && productResult.Data.RichTextDescription.ParagraphList.Length > 0)
                    {
                        var paraLength = productResult.Data.RichTextDescription.ParagraphList.Length;
                        for (var item = 0; item < paraLength; item++)
                        {
                            if (string.IsNullOrEmpty(productResult.Data.RichTextDescription.ParagraphList[item].ImgId))
                            {
                                continue;
                            }
                            using (System.Net.WebClient webClient = new System.Net.WebClient())
                            {
                                byte[] imageByte = webClient.DownloadData($"https://cf.shopee.com.my/file/{productResult.Data.RichTextDescription.ParagraphList[item].ImgId}");
                                Stream stream = new MemoryStream(imageByte);
                                var contentType = "jpeg";
                                stream.Position = 0;
                                var blob = await _azureBlobStorage.UploadBlobAsync(stream, product.Id + "/" +
                                   FilePathEnum.PRODUCT_DESCRIPTION_IMAGES + "/" + productResult.Data.RichTextDescription.ParagraphList[item].ImgId, contentType,
                                  ContainerNameEnum.Products, true);
                                imageUrl += "<br/>";
                                imageUrl += "<img src='" + blob.StorageUri.PrimaryUri.ToString() + "'/>";
                            }
                        }

                    }
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        product.Description = product.Description + imageUrl;
                        _rewardsDBContext.Products.Update(product);
                        await _rewardsDBContext.SaveChangesAsync();
                    }

                }
                response.Code = 0;
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _rewardsDBContext.Database.RollbackTransaction();
                //  log
                response.ErrorMessage = "Fail to create product json" + ex.ToString();
                return new BadRequestObjectResult(response);

            }
        }

        private async Task<bool> AddImageWithSizeWithRatio(Stream file, string name, int id, string filePath, string contentType, string containerName, string imageSizeName, int width, int height)
        {
            string folderUrl = "";
            using (MemoryStream ms = new MemoryStream())
            {

                try
                {
                    file.Position = 0;
                    using (var image = Image.Load(file, out IImageFormat format))
                    {

                        var imageRatio = (float)image.Width / image.Height;
                        int newHeight = (int)(width / imageRatio);
                        if (newHeight > height)
                        {
                            var newWidth = (int)(height * imageRatio);
                            image.Mutate(x => x.Resize(newWidth, height));

                        }
                        else
                        {
                            image.Mutate(x => x.Resize(width, newHeight));
                        }
                        byte[] fileData;

                        image.Save(ms, format);
                        fileData = ms.ToArray();

                        var filename = imageSizeName + name;
                        ms.Position = 0;
                        var blob = await _azureBlobStorage.UploadBlobAsync(ms, id + "/" +
                            filePath + "/" + filename, contentType,
                          containerName, true);
                        if (blob == null)
                        {
                            return false;
                        }
                        folderUrl = blob.StorageUri.PrimaryUri.OriginalString.Replace(filename, "");
                    }

                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }

        }

        private async Task<bool> AddImageWithSize(Stream file, string name, int id, string filePath, string contentType, string containerName, string imageSizeName, int width, int height)
        {
            string folderUrl = "";
            using (MemoryStream ms = new MemoryStream())
            {

                try
                {
                    using (var image = Image.Load(file, out IImageFormat format))
                    {


                        image.Mutate(x => x.Resize(width, height));
                        byte[] fileData;

                        image.Save(ms, format);
                        fileData = ms.ToArray();

                        var filename = imageSizeName + name;
                        ms.Position = 0;
                        var blob = await _azureBlobStorage.UploadBlobAsync(ms, id + "/" +
                            filePath + "/" + filename, contentType,
                          containerName, true);
                        if (blob == null)
                        {
                            return false;
                        }
                        folderUrl = blob.StorageUri.PrimaryUri.OriginalString.Replace(filename, "");
                    }

                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }

        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }
        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformats officedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

        private class UpdateProductRequestModel
        {
            public Guid Id { get; set; }
            public byte StatusId { get; set; }
            public string JSON { get; set; }

        }

        public class ShopeeProductModel
        {
            [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
            public ProductData Data { get; set; }

            [JsonProperty("error")]
            public object Error { get; set; }

            [JsonProperty("error_msg")]
            public object ErrorMsg { get; set; }
        }
        public partial class ProductData
        {
            [JsonProperty("itemid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Itemid { get; set; }

            [JsonProperty("shopid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Shopid { get; set; }

            [JsonProperty("userid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Userid { get; set; }

            [JsonProperty("price_max_before_discount", NullValueHandling = NullValueHandling.Ignore)]
            public long? PriceMaxBeforeDiscount { get; set; }

            [JsonProperty("has_lowest_price_guarantee", NullValueHandling = NullValueHandling.Ignore)]
            public bool? HasLowestPriceGuarantee { get; set; }

            [JsonProperty("price_before_discount", NullValueHandling = NullValueHandling.Ignore)]
            public long? PriceBeforeDiscount { get; set; }

            [JsonProperty("price_min_before_discount", NullValueHandling = NullValueHandling.Ignore)]
            public long? PriceMinBeforeDiscount { get; set; }

            [JsonProperty("exclusive_price_info")]
            public object ExclusivePriceInfo { get; set; }

            [JsonProperty("hidden_price_display")]
            public object HiddenPriceDisplay { get; set; }

            [JsonProperty("price_min", NullValueHandling = NullValueHandling.Ignore)]
            public long? PriceMin { get; set; }

            [JsonProperty("price_max", NullValueHandling = NullValueHandling.Ignore)]
            public long? PriceMax { get; set; }

            [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
            public long? Price { get; set; }

            [JsonProperty("stock", NullValueHandling = NullValueHandling.Ignore)]
            public long? Stock { get; set; }

            [JsonProperty("discount", NullValueHandling = NullValueHandling.Ignore)]
            public string Discount { get; set; }

            [JsonProperty("historical_sold", NullValueHandling = NullValueHandling.Ignore)]
            public long? HistoricalSold { get; set; }

            [JsonProperty("sold", NullValueHandling = NullValueHandling.Ignore)]
            public long? Sold { get; set; }

            [JsonProperty("show_discount", NullValueHandling = NullValueHandling.Ignore)]
            public long? ShowDiscount { get; set; }

            [JsonProperty("raw_discount", NullValueHandling = NullValueHandling.Ignore)]
            public long? RawDiscount { get; set; }

            [JsonProperty("min_purchase_limit", NullValueHandling = NullValueHandling.Ignore)]
            public long? MinPurchaseLimit { get; set; }

            [JsonProperty("overall_purchase_limit", NullValueHandling = NullValueHandling.Ignore)]
            public OverallPurchaseLimit OverallPurchaseLimit { get; set; }

            [JsonProperty("pack_size")]
            public object PackSize { get; set; }

            [JsonProperty("is_live_streaming_price")]
            public object IsLiveStreamingPrice { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("ctime", NullValueHandling = NullValueHandling.Ignore)]
            public long? Ctime { get; set; }

            [JsonProperty("item_status", NullValueHandling = NullValueHandling.Ignore)]
            public string ItemStatus { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public long? Status { get; set; }

            [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
            public long? Condition { get; set; }

            [JsonProperty("catid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Catid { get; set; }

            [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
            public string Description { get; set; }

            [JsonProperty("is_mart", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsMart { get; set; }

            [JsonProperty("rich_text_description")]
            public RichTextDescription RichTextDescription { get; set; }

            [JsonProperty("show_shopee_verified_label", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowShopeeVerifiedLabel { get; set; }

            [JsonProperty("size_chart")]
            public object SizeChart { get; set; }

            [JsonProperty("reference_item_id", NullValueHandling = NullValueHandling.Ignore)]
            public string ReferenceItemId { get; set; }

            [JsonProperty("brand")]
            public object Brand { get; set; }

            [JsonProperty("item_rating", NullValueHandling = NullValueHandling.Ignore)]
            public ItemRating ItemRating { get; set; }

            [JsonProperty("label_ids", NullValueHandling = NullValueHandling.Ignore)]
            public List<long> LabelIds { get; set; }

            [JsonProperty("attributes")]
            public object Attributes { get; set; }

            [JsonProperty("liked", NullValueHandling = NullValueHandling.Ignore)]
            public bool? Liked { get; set; }

            [JsonProperty("liked_count", NullValueHandling = NullValueHandling.Ignore)]
            public long? LikedCount { get; set; }

            [JsonProperty("cmt_count", NullValueHandling = NullValueHandling.Ignore)]
            public long? CmtCount { get; set; }

            [JsonProperty("flag", NullValueHandling = NullValueHandling.Ignore)]
            public long? Flag { get; set; }

            [JsonProperty("shopee_verified", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShopeeVerified { get; set; }

            [JsonProperty("is_adult", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsAdult { get; set; }

            [JsonProperty("is_preferred_plus_seller", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsPreferredPlusSeller { get; set; }

            [JsonProperty("tier_variations", NullValueHandling = NullValueHandling.Ignore)]
            public List<TierVariation> TierVariations { get; set; }

            [JsonProperty("bundle_deal_id", NullValueHandling = NullValueHandling.Ignore)]
            public long? BundleDealId { get; set; }

            [JsonProperty("can_use_bundle_deal", NullValueHandling = NullValueHandling.Ignore)]
            public bool? CanUseBundleDeal { get; set; }

            [JsonProperty("add_on_deal_info")]
            public object AddOnDealInfo { get; set; }

            [JsonProperty("bundle_deal_info", NullValueHandling = NullValueHandling.Ignore)]
            public BundleDealInfo BundleDealInfo { get; set; }

            [JsonProperty("can_use_wholesale", NullValueHandling = NullValueHandling.Ignore)]
            public bool? CanUseWholesale { get; set; }

            [JsonProperty("wholesale_tier_list", NullValueHandling = NullValueHandling.Ignore)]
            public List<object> WholesaleTierList { get; set; }

            [JsonProperty("is_group_buy_item")]
            public object IsGroupBuyItem { get; set; }

            [JsonProperty("group_buy_info")]
            public object GroupBuyInfo { get; set; }

            [JsonProperty("welcome_package_type", NullValueHandling = NullValueHandling.Ignore)]
            public long? WelcomePackageType { get; set; }

            [JsonProperty("welcome_package_info")]
            public object WelcomePackageInfo { get; set; }

            [JsonProperty("images", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> Images { get; set; }

            [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
            public string Image { get; set; }

            [JsonProperty("video_info_list", NullValueHandling = NullValueHandling.Ignore)]
            public List<VideoInfoList> VideoInfoList { get; set; }

            [JsonProperty("item_type", NullValueHandling = NullValueHandling.Ignore)]
            public long? ItemType { get; set; }

            [JsonProperty("is_official_shop", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsOfficialShop { get; set; }

            [JsonProperty("show_official_shop_label_in_title", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowOfficialShopLabelInTitle { get; set; }

            [JsonProperty("shop_location", NullValueHandling = NullValueHandling.Ignore)]
            public string ShopLocation { get; set; }

            [JsonProperty("coin_earn_label")]
            public object CoinEarnLabel { get; set; }

            [JsonProperty("cb_option", NullValueHandling = NullValueHandling.Ignore)]
            public long? CbOption { get; set; }

            [JsonProperty("is_pre_order", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsPreOrder { get; set; }

            [JsonProperty("estimated_days", NullValueHandling = NullValueHandling.Ignore)]
            public long? EstimatedDays { get; set; }

            [JsonProperty("badge_icon_type", NullValueHandling = NullValueHandling.Ignore)]
            public long? BadgeIconType { get; set; }

            [JsonProperty("show_free_shipping", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowFreeShipping { get; set; }

            [JsonProperty("shipping_icon_type", NullValueHandling = NullValueHandling.Ignore)]
            public long? ShippingIconType { get; set; }

            [JsonProperty("cod_flag", NullValueHandling = NullValueHandling.Ignore)]
            public long? CodFlag { get; set; }

            [JsonProperty("show_original_guarantee", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowOriginalGuarantee { get; set; }

            [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)]
            public List<Category> Categories { get; set; }

            [JsonProperty("other_stock", NullValueHandling = NullValueHandling.Ignore)]
            public long? OtherStock { get; set; }

            [JsonProperty("item_has_post", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ItemHasPost { get; set; }

            [JsonProperty("discount_stock", NullValueHandling = NullValueHandling.Ignore)]
            public long? DiscountStock { get; set; }

            [JsonProperty("current_promotion_has_reserve_stock", NullValueHandling = NullValueHandling.Ignore)]
            public bool? CurrentPromotionHasReserveStock { get; set; }

            [JsonProperty("current_promotion_reserved_stock", NullValueHandling = NullValueHandling.Ignore)]
            public long? CurrentPromotionReservedStock { get; set; }

            [JsonProperty("normal_stock", NullValueHandling = NullValueHandling.Ignore)]
            public long? NormalStock { get; set; }

            [JsonProperty("brand_id", NullValueHandling = NullValueHandling.Ignore)]
            public long? BrandId { get; set; }

            [JsonProperty("has_gimmick_tag")]
            public object HasGimmickTag { get; set; }

            [JsonProperty("is_alcohol_product", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsAlcoholProduct { get; set; }

            [JsonProperty("show_recycling_info", NullValueHandling = NullValueHandling.Ignore)]
            public bool? ShowRecyclingInfo { get; set; }

            [JsonProperty("coin_info", NullValueHandling = NullValueHandling.Ignore)]
            public CoinInfo CoinInfo { get; set; }

            [JsonProperty("models", NullValueHandling = NullValueHandling.Ignore)]
            public List<Model> Models { get; set; }

            [JsonProperty("spl_info")]
            public object SplInfo { get; set; }

            [JsonProperty("preview_info")]
            public object PreviewInfo { get; set; }

            [JsonProperty("is_cc_installment_payment_eligible", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsCcInstallmentPaymentEligible { get; set; }

            [JsonProperty("is_non_cc_installment_payment_eligible", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsNonCcInstallmentPaymentEligible { get; set; }

            [JsonProperty("flash_sale")]
            public object FlashSale { get; set; }

            [JsonProperty("upcoming_flash_sale")]
            public object UpcomingFlashSale { get; set; }

            [JsonProperty("deep_discount")]
            public object DeepDiscount { get; set; }

            [JsonProperty("has_low_fulfillment_rate", NullValueHandling = NullValueHandling.Ignore)]
            public bool? HasLowFulfillmentRate { get; set; }

            [JsonProperty("is_partial_fulfilled", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsPartialFulfilled { get; set; }

            [JsonProperty("makeups")]
            public object Makeups { get; set; }
        }

        public partial class RichTextDescription
        {
            [JsonProperty("paragraph_list")]
            public ParagraphList[] ParagraphList { get; set; }
        }

        public partial class ParagraphList
        {
            [JsonProperty("type")]
            public long Type { get; set; }

            [JsonProperty("text")]
            public object Text { get; set; }

            [JsonProperty("img_id")]
            public string ImgId { get; set; }

            [JsonProperty("ratio", NullValueHandling = NullValueHandling.Ignore)]
            public double? Ratio { get; set; }

            [JsonProperty("empty_paragraph_count")]
            public object EmptyParagraphCount { get; set; }
        }


        public partial class BundleDealInfo
        {
            [JsonProperty("bundle_deal_id", NullValueHandling = NullValueHandling.Ignore)]
            public long? BundleDealId { get; set; }

            [JsonProperty("bundle_deal_label", NullValueHandling = NullValueHandling.Ignore)]
            public string BundleDealLabel { get; set; }
        }

        public partial class Category
        {
            [JsonProperty("catid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Catid { get; set; }

            [JsonProperty("display_name", NullValueHandling = NullValueHandling.Ignore)]
            public string DisplayName { get; set; }

            [JsonProperty("no_sub", NullValueHandling = NullValueHandling.Ignore)]
            public bool? NoSub { get; set; }

            [JsonProperty("is_default_subcat", NullValueHandling = NullValueHandling.Ignore)]
            public bool? IsDefaultSubcat { get; set; }
        }

        public partial class CoinInfo
        {
            [JsonProperty("spend_cash_unit", NullValueHandling = NullValueHandling.Ignore)]
            public long? SpendCashUnit { get; set; }

            [JsonProperty("coin_earn_items", NullValueHandling = NullValueHandling.Ignore)]
            public List<CoinEarnItem> CoinEarnItems { get; set; }
        }

        public partial class CoinEarnItem
        {
            [JsonProperty("coin_earn", NullValueHandling = NullValueHandling.Ignore)]
            public long? CoinEarn { get; set; }
        }

        public partial class ItemRating
        {
            [JsonProperty("rating_star", NullValueHandling = NullValueHandling.Ignore)]
            public double? RatingStar { get; set; }

            [JsonProperty("rating_count", NullValueHandling = NullValueHandling.Ignore)]
            public List<long> RatingCount { get; set; }
        }

        public partial class Model
        {
            [JsonProperty("itemid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Itemid { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public long? Status { get; set; }

            [JsonProperty("current_promotion_reserved_stock", NullValueHandling = NullValueHandling.Ignore)]
            public long? CurrentPromotionReservedStock { get; set; }

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("promotionid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Promotionid { get; set; }

            [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
            public long? Price { get; set; }

            [JsonProperty("price_stocks", NullValueHandling = NullValueHandling.Ignore)]
            public List<PriceStock> PriceStocks { get; set; }

            [JsonProperty("current_promotion_has_reserve_stock", NullValueHandling = NullValueHandling.Ignore)]
            public bool? CurrentPromotionHasReserveStock { get; set; }

            [JsonProperty("normal_stock", NullValueHandling = NullValueHandling.Ignore)]
            public long? NormalStock { get; set; }

            [JsonProperty("extinfo", NullValueHandling = NullValueHandling.Ignore)]
            public Extinfo Extinfo { get; set; }

            [JsonProperty("price_before_discount", NullValueHandling = NullValueHandling.Ignore)]
            public long? PriceBeforeDiscount { get; set; }

            [JsonProperty("modelid", NullValueHandling = NullValueHandling.Ignore)]
            public long? Modelid { get; set; }

            [JsonProperty("stock", NullValueHandling = NullValueHandling.Ignore)]
            public long? Stock { get; set; }
        }

        public partial class Extinfo
        {
            [JsonProperty("tier_index", NullValueHandling = NullValueHandling.Ignore)]
            public List<long> TierIndex { get; set; }

            [JsonProperty("group_buy_info")]
            public object GroupBuyInfo { get; set; }
        }

        public partial class PriceStock
        {
            [JsonProperty("allocated_stock")]
            public long? AllocatedStock { get; set; }

            [JsonProperty("stock_breakdown_by_location")]
            public List<StockBreakdownByLocation> StockBreakdownByLocation { get; set; }
        }

        public partial class StockBreakdownByLocation
        {
            [JsonProperty("location_id", NullValueHandling = NullValueHandling.Ignore)]
            public string LocationId { get; set; }

            [JsonProperty("available_stock", NullValueHandling = NullValueHandling.Ignore)]
            public long? AvailableStock { get; set; }

            [JsonProperty("fulfilment_type", NullValueHandling = NullValueHandling.Ignore)]
            public long? FulfilmentType { get; set; }

            [JsonProperty("address_id")]
            public object AddressId { get; set; }
        }

        public partial class OverallPurchaseLimit
        {
            [JsonProperty("order_max_purchase_limit", NullValueHandling = NullValueHandling.Ignore)]
            public long? OrderMaxPurchaseLimit { get; set; }

            [JsonProperty("overall_purchase_limit")]
            public object OverallPurchaseLimitOverallPurchaseLimit { get; set; }

            [JsonProperty("item_overall_quota")]
            public object ItemOverallQuota { get; set; }

            [JsonProperty("start_date")]
            public object StartDate { get; set; }

            [JsonProperty("end_date")]
            public object EndDate { get; set; }
        }

        public partial class TierVariation
        {
            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> Options { get; set; }

            [JsonProperty("images")]
            public List<string> Images { get; set; }

            [JsonProperty("properties")]
            public object Properties { get; set; }

            [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
            public long? Type { get; set; }
        }

        public partial class VideoInfoList
        {
            [JsonProperty("video_id", NullValueHandling = NullValueHandling.Ignore)]
            public string VideoId { get; set; }

            [JsonProperty("thumb_url", NullValueHandling = NullValueHandling.Ignore)]
            public string ThumbUrl { get; set; }

            [JsonProperty("duration", NullValueHandling = NullValueHandling.Ignore)]
            public long? Duration { get; set; }

            [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
            public long? Version { get; set; }

            [JsonProperty("formats", NullValueHandling = NullValueHandling.Ignore)]
            public List<object> Formats { get; set; }

            [JsonProperty("default_format")]
            public object DefaultFormat { get; set; }
        }
        public partial class ShopeeProductModelResults
        {
            public static ShopeeProductModelResults FromJson(string json) => JsonConvert.DeserializeObject<ShopeeProductModelResults>(json, Converter.Settings);
        }

    }
}
