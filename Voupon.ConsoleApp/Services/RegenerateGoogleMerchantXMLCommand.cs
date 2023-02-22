using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Voupon.Common.Azure.Blob;
using Voupon.Database.MSSQL.RewardsEntities;
using Voupon.Database.MSSQL.VodusEntities;

namespace Voupon.ConsoleApp.Services
{
    public static class RegenerateGoogleMerchantXMLCommand
    {
        public async static Task<bool> Execute(IAzureBlobStorage azureBlobStorage, VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
        {
            const  int AGG_SLEEP_TIME_IN_MILLISECONDS = 10000;
            var apiResponseViewModel = new ApiResponseViewModel();
            const double ADDITIONAL_SHIPPING_DISCOUNTS = 6.00;
            try
            {
                var result = new List<GoogleMerchantKeywords>();
                var additionalDiscounts = await rewardsDBContext.AdditionalDiscounts.Where(x=>x.StatusId == 1).AsNoTracking().ToListAsync();
                var googleMerchantKeywords = await rewardsDBContext.GoogleMerchantKeywords.ToListAsync();
                if (googleMerchantKeywords == null)
                {
                    Console.WriteLine("No keywords detected");
                    return false;
                }

                Console.WriteLine("Total keywords: " + googleMerchantKeywords.Count());
                var selectedProductForGoogleMerchant = new List<SearchProductViewModel>();
                var provinces = await rewardsDBContext.Provinces.AsNoTracking().ToListAsync();
                var aggregatorUrl = await vodusV2Context.AggregatorApiUrls.AsNoTracking().ToListAsync();
                var appConfig = await rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                if (appConfig == null || !appConfig.IsAggregatorEnabled)
                {
                    return false;
                }

                var i = 0;
                foreach (var googleMerchant in googleMerchantKeywords)
                {
                    i++;
                    Console.WriteLine($"Getting products: {i}/{googleMerchantKeywords.Count()}");
                    List<ProductModel> filteredList = new List<ProductModel>();
                    var aggregatorPriceFilter = new List<int>();

                    var timeStart = DateTime.Now;

                    var items = new List<Database.MSSQL.RewardsEntities.Products>();
                    if (!string.IsNullOrEmpty(googleMerchant.Keyword))
                    {
                        items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Include(x => x.DealExpirations).Where(x => x.Merchant.IsPublished && x.IsActivated && x.IsPublished && x.Title.Contains(googleMerchant.Keyword)).ToListAsync();
                    }
                    else
                    {
                        items = await rewardsDBContext.Products.AsNoTracking().Include(x => x.ProductOutlets).ThenInclude(x => x.Outlet).Include(x => x.Merchant).Include(x => x.ProductDiscounts).Include(x => x.ProductSubCategory).Include(x => x.ProductCategory).Include(x => x.StatusType).Include(x => x.DealType).Include(x => x.DealExpirations).Where(x => x.Merchant.IsPublished && x.IsActivated && x.IsPublished).ToListAsync();
                    }

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

                        if (item.ProductOutlets.Select(z => z.Outlet).Any())
                        {
                            newItem.OutletLocation = provinces.Where(x => x.Id == item.ProductOutlets.Select(z => z.Outlet).First().ProvinceId).First().Name;

                        }
                        else
                        {
                            newItem.OutletLocation = "Global";
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
                            var higherDiscount = item.ProductDiscounts.OrderByDescending(x => x.PointRequired).ToList().FirstOrDefault();

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

                        newItem.OutletName = item.ProductOutlets.Select(x => x.Outlet.Name).ToList();
                        newItem.OutletProvince = item.ProductOutlets.Select(x => x.Outlet.ProvinceId).ToList();
                        newItem.TotalOutlets = item.ProductOutlets.Count();
                        list.Add(newItem);
                    }

                    if (list != null && list.Any())
                    {
                        var productList = list;

                        filteredList = productList.Where(x => x.IsPublished == true && x.IsActivated == true && ((x.ExpirationTypeId == 1 || x.ExpirationTypeId == 2)) || x.ExpirationTypeId == 4 || x.ExpirationTypeId == 5).OrderBy(x => x.Title).ToList();

                        var star = DateTime.Now;

                        foreach (var item in filteredList)
                        {
                            BlobSmallImagesListQuery blobRequest = new BlobSmallImagesListQuery()
                            {
                                Id = item.Id,
                                ContainerName = ContainerNameEnum.Products,
                                FilePath = FilePathEnum.Products_Images
                            };
                            var filename = await azureBlobStorage.ListBlobsAsync(blobRequest.ContainerName, blobRequest.Id + "/" + blobRequest.FilePath);
                            if (filename != null)
                            {
                                if (!blobRequest.GetIListBlobItem)
                                {
                                    var fileList = new List<string>();
                                    foreach (var file in filename)
                                    {
                                        if (file.StorageUri.PrimaryUri.OriginalString.Contains("small"))
                                        {
                                            fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
                                        }
                                        else if (!file.StorageUri.PrimaryUri.OriginalString.Contains("big") && !file.StorageUri.PrimaryUri.OriginalString.Contains("normal") && !file.StorageUri.PrimaryUri.OriginalString.Contains("org"))
                                        {
                                            fileList.Add(file.StorageUri.PrimaryUri.OriginalString.Replace("http://", "https://"));
                                        }

                                    }
                                    item.ImageFolderUrl = fileList;
                                }
                            }

                        }
                    }

                    var resultList = new List<SearchProductViewModel>();
                    if (filteredList != null && filteredList.Where(x => x.ImageFolderUrl.Count() > 0).Any())
                    {
                        resultList.AddRange(filteredList.Where(x => x.ImageFolderUrl.Count() > 0).Select(x => new SearchProductViewModel
                        {
                            DealExpirationId = x.DealExpirationId,
                            DealType = x.DealType,
                            DealTypeId = x.DealTypeId,
                            DiscountedPrice = x.DiscountedPrice,
                            DiscountRate = x.DiscountRate,
                            ExpirationTypeId = x.ExpirationTypeId,
                            Id = x.Id,
                            PointsRequired = x.PointsRequired,
                            Price = x.Price,
                            Rating = x.Rating,
                            Title = x.Title,
                            TotalSold = (x.TotalBought.HasValue ? (int)x.TotalBought : 0),
                            ProductSubCategory = x.ProductSubCategory,
                            ProductSubCategoryId = x.ProductSubCategoryId,
                            ProductCategoryId = x.ProductCategoryId,
                            ProductImage = (x.ImageFolderUrl != null && x.ImageFolderUrl.Any() ? x.ImageFolderUrl[0] : ""),
                            OutletLocation = x.OutletLocation,
                            Brand = x.MerchantName,
                            Description = x.Description,
                            Language = googleMerchant.Language

                        }));
                    }

                    if (!string.IsNullOrEmpty(googleMerchant.Keyword))
                    {
                        var aggRequest = new AggregatorSearchByKeywordQuery
                        {
                            SearchQuery = googleMerchant.Keyword,
                            PageNumber = 1
                        };

                        var agg = await rewardsDBContext.AggregatorApiUrls.Where(x => x.StatusId == 1 && x.TypeId == 2).Where(x => x.IsLocalAggregator == false).OrderBy(x => x.LastUpdatedAt).FirstOrDefaultAsync();

                        if (agg == null)
                        {
                            return false;
                        }

                        agg.LastUpdatedAt = DateTime.Now;
                        rewardsDBContext.Update(agg);
                        await rewardsDBContext.SaveChangesAsync();

                        Thread.Sleep(AGG_SLEEP_TIME_IN_MILLISECONDS);

                        StringContent httpContent = new StringContent(JsonConvert.SerializeObject(aggRequest), System.Text.Encoding.UTF8, "application/json");
                        var httpClient = new HttpClient();
                        var aggResult = await httpClient.PostAsync($"{agg.Url}/aggregator/product-search", httpContent);
                        var resultString = await aggResult.Content.ReadAsStringAsync();
                        var crawlerResult = JsonConvert.DeserializeObject<ApiResponseViewModel>(resultString);
                        if (crawlerResult.successful)
                        {
                            if(crawlerResult.data == null)
                            {
                                continue;
                            }
                            var aggregatorResultList = new List<SearchProductViewModel>();
                            var aggregatorData = JsonConvert.DeserializeObject<List<SearchProductViewModel>>(crawlerResult.data.ToString());
                            if (aggregatorData != null && aggregatorData.Any())
                            {
                                foreach (var aggreagatorItem in aggregatorData)
                                {
                                    aggreagatorItem.Language = googleMerchant.Language;
                                    aggregatorResultList.Add(aggreagatorItem);
                                }
                            }

                            if (aggregatorResultList != null && aggregatorResultList.Any())
                            {
                                aggregatorResultList.OrderBy(x => x.Price);
                                resultList.AddRange(aggregatorResultList);
                            }
                        }

                        if (googleMerchant.SortBy == "sold")
                        {
                            var take = (googleMerchant.TotalListing > 2 ? (int)Math.Floor((decimal)googleMerchant.TotalListing) : googleMerchant.TotalListing);
                            var itemsToAdd = resultList.OrderByDescending(x => x.TotalSold).Take((take > 5 ? 4 : take)).ToList();
                            selectedProductForGoogleMerchant.AddRange(itemsToAdd);
                        }
                        else
                        {

                        }
                    }
                }
                if (selectedProductForGoogleMerchant != null && selectedProductForGoogleMerchant.Any())
                {
                    //  Get shipping details
                    var vodusProducts = selectedProductForGoogleMerchant.Where(x => x.Id > 0).Select(x => x.Id);

                    var shippingCosts = await rewardsDBContext.ShippingCost.AsNoTracking().Include(x => x.ProductShipping).ThenInclude(x => x.Product).ThenInclude(x => x.Merchant).Where(x => x.ProvinceId == 14 && vodusProducts.ToArray().Contains(x.ProductShipping.ProductId)).ToListAsync();
                    var groupedProducts = shippingCosts.GroupBy(x => x.ProductShipping.Product.MerchantId);
                    decimal totalShippingCost = 0;
                    OrderShippingCostsModel orderShippingCostsModel = new OrderShippingCostsModel();
                    List<OrderShippingCostForPoductIdAndVariationIdModel> orderShippingCosts = new List<OrderShippingCostForPoductIdAndVariationIdModel>();
                    foreach (var merchantProducts in groupedProducts)
                    {
                        var merchantProductIdWithShippingCost = 0;
                        var merchantProductVarationIdWithShippingCost = 0;
                        var currentMerchantId = merchantProducts.Key;
                        decimal maxShippingCost = 0;
                        foreach (var Product in merchantProducts)
                        {
                            maxShippingCost = Product.Cost;
                            merchantProductIdWithShippingCost = Product.ProductShipping.ProductId;
                        }
                        OrderShippingCostForPoductIdAndVariationIdModel orderShippingCost = new OrderShippingCostForPoductIdAndVariationIdModel()
                        {
                            ProductId = merchantProductIdWithShippingCost,
                            VariationId = merchantProductVarationIdWithShippingCost,
                            OrderShippingCost = Math.Max(maxShippingCost - (decimal)ADDITIONAL_SHIPPING_DISCOUNTS, 0)
                        };
                        orderShippingCosts.Add(orderShippingCost);
                        totalShippingCost += orderShippingCost.OrderShippingCost;
                    }

                    //  Add external item shipping costs
                    orderShippingCostsModel.OrderShippingCosts = orderShippingCosts;
                    orderShippingCostsModel.TotalShippingCost = totalShippingCost;

                    if (orderShippingCostsModel != null)
                    {
                        var shippingResult = orderShippingCostsModel;

                        foreach (var item in shippingResult.OrderShippingCosts)
                        {
                            var itemToUpdate = selectedProductForGoogleMerchant.Where(x => x.Id == item.ProductId).First();
                            itemToUpdate.ShippingCost = item.OrderShippingCost;
                        }
                    }

                    var totalItemsToGetShippingCost = 0;

                    foreach (var item in selectedProductForGoogleMerchant.Where(x => x.Id == 0))
                    {
                        if (item.Id == 0)
                        {
                            totalItemsToGetShippingCost++;
                            Console.WriteLine($"Getting shipping costs: {totalItemsToGetShippingCost}/{selectedProductForGoogleMerchant.Where(x => x.Id == 0).Count()}");
                            var externalShippingResult = new AggregatorShippingCostQuery()
                            {
                                ExternalItemId = item.ExternalItemId,
                                ExternalShopId = item.ExternalShopId,
                                ExternalTypeId = item.ExternalTypeId,
                                City = "",
                                Town = "Kuala Lumpur"
                            };

                            //  This is fixed to get from not local
                            var aggUrl = aggregatorUrl.Where(x => x.IsLocalAggregator == false && x.Status == 1).FirstOrDefault().Url;

                            Thread.Sleep(AGG_SLEEP_TIME_IN_MILLISECONDS);

                            StringContent httpContent = new StringContent(JsonConvert.SerializeObject(externalShippingResult), System.Text.Encoding.UTF8, "application/json");
                            var httpClient = new HttpClient();
                            var aggResult = await httpClient.PostAsync($"{aggUrl}/v1/shipping-cost", httpContent);
                            var resultString = await aggResult.Content.ReadAsStringAsync();
                            var crawlerResult = JsonConvert.DeserializeObject<ApiResponseViewModel>(resultString);
                            if (crawlerResult.successful)
                            {
                                var aggregatorData = JsonConvert.DeserializeObject<OrderShippingCostForPoductIdAndVariationIdModel>(crawlerResult.data.ToString());
                                //aggregatorData.OrderShippingCost = (aggregatorData.OrderShippingCost >= appSettings.Value.App.AdditionalShippingDiscount ? aggregatorData.OrderShippingCost - appSettings.Value.App.AdditionalShippingDiscount : 0);
                                aggregatorData.OrderShippingCost = aggregatorData.OrderShippingCost;
                                apiResponseViewModel.data = JsonConvert.SerializeObject(aggregatorData);
                                apiResponseViewModel.successful = crawlerResult.successful;

                                item.ShippingCost += aggregatorData.OrderShippingCost;
                            }
                        }
                    }

                    var enList = selectedProductForGoogleMerchant.Where(x => x.Language.ToLower() == "en").ToList();
                    var msList = selectedProductForGoogleMerchant.Where(x => x.Language.ToLower() == "ms").ToList();

                    if (enList != null && enList.Any())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            XmlWriterSettings settings = new XmlWriterSettings { Indent = true,  DoNotEscapeUriAttributes = true,  Encoding = Encoding.UTF8 };
                            using (XmlWriter writer = XmlWriter.Create(ms, settings))
                            {
                                writer.WriteStartElement("rss", "http://base.google.com/ns/1.0");
                                writer.WriteAttributeString("version", "2.0");
                                writer.WriteStartElement("channel");
                                writer.WriteElementString("title", "Vodus Google Merchant Products");
                                writer.WriteElementString("link", "https://vodus.my");
                                writer.WriteElementString("description", "Vodus Google Merchant Products");
                                foreach (var item in enList)
                                {
                                    writer.WriteStartElement("item");
                                    writer.WriteElementString("id", (item.Id > 0 ? item.Id.ToString() : item.ExternalItemId + "-" + item.ExternalShopId + "-" + item.ExternalTypeId));
                                    decimal price = 0;


                                    //additionalDiscounts
                                    if (item.Id == 0)
                                    {

                                        if (item.ExternalItemId == "9112361444")
                                        {
                                            var aaa = "9112361444";
                                        }
                                        if (item.DiscountedPriceMin.HasValue)
                                        {
                                            price = item.DiscountedPriceMin.Value;
                                        }
                                        else
                                        {
                                            price = (decimal)item.DiscountedPrice.Value;
                                        }


                                        if (additionalDiscounts != null)
                                        {
                                            var qualifiedForDiscount = additionalDiscounts.OrderBy(x => x.MaxPrice).Where(x => price <= x.MaxPrice).FirstOrDefault();
                                            if (qualifiedForDiscount != null)
                                            {
                                                price -= ((decimal)((price * ((decimal)qualifiedForDiscount.DiscountPercentage)) / 100));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        price = (decimal)item.DiscountedPrice.Value;
                                    }

                                    writer.WriteElementString("title", item.Title.Length > 150 ? item.Title.Substring(0, 149) : item.Title);
                                    writer.WriteElementString("description", item.Description.Length > 5000 ? item.Description.Substring(0, 4999) : item.Title);
                                    writer.WriteElementString("link", $"https://vodus.my/product/{item.Id}" + (!string.IsNullOrEmpty(item.ExternalItemId) ? $"?i={item.ExternalItemId}&s={item.ExternalShopId}&t={item.ExternalTypeId}" : ""));
                                    writer.WriteElementString("image_link", item.ProductImage);
                                    writer.WriteElementString("availability", "in_stock");
                                    writer.WriteElementString("price", price.ToString("F") + " MYR");
                                    writer.WriteElementString("brand", item.Brand);
                                    writer.WriteStartElement("shipping");
                                    writer.WriteElementString("price", item.ShippingCost.ToString("F") + " MYR");
                                    writer.WriteElementString("country", "MY");
                                    writer.WriteEndElement();
                                    writer.WriteEndElement();
                                }
                                writer.WriteEndElement();

                                writer.Flush();
                            }
                            ms.Position = 0;

                            var blob = await azureBlobStorage.UploadBlobAsync(ms, "google-merchant-products-en.xml", "application /xml",
                                 "google-merchant", true);
                            if (blob == null)
                            {
                                Console.WriteLine("Fail to upload the xml");
                                return false;
                            }
                        }
                    }

                    /*
                    string subject = "Google Merchant XML";

                    var MARKETING_TEAM_EMAILS = Environment.GetEnvironmentVariable("MARKETING_TEAM_EMAILS");
                    if (MARKETING_TEAM_EMAILS != null)
                    {
                        var sendGridClient = new SendGridClient(Environment.GetEnvironmentVariable("SENDGRID_APIKEY"));
                        var msg = new SendGridMessage();
                        msg.SetTemplateId("d3071f81-7e2d-4e0f-9f1d-4e6ebcab4d96");
                        msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
                        msg.SetSubject(subject);

                        var emailList = new List<EmailAddress>();
                        var emailItems = MARKETING_TEAM_EMAILS.Split(",");

                        foreach (var email in emailItems)
                        {
                            emailList.Add(new EmailAddress(email));
                        }

                        msg.AddTos(emailList);
                        msg.AddSubstitution("-EmailBody-", "Done generating Google merchant product XML");
                        var res = await sendGridClient.SendEmailAsync(msg);
                        Console.WriteLine(JsonConvert.SerializeObject(res));
                    }
                    else
                    {
                        var sendGridClient = new SendGridClient(Environment.GetEnvironmentVariable("SENDGRID_APIKEY"));
                        var msg = new SendGridMessage();
                        msg.SetTemplateId("d3071f81-7e2d-4e0f-9f1d-4e6ebcab4d96");
                        msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
                        msg.SetSubject(subject + " NO MARKETING EMAIL");

                        msg.AddTo(new EmailAddress("kok.hong@vodus.my"));
                        msg.AddSubstitution("-EmailBody-", "Done generating Google merchant product XML");
                        var res = await sendGridClient.SendEmailAsync(msg);
                        Console.WriteLine(JsonConvert.SerializeObject(res));
                    }
                    */
                    Console.WriteLine("Done creating new Google Merchant XML");
                }
                else
                {
                    Console.WriteLine("No xml to regenerate");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed with " + ex.ToString());
                /*
                var MARKETING_TEAM_EMAILS = Environment.GetEnvironmentVariable("MARKETING_TEAM_EMAILS");
                if (MARKETING_TEAM_EMAILS != null)
                {
                    var sendGridClient = new SendGridClient(Environment.GetEnvironmentVariable("SENDGRID_APIKEY"));
                    var msg = new SendGridMessage();
                    msg.SetTemplateId("d3071f81-7e2d-4e0f-9f1d-4e6ebcab4d96");
                    msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
                    msg.SetSubject("Error generating Google Merchant XML");

                    var emailList = new List<EmailAddress>();
                    var emailItems = MARKETING_TEAM_EMAILS.Split(",");

                    foreach (var email in emailItems)
                    {
                        emailList.Add(new EmailAddress(email));
                    }

                    msg.AddTos(emailList);
                    msg.AddSubstitution("-EmailBody-", "Error generating Google merchant product XML. Error = " + ex.ToString());
                    var res = await sendGridClient.SendEmailAsync(msg);
                    Console.WriteLine(JsonConvert.SerializeObject(res));
                }
                else
                {
                    var sendGridClient = new SendGridClient(Environment.GetEnvironmentVariable("SENDGRID_APIKEY"));
                    var msg = new SendGridMessage();
                    msg.SetTemplateId("d3071f81-7e2d-4e0f-9f1d-4e6ebcab4d96");
                    msg.SetFrom(new EmailAddress("noreply@vodus.my", "Vodus No-Reply"));
                    msg.SetSubject("Error generating Google Merchant XML no MARKETING EMAIL");

                    msg.AddTo(new EmailAddress("kok.hong@vodus.my"));
                    msg.AddSubstitution("-EmailBody-", "Error generating Google merchant product XML. Error = " + ex.ToString());
                    var res = await sendGridClient.SendEmailAsync(msg);
                    Console.WriteLine(JsonConvert.SerializeObject(res));
                }
                */
                return false;
            }
        }


        public class GoogleMerchantKeywords
        {
            public Guid Id { get; set; }
            public string Keyword { get; set; }
            public int TotalListing { get; set; }
            public string SortBy { get; set; }
            public string Language { get; set; }
        }

        public class SearchProductViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string ProductImage { get; set; }
            public int? ProductCategoryId { get; set; }
            public string ProductCategory { get; set; }
            public int? ProductSubCategoryId { get; set; }
            public string ProductSubCategory { get; set; }
            public int? DealTypeId { get; set; }
            public string DealType { get; set; }
            public decimal? Price { get; set; }
            public decimal? DiscountedPrice { get; set; }
            public int? DiscountRate { get; set; }
            public int? PointsRequired { get; set; }
            public int? DealExpirationId { get; set; }
            public int? ExpirationTypeId { get; set; }

            public int TotalSold { get; set; }
            public decimal Rating { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalShopId { get; set; }

            public string ExternalItemId { get; set; }

            public string ExternalUrl { get; set; }
            public string OutletLocation { get; set; }

            public decimal? DiscountedPriceMin { get; set; }

            public decimal? DiscountedPriceMax { get; set; }

            public decimal? BeforeDiscountedPriceMin { get; set; }

            public decimal? BeforeDiscountedPriceMax { get; set; }

            public bool IsOriginalGuaranteeProduct { get; set; }

            public string Brand { get; set; }

            public string Description { get; set; }

            public string Language { get; set; }

            public decimal ShippingCost { get; set; }
        }

        public class SearchProductViewModelFilterTitles : SearchProductViewModel
        {
            public List<string> FilterTitle { get; set; }
        }

        public class ProductModel
        {
            public int Id { get; set; }
            public int MerchantId { get; set; }
            public string MerchantCode { get; set; }
            public string MerchantName { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public string Description { get; set; }
            public string AdditionInfo { get; set; }
            public string FinePrintInfo { get; set; }
            public string RedemptionInfo { get; set; }
            public List<string> ImageFolderUrl { get; set; }
            public int? ProductCategoryId { get; set; }
            public string ProductCategory { get; set; }
            public int? ProductSubCategoryId { get; set; }
            public string ProductSubCategory { get; set; }
            public int? DealTypeId { get; set; }
            public string DealType { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public decimal? Price { get; set; }
            public decimal? DiscountedPrice { get; set; }
            public int? DiscountRate { get; set; }
            public int? PointsRequired { get; set; }
            public int? AvailableQuantity { get; set; }
            public int? DealExpirationId { get; set; }
            public int? ExpirationTypeId { get; set; }
            public int? LuckyDrawId { get; set; }
            public int StatusTypeId { get; set; }
            public string StatusType { get; set; }
            public bool IsActivated { get; set; }
            public bool IsPublished { get; set; }
            public DateTime CreatedAt { get; set; }
            public Guid CreatedByUserId { get; set; }
            public DateTime? LastUpdatedAt { get; set; }
            public Guid? LastUpdatedByUser { get; set; }
            public int? TotalBought { get; set; }
            public string Remarks { get; set; }
            public List<string> OutletName { get; set; }
            public List<int?> OutletProvince { get; set; }
            public int TotalOutlets { get; set; }
            public decimal? CTR { get; set; }
            public int WeightedCTR { get; set; }
            public decimal Rating { get; set; }
            public Guid? ThirdPartyTypeId { get; set; }
            public Guid? ThirdPartyProductId { get; set; }

            public string OutletLocation { get; set; }
        }

        public class OrderShippingCostForPoductIdAndVariationIdModel
        {
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
            public string ExternalItemVariationText { get; set; }
            public int ProductId { get; set; }
            public int VariationId { get; set; }
            public decimal OrderShippingCost { get; set; }
        }

        public class OrderShippingCostsModel
        {
            public decimal TotalShippingCost { get; set; }

            public List<OrderShippingCostForPoductIdAndVariationIdModel> OrderShippingCosts { get; set; }
        }

        public class AggregatorShippingCostQuery
        {
            public string Town { get; set; }
            public string City { get; set; }
            public string ExternalItemId { get; set; }
            public string ExternalShopId { get; set; }
            public byte ExternalTypeId { get; set; }
        }

        public class AggregatorSearchByKeywordQuery
        {
            public string SearchQuery { get; set; }
            public int PageNumber { get; set; }

            public bool PriceFilter { get; set; }

            public int MinPrice { get; set; }

            public int MaxPrice { get; set; }
            public List<int> LocationFilter { get; set; }
        }
        public class ApiResponseViewModel
        {
            private static readonly object EMPTY = new object();

            [JsonProperty("successful")]
            public bool successful { get; set; }

            [JsonProperty("code")]
            public int code { get; set; }
            [JsonProperty("message")]
            public string message { get; set; }
            [JsonProperty("data")]
            public object data { get; set; }

            public ApiResponseViewModel()
            {
                successful = false;
                message = null;
                code = -1;
                data = EMPTY;
            }
        }

        public class BlobSmallImagesListQuery
        {
            private bool getIListBlobItem = false;
            public int Id { get; set; }
            public string FilePath { get; set; }
            public string ContainerName { get; set; }
            public bool GetIListBlobItem
            {
                get { return getIListBlobItem; }
                set { getIListBlobItem = value; }
            }
        }

    }
}