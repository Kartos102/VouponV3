using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Voupon.Common.Azure.Blob;
using Voupon.Rewards.WebApp.Common.Blob.Queries;
using Voupon.Rewards.WebApp.Common.MasterMemberProfiles.Queries;
using Voupon.Rewards.WebApp.Common.Orders.Queries;
using Voupon.Rewards.WebApp.Common.ProductCategories.Queries;
using Voupon.Rewards.WebApp.Common.ShippingCost.Models;
using Voupon.Rewards.WebApp.Infrastructures.Enums;
using Voupon.Rewards.WebApp.Services.Aggregators.Queries;
using Voupon.Rewards.WebApp.Services.AppConfig.Queries;
using Voupon.Rewards.WebApp.Services.Cart.Commands;
using Voupon.Rewards.WebApp.Services.Cart.Models;
using Voupon.Rewards.WebApp.Services.Cart.Queries;
using Voupon.Rewards.WebApp.Services.Config.Queries;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.Services.Profile.Page;
using Voupon.Rewards.WebApp.ViewModels;
using static Voupon.Rewards.WebApp.Common.MasterMemberProfiles.Queries.MasterMemberProfileDetailsQueryHandler;
using static Voupon.Rewards.WebApp.Services.Deal.Page.DetailPage;

namespace Voupon.Rewards.WebApp.Controllers
{
   
    public class CartController : BaseController
    {
        [Authorize]
        [Route("/cart")]
        public async Task<IActionResult> Index()
        {
            var request = new EditPage
            {
                Email = User.Identity.Name
            };

            if (string.IsNullOrEmpty(request.Email))
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await Mediator.Send(request);

            if (result.Id == 0)
            {
                return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
            }
            if (result.AddressLine1 == null || result.City == null
                || result.CountryId == null || result.DateOfBirthDay == 0 || result.DateOfBirthMonth == 0 || result.DateOfBirthYear == 0
                || result.DemographicGenderId == 0 || result.DemographicStateId == 0
                || result.FirstName == "" || result.LastName == "" || result.MobileCountryCode == null || result.MobileNumber == null
                || result.Postcode == null)
            {
                return RedirectToAction("Edit", "Profile", new { from = "cart" });
            }

            return View();
        }

        [HttpGet]
        [Route("/cart/profile-for-checkout")]
        public async Task<IActionResult> GetProfile()
        {
            ApiResponseViewModel result = await Mediator.Send(new MasterMemberProfileShippingQuery() { MasterMemberProfileId = GetMasterMemberId(Request.HttpContext) });
            if (result.Successful)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

        [HttpPost]
        [Route("/cart/create-order")]
        public async Task<IActionResult> CreateOrder([FromForm] CreateOrderCommand command)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    return BadRequest("Please login first");
                }

                var masterMemberId = GetMasterMemberId(Request.HttpContext);
                if (masterMemberId == 0)
                {
                    ApiResponseViewModel response = new ApiResponseViewModel();
                    response.Successful = false;
                    response.Message = "Please re-login to continue";
                    return BadRequest(response.Message);
                }

                command.MasterMemberProfileId = masterMemberId;
                command.UserName = User.Identity.Name;
                command.BillingEmail = User.Identity.Name;
                command.ShippingEmail = User.Identity.Name;
                command.ShippingCostModel.MasterMemberProfileId = masterMemberId;
                ApiResponseViewModel responseViewModel = await Mediator.Send(command.ShippingCostModel);
                if (responseViewModel.Successful)
                {
                    OrderShippingCostsModel orderShippingCostsModel = (OrderShippingCostsModel)responseViewModel.Data;
                    command.OrderShippingCost = orderShippingCostsModel.TotalShippingCost;
                    command.OrderShippingCosts = orderShippingCostsModel.OrderShippingCosts;
                }
                else
                {
                    responseViewModel.Successful = false;
                    responseViewModel.Message = "Invalid request [009]";
                }

                var result = await Mediator.Send(command);
                if (result.Successful)
                {
                    var resultData = (CreateOrderResponseViewModel)result.Data;
                    if (resultData.OrderStatus == 2)
                    {
                        TempData["CheckoutOrderCompleted"] = "Thank you for your order. Your VPoints have been deducted and your order is being processed.";
                        var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions()
                        {
                            Path = "/",
                            Expires = DateTime.Now.AddDays(365),
                            SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                            Secure = true
                        };
                        HttpContext.Response.Cookies.Append("Rewards.Temporary.Points", resultData.MasterMemberPoints.ToString(), cookieOptions);
                    }
                    return Ok(result);
                }
                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                var result = await Mediator.Send(new CreateErrorLogCommand
                {
                    TypeId = CreateErrorLogCommand.Type.Controller,
                    ActionName = "CreateOrder",
                    ActionRequest = JsonConvert.SerializeObject(command),
                    Errors = ex.ToString()
                });
                return BadRequest("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        [Route("/cart/get-cart-products")]
        public async Task<ApiResponseViewModel> GetCartProducts()
        {
            var masterMemberProfile =  GetMasterMemberId(Request.HttpContext);
            ApiResponseViewModel result = await Mediator.Send(new MasterMemberProfileDetailsQuery() { MasterMemberProfileId = masterMemberProfile });
            if (result.Successful)
            {
                ApiResponseViewModel responseViewModel = await Mediator.Send(new CartProductsListQuery() { MasterMemberProfileId = masterMemberProfile });
                if (result.Successful)
                {
                    var cartProductsList = new List<CartProductModel>();
                    cartProductsList = (List<CartProductModel>)responseViewModel.Data;
                    foreach (var cartProduct in cartProductsList)
                    {
                        if (cartProduct.IsVariationProduct == false)
                        {
                            if (cartProduct.Id != 0)
                            {
                                BlobSmallImagesListQuery getImagesCommand = new BlobSmallImagesListQuery()
                                {
                                    Id = cartProduct.ProductId,
                                    ContainerName = ContainerNameEnum.Products,
                                    FilePath = FilePathEnum.Products_Images
                                };
                                ApiResponseViewModel getImagesResponse = await Mediator.Send(getImagesCommand);
                                if (getImagesResponse.Successful)
                                {
                                    ApiResponseViewModel apiResponseViewModel = new ApiResponseViewModel();
                                    var ImagesList = (List<string>)getImagesResponse.Data;
                                    if (ImagesList.Count() > 0) 
                                    {
                                        cartProduct.ProductCartPreviewSmallImage = ImagesList[0];
                                    }
                                    
                                }
                            }
                            if (!string.IsNullOrEmpty(cartProduct.ExternalItemId))
                            {
                                cartProduct.Id = 0;
                            }
                        }

                        /*
                        if (!string.IsNullOrEmpty(cartProduct.ExternalId))
                        {
                            var productResult = await Mediator.Send(new AggregatorProductQuery 
                            {
                                ExternalItemId = cartProduct.ExternalItemId,
                                ExternalShopId = cartProduct.ExternalShopId,
                                ExternalTypeId = cartProduct.ExternalTypeId
                            });

                            if (productResult.Successful)
                            {
                                var productData = (DetailPageViewModel)JsonConvert.DeserializeObject<DetailPageViewModel>(productResult.Data.ToString());

                                if(productData.Price != null)
                                {
                                    if (!string.IsNullOrEmpty(cartProduct.VariationText))
                                    {
                                        var variationText = cartProduct.VariationText.Split(",");
                                        //var a = variationText.Length;
                                        if (variationText.Length == 1)
                                        {
                                            if (productData.VariationModel != null)
                                            {
                                                var variationItem = productData.VariationModel.VariationList.SelectMany(x => x.VariationOptions).Where(x => x.Name.ToLower() == variationText[0].ToLower()).FirstOrDefault();
                                                if (variationItem == null)
                                                {
                                                    cartProduct.AvailableQuantity = productData.VariationModel.ProductVariationDetailsList.Where(x => x.Order == variationItem.Order.ToString()).FirstOrDefault().AvailableQuantity;
                                                }
                                            }

                                        }
                                        else
                                        {
                                            var variationItem = productData.VariationModel.VariationList.SelectMany(x => x.VariationOptions).Where(x => x.Name.ToLower() == variationText[0].ToLower().Trim()).FirstOrDefault();
                                            var variationItem2 = productData.VariationModel.VariationList.SelectMany(x => x.VariationOptions).Where(x => x.Name.ToLower() == variationText[1].ToLower().Trim()).FirstOrDefault();


                                            if (variationItem != null && variationItem2 != null)
                                            {
                                                var order = variationItem.Order.ToString() + "," + variationItem2.Order.ToString();
                                                cartProduct.AvailableQuantity = productData.VariationModel.ProductVariationDetailsList.Where(x => x.Order == order).FirstOrDefault().AvailableQuantity;

                                            }

                                        }
                                    }
                                    else
                                    {
                                        cartProduct.AvailableQuantity = (productData.MaxPurchaseLimit > 0 ? productData.MaxPurchaseLimit : (productData.AvailableQuantity.HasValue ? productData.AvailableQuantity.Value : 0));
                                    }

                                }
                                
                                cartProduct.ExternalItemUrl = productData.ExternalItemUrl;
                                cartProduct.ExternalShopUrl = productData.ExternalShopUrl;
                            }

                        }
                        */
                    }
                    responseViewModel.Data = cartProductsList;
                    return responseViewModel;
                }
                else
                {
                    responseViewModel.Successful = false;
                    responseViewModel.Message = "Fail to Get cart Products [001]";
                    return responseViewModel;
                }
            }
            else
            {
                return result;
            }
        }

        [HttpPost]
        [Route("/cart/get-shipping-cost-for-cart-products")]
        public async Task<ApiResponseViewModel> GetShippingCostForCartProducts([FromBody] ShippingCostQuery command)
        {
            if (command == null)
            {
                var responseViewModel = new ApiResponseViewModel();
                responseViewModel.Successful = false;
                responseViewModel.Message = "Fail to Get cart Products cost [001]";
                return responseViewModel;
            }
            var masterMemberProfile = GetMasterMemberId(Request.HttpContext);
            ApiResponseViewModel result = await Mediator.Send(new MasterMemberProfileDetailsQuery() { MasterMemberProfileId = masterMemberProfile });
            if (result.Successful)
            {

                // TODO get max order quantity filters
                var maxOrderFilter = await Mediator.Send(new MaxOrderFilterQuery());
                var appConfig = await Mediator.Send(new AppConfigQuery());

                
                command.MasterMemberProfileId = masterMemberProfile;
                OrderShippingCostsModel orderShippingCostsModel = new OrderShippingCostsModel();
                ApiResponseViewModel responseViewModel = await Mediator.Send(command);
                if(responseViewModel.Data != null)
                {
                    orderShippingCostsModel = (OrderShippingCostsModel)responseViewModel.Data;
                }

                if (result.Successful)
                {
                    var externalItems = command.ProductVariationIds.Where(x => !string.IsNullOrEmpty(x.ExternalItemId));

                    if (externalItems != null && externalItems.Any())
                    {
                        var master = JsonConvert.DeserializeObject<MasterMemberProfileViewModel>(JsonConvert.SerializeObject(result.Data));
                        foreach (var item in externalItems)
                        {
                            var externalShippingResult = await Mediator.Send(new AggregatorShippingCostQuery()
                            {
                                ExternalItemId = item.ExternalItemId,
                                ExternalShopId = item.ExternalShopId,
                                ExternalTypeId = item.ExternalTypeId,
                                City = master.City,
                                State = master.State
                            });

                            if (externalShippingResult.Successful)
                            {
                                var aggregatorData = JsonConvert.DeserializeObject<OrderShippingCostForPoductIdAndVariationIdModel>(externalShippingResult.Data.ToString());
                                if (orderShippingCostsModel.OrderShippingCosts.Where(x=>x.ExternalShopId == aggregatorData.ExternalShopId).Any())
                                {
                                   var ordersFromSameShop = orderShippingCostsModel.OrderShippingCosts.Where(x => x.ExternalShopId == aggregatorData.ExternalShopId && x.OrderShippingCost != 0).FirstOrDefault();
                                    if (ordersFromSameShop != null)
                                    {
                                        if (ordersFromSameShop.OrderShippingCost >= aggregatorData.OrderShippingCost)
                                        {
                                            aggregatorData.OrderShippingCost = 0;
                                        }
                                        else
                                        {
                                            ordersFromSameShop.OrderShippingCost = 0;
                                        }
                                    }
                                }
                                if(item.ProductPrice > 100)
                                {
                                    aggregatorData.OrderShippingCost = item.Quantity * aggregatorData.OrderShippingCost;
                                }
                                else
                                {
                                    var maxOrderFilterForProduct = maxOrderFilter.Where(x => item.ProductTitle.ToLower().Contains(x.Keyword)).FirstOrDefault();
                                    if (maxOrderFilterForProduct != null)
                                    {
                                        var numberOfShippingRequired = Math.Ceiling((decimal)item.Quantity / maxOrderFilterForProduct.MaxQuantity);
                                        if (numberOfShippingRequired > 1)
                                        {
                                            aggregatorData.OrderShippingCost = numberOfShippingRequired * aggregatorData.OrderShippingCost;
                                        }
                                    }
                                    else
                                    {
                                        var numberOfShippingRequired = Math.Ceiling((decimal)item.Quantity / appConfig.MaxOrderFilter);
                                        if (numberOfShippingRequired > 1)
                                        {
                                            aggregatorData.OrderShippingCost = numberOfShippingRequired * aggregatorData.OrderShippingCost;
                                        }
                                    }
                                }
                               
                                orderShippingCostsModel.OrderShippingCosts.Add(aggregatorData);

                                orderShippingCostsModel.TotalShippingCost += Math.Max(aggregatorData.OrderShippingCost, 0);
                            }
                        }
                        if(orderShippingCostsModel.OrderShippingCosts != null && orderShippingCostsModel.OrderShippingCosts.Count > 0)
                        {
                            orderShippingCostsModel.TotalShippingCost = orderShippingCostsModel.OrderShippingCosts.Sum(x => x.OrderShippingCost);
                        }
                        
                    }
                    
                    responseViewModel.Data = orderShippingCostsModel;
                    responseViewModel.Message = responseViewModel.Message;
                    return responseViewModel;
                }
                else
                {
                    responseViewModel.Successful = false;
                    responseViewModel.Message = "Fail to Get cart Products cost [002]";
                    return responseViewModel;
                }
            }
            else
            {
                return result;
            }
        }

        [HttpGet]
        [Route("/cart/get-cart-products-count")]
        public async Task<ApiResponseViewModel> GetCartProductsCount()
        {
            var masterMemberProfile = GetMasterMemberId(Request.HttpContext);
            ApiResponseViewModel result = await Mediator.Send(new MasterMemberProfileDetailsQuery() { MasterMemberProfileId = masterMemberProfile });
            if (result.Successful)
            {
                ApiResponseViewModel responseViewModel = await Mediator.Send(new GetCartProductsCountQuery() { MasterMemberProfileId = masterMemberProfile });
                if (result.Successful)
                {

                    responseViewModel.Data = (int)responseViewModel.Data;
                    responseViewModel.Message = responseViewModel.Message;
                    return responseViewModel;
                }
                else
                {
                    responseViewModel.Successful = false;
                    responseViewModel.Message = "Fail to Get cart Products count [002]";
                    return responseViewModel;
                }
            }
            else
            {
                return result;
            }
        }

        [HttpPost]
        [Route("/cart/add-to-cart")]
        public async Task<ApiResponseViewModel> AddToCart([FromForm] AddToCartCommand command)
        {
            var masterMemberProfile = GetMasterMemberId(Request.HttpContext);
            ApiResponseViewModel result = await Mediator.Send(new MasterMemberProfileDetailsQuery() { MasterMemberProfileId = masterMemberProfile });
            if (result.Successful)
            {
                command.MasterMemberProfileId = masterMemberProfile;
                ApiResponseViewModel responseViewModel = await Mediator.Send(command);
                return responseViewModel;
            }
            else
            {
                return result;
            }
        }

        [HttpPost]
        [Route("/cart/add-to-cart-external")]
        public async Task<ApiResponseViewModel> AddToCartExternal([FromForm] AddToCartExternalCommand command)
        {
            var masterMemberProfile = GetMasterMemberId(Request.HttpContext);
            ApiResponseViewModel result = await Mediator.Send(new MasterMemberProfileDetailsQuery() { MasterMemberProfileId = masterMemberProfile });
            if (result.Successful)
            {
                command.MasterMemberProfileId = masterMemberProfile;
                ApiResponseViewModel responseViewModel = await Mediator.Send(command);
                return responseViewModel;
            }
            else
            {
                return result;
            }
        }

        [HttpPost]
        [Route("/cart/delete-from-cart")]
        public async Task<ApiResponseViewModel> DeleteFromCart(int cartProductId)
        {
            var masterMemberProfile = GetMasterMemberId(Request.HttpContext);
            ApiResponseViewModel result = await Mediator.Send(new MasterMemberProfileDetailsQuery() { MasterMemberProfileId = masterMemberProfile });
            if (result.Successful)
            {
                ApiResponseViewModel responseViewModel = await Mediator.Send(new DelateFromCartCommand() { CartProductId = cartProductId });
                return responseViewModel;
            }
            else
            {
                return result;
            }
        }

        [HttpPost]
        [Route("/cart/delete-from-cart-external")]
        public async Task<ApiResponseViewModel> DeleteFromCartExternal(Guid id)
        {
            var masterMemberProfile = GetMasterMemberId(Request.HttpContext);
            ApiResponseViewModel result = await Mediator.Send(new MasterMemberProfileDetailsQuery() { MasterMemberProfileId = masterMemberProfile });
            if (result.Successful)
            {
                ApiResponseViewModel responseViewModel = await Mediator.Send(new DeleteFromCartExternalCommand() { Id = id, MasterMemberProfileId = masterMemberProfile });
                return responseViewModel;
            }
            else
            {
                return result;
            }
        }
    }
}