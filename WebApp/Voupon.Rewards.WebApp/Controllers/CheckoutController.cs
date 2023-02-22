using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Voupon.Rewards.WebApp.Infrastructures.Enums;
using Voupon.Rewards.WebApp.Services.Checkout.Commands;
using Voupon.Rewards.WebApp.Services.Checkout.Pages;
using Voupon.Rewards.WebApp.Services.Checkout.Queries;
using Voupon.Rewards.WebApp.Services.Logger;
using Voupon.Rewards.WebApp.ViewModels.ThirdParty.IPay88;
using Voupon.Rewards.WebApp.ViewModels.ThirdParty.RevenueMonster.Callback;
using Voupon.Rewards.WebApp.ViewModels.ThirdParty.RevenueMonster.RevenueMonsterQueryById;
using static Voupon.Rewards.WebApp.Services.Checkout.Commands.CreatePaymentCommand;
using static Voupon.Rewards.WebApp.Services.Checkout.Pages.PaymentPage;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Voupon.Rewards.WebApp.Controllers
{
    public class CheckoutController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [Route("/checkout/payment/{id}")]
        public async Task<IActionResult> Payment(PaymentPage page)
        {
            page.MasterMemberProfileId = GetMasterMemberId(Request.HttpContext);
            var appConfigResult = await Mediator.Send(new GetPassPaymentGatewayVariable());
            page.IsTestPassPaymentGateway = (bool)appConfigResult.Data;
            var result = await Mediator.Send(page);
            if ((bool)appConfigResult.Data)
            {
                var paymentParameters = (OrderViewModel)result.Data;
                return RedirectToAction("TestPassPaymentGateway", paymentParameters.Ipay88RequestViewModel);

                //PassPaymentGateway();
            }
            if (result.Successful)
            {

                return View("~/views/checkout/payment.cshtml", (OrderViewModel)result.Data);
            }
            else
            {

                return View("~/views/checkout/payment.cshtml", new OrderViewModel
                {

                    ErrorMessage = result.Message
                });
            }
            //return View(ErrorPageEnum.INVALID_REQUEST_PAGE);
        }

        [Route("/checkout/response")]
        public async Task<IActionResult> CheckoutResponse()
        {
            var error = "";
            var command = new CreatePaymentCommand();
            try
            {
                if (!User.Identity.IsAuthenticated)
                {
                    ViewData["Error"] = "Failed to complete payment. Please relogin.";
                    return View();
                }

                var status = HttpContext.Request.Query["status"];
                var shortId = HttpContext.Request.Query["orderId"];

                if (string.IsNullOrEmpty(status))
                {
                    ViewData["Error"] = "Failed to complete payment. Please try again.";
                    return View();
                }

                if (status == "SUCCESS")
                {
                    /*
                    var result = await Mediator.Send(new RevenueMonsterPaymentStatusByOrderIdQuery
                    {
                        ShortId = shortId
                    });

                    if(result.Successful)
                    {

                    }
                    */
                    return View();
                }

                if (status == "CANCELLED")
                {
                    ViewData["Error"] = "Transaction has been cancelled";
                    return View();
                }

                if (status == "SUCCESS")
                {
                    /*
                    var result = await Mediator.Send(new RevenueMonsterPaymentStatusByOrderIdQuery
                    {
                        ShortId = shortId
                    });

                    if(result.Successful)
                    {

                    }
                    */
                    return View();
                }

                ViewData["Error"] = "Failed to complete payment. Please try again.";
                return View();
            }
            catch (Exception ex)
            {
                await Mediator.Send(new CreateErrorLogCommand
                {
                    ActionName = "/checkout/response",
                    TypeId = CreateErrorLogCommand.Type.Controller,
                    Email = User.Identity.Name,
                    ActionRequest = JsonSerializer.Serialize(command),
                    Errors = ex.ToString()
                });

                ViewData["Error"] = ex.ToString();
                return View();
            }
        }

        [Route("/api/v1/thirdparty/revenuemonster/regenFromAdmin")]
        public async Task<IActionResult> RevenueMonsterCallbackRegenFromAdmin([FromBody] JsonElement content)
        {
            var command = new CreateRevenueMonsterPaymentCommand();
            try
            {
                var callbackViewModel = JsonConvert.DeserializeObject<RevenueMonsterQueryByIdModel>(content.ToString());

                if (callbackViewModel.Item.Status != "SUCCESS")
                {
                    await Mediator.Send(new CreateErrorLogCommand
                    {
                        ActionName = "/api/v1/thirdparty/revenuemonster/callback",
                        TypeId = CreateErrorLogCommand.Type.Controller,
                        ActionRequest = content.ToString(),
                        Errors = "Payment not successful"
                    });
                }

                var paymentData = callbackViewModel.Item;
                command = new CreateRevenueMonsterPaymentCommand
                {
                    IsRegenFromAdmin = true,
                    PaymentProviderId = 2,
                    MerchantCode = paymentData.Store.Id,
                    PaymentId = 0,
                    ShortOrderId = paymentData.Order.Id,
                    Amount = ((decimal)paymentData.Order.Amount / 100),
                    Currency = paymentData.CurrencyType,
                    TransactionId = paymentData.TransactionId,
                    Status = paymentData.Status,
                    JsonResponse = JsonSerializer.Serialize(callbackViewModel)
                };
                command.JsonResponse = JsonSerializer.Serialize(command);

                return new OkObjectResult(await Mediator.Send(command));

            }
            catch (Exception ex)
            {
                await Mediator.Send(new CreateErrorLogCommand
                {
                    ActionName = "/api/v1/thirdparty/revenuemonster/callback",
                    TypeId = CreateErrorLogCommand.Type.Controller,
                    ActionRequest = content.ToString(),
                    Errors = ex.ToString()
                });

                return new BadRequestObjectResult(ex.ToString());
            }
        }


        [Route("/api/v1/thirdparty/revenuemonster/callback")]
        public async Task<IActionResult> RevenueMonsterCallback([FromBody] JsonElement content)
        {
            var command = new CreateRevenueMonsterPaymentCommand();
            try
            {
                var callbackViewModel = JsonConvert.DeserializeObject<RevenueMonsterCallback>(content.ToString());

                if (callbackViewModel.Data.Status != "SUCCESS")
                {
                    await Mediator.Send(new CreateErrorLogCommand
                    {
                        ActionName = "/api/v1/thirdparty/revenuemonster/callback",
                        TypeId = CreateErrorLogCommand.Type.Controller,
                        ActionRequest = content.ToString(),
                        Errors = "Payment not successful"
                    });
                }

                var paymentData = callbackViewModel.Data;
                command = new CreateRevenueMonsterPaymentCommand
                {
                    IsRegenFromAdmin = false,
                    PaymentProviderId = 2,
                    MerchantCode = paymentData.Store.Id,
                    PaymentId = 0,
                    ShortOrderId = paymentData.Order.Id,
                    Amount = ((decimal)paymentData.Order.Amount / 100),
                    Currency = paymentData.CurrencyType,
                    TransactionId = paymentData.TransactionId,
                    Status = paymentData.Status,
                    JsonResponse = JsonSerializer.Serialize(callbackViewModel)
                };
                command.JsonResponse = JsonSerializer.Serialize(command);

                var result = await Mediator.Send(command);
                return View();

            }
            catch (Exception ex)
            {
                await Mediator.Send(new CreateErrorLogCommand
                {
                    ActionName = "/api/v1/thirdparty/revenuemonster/callback",
                    TypeId = CreateErrorLogCommand.Type.Controller,
                    ActionRequest = content.ToString(),
                    Errors = ex.ToString()
                });

                ViewData["Result"] = ex.ToString();
                return View();
            }
        }

        [Route("/api/v1/thirdparty/ipay88/callback")]
        public async Task<IActionResult> IPay88Callback()
        {
            var callbackResponse = "";
            var command = new CreatePaymentCommand();
            try
            {
                if (!Request.HasFormContentType || Request.Form == null)
                {
                    ViewData["Result"] = "Failed to complete payment. Please try again";
                    return View();
                }

                if (Request.Form["ErrDesc"].Any())
                {
                    callbackResponse = Request.Form["ErrDesc"];
                }

                if (!string.IsNullOrEmpty(callbackResponse))
                {
                    ViewData["Result"] = callbackResponse;
                    return View();
                }

                command = new CreatePaymentCommand
                {
                    PaymentProviderId = 1,
                    MerchantCode = Request.Form["MerchantCode"],
                    PaymentId = int.Parse(Request.Form["PaymentId"]),
                    RefNo = new Guid(Request.Form["RefNo"]),
                    Amount = decimal.Parse(Request.Form["Amount"]),
                    Currency = Request.Form["Currency"],
                    Remark = Request.Form["Remark"],
                    TransactionId = Request.Form["TransId"],
                    AuthCode = Request.Form["AuthCode"],
                    Status = Request.Form["Status"],
                    ErrorDescription = Request.Form["ErrDesc"],
                    Signature = Request.Form["Signature"],
                    CreditCardName = Request.Form["CCName"],
                    CreditCardNumber = Request.Form["CCNo"],
                    CreditCardBankName = Request.Form["S_bankname"],
                    CreditCardIssuingCountry = Request.Form["S_country"]
                };

                if (command.Status != "1")
                {
                    ViewData["Result"] = "Payment was not completed";
                    return View();
                }

                if (Request.Form["CCName"].Any())
                {
                    command.CreditCardName = Request.Form["CCName"];
                }

                if (Request.Form["CCNo"].Any())
                {
                    command.CreditCardNumber = Request.Form["CCNo"];
                }

                if (Request.Form["S_bankname"].Any())
                {
                    command.CreditCardBankName = Request.Form["S_bankname"];
                }

                if (Request.Form["S_country"].Any())
                {
                    command.CreditCardIssuingCountry = Request.Form["S_country"];
                }

                /*
                var command = new CreatePaymentCommand
                {
                    MerchantCode = "M17892",
                    PaymentId = 2,
                    RefNo = new Guid("9F683DDC-3E03-41E4-BC91-42332A4DBB5F"),
                    Amount = (decimal)1.00,
                    Currency = "MYR",
                    Remark =null,
                    TransactionId ="1zzz222aaaaaaa",
                    AuthCode = "123",
                    Status = "1",
                    ErrorDescription = null,
                    Signature = "ssfasd12sadadads",
                    CreditCardName = "UNG KOK HONG",
                    CreditCardNumber = "1234444455556666",
                    CreditCardBankName = "KOK BANK",
                    CreditCardIssuingCountry = "MALAYSIA"
                };
                */
                command.JsonResponse = JsonSerializer.Serialize(command);

                var result = await Mediator.Send(command);

                if (result.Successful)
                {
                    ViewData["Result"] = "RECEIVEOK";
                    return View();
                }
                else
                {
                    ViewData["Result"] = "RECEIVEOK";
                    return View();
                }

            }
            catch (Exception ex)
            {
                await Mediator.Send(new CreateErrorLogCommand
                {
                    ActionName = "/api/v1/thirdparty/ipay88/callback",
                    TypeId = CreateErrorLogCommand.Type.Controller,
                    ActionRequest = JsonSerializer.Serialize(command),
                    Errors = ex.ToString()
                });

                ViewData["Result"] = ex.ToString();
                return View();
            }
        }

        [Route("/api/v1/thirdparty/testPayment/PaymentCallback")]
        public async Task<IActionResult> TestPassPaymentGateway(IPay88RequestViewModel orderParameters)
        {
            var appConfigResult = await Mediator.Send(new GetPassPaymentGatewayVariable());
            if (!(bool)appConfigResult.Data)
            {
                ViewData["Result"] = "Payment Failed Test Payment Is not turned on";
                return View();
            }
            var command = new CreatePaymentCommand();
            try
            {
                if (orderParameters == null)
                {
                    ViewData["Result"] = "Failed to complete payment. Please try again";
                    return View();
                }

                command = new CreatePaymentCommand
                {
                    PaymentProviderId = 1,
                    MerchantCode = orderParameters.MerchantCode,
                    PaymentId = orderParameters.PaymentId,
                    RefNo = new Guid(orderParameters.RefNo),
                    Amount = decimal.Parse(orderParameters.Amount),
                    Currency = orderParameters.Currency,
                    Remark = "TEST PAYMENT",
                    TransactionId = "XTestTransactionIdX" + orderParameters.RefNo,
                    AuthCode = "XXXXXXXTestAuthXXXXXXXX",
                    Status = "SUCCESS",
                    //ErrorDescription = Request.Form["ErrDesc"],
                    Signature = orderParameters.Signature,
                    //CreditCardName = Request.Form["CCName"],
                    //CreditCardNumber = Request.Form["CCNo"],
                    //CreditCardBankName = Request.Form["S_bankname"],
                    //CreditCardIssuingCountry = Request.Form["S_country"]
                };

                //if (Request.Form["CCName"].Any())
                //{
                //    command.CreditCardName = Request.Form["CCName"];
                //}

                //if (Request.Form["CCNo"].Any())
                //{
                //    command.CreditCardNumber = Request.Form["CCNo"];
                //}

                //if (Request.Form["S_bankname"].Any())
                //{
                //    command.CreditCardBankName = Request.Form["S_bankname"];
                //}

                //if (Request.Form["S_country"].Any())
                //{
                //    command.CreditCardIssuingCountry = Request.Form["S_country"];
                //}

                /*
                var command = new CreatePaymentCommand
                {
                    MerchantCode = "M17892",
                    PaymentId = 2,
                    RefNo = new Guid("9F683DDC-3E03-41E4-BC91-42332A4DBB5F"),
                    Amount = (decimal)1.00,
                    Currency = "MYR",
                    Remark =null,
                    TransactionId ="1zzz222aaaaaaa",
                    AuthCode = "123",
                    Status = "1",
                    ErrorDescription = null,
                    Signature = "ssfasd12sadadads",
                    CreditCardName = "UNG KOK HONG",
                    CreditCardNumber = "1234444455556666",
                    CreditCardBankName = "KOK BANK",
                    CreditCardIssuingCountry = "MALAYSIA"
                };
                */
                command.JsonResponse = JsonSerializer.Serialize(command);

                var result = await Mediator.Send(command);

                if (result.Successful)
                {
                    ViewData["Result"] = "RECEIVEOK</br> Amount: " + decimal.Parse(orderParameters.Amount);
                    return View();
                }
                else
                {
                    ViewData["Result"] = "RECEIVEFAIL";
                    return View();
                }

            }
            catch (Exception ex)
            {
                await Mediator.Send(new CreateErrorLogCommand
                {
                    ActionName = "/api/v1/thirdparty/testPayment/PaymentCallback",
                    TypeId = CreateErrorLogCommand.Type.Controller,
                    ActionRequest = JsonSerializer.Serialize(command),
                    Errors = ex.ToString()
                });

                ViewData["Result"] = ex.ToString();
                return View();
            }
        }
    }
}