using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Services.Sample.Create.Commands;
using Voupon.Merchant.WebApp.Services.Sample.Queries.List;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Controllers
{
    [Route("[controller]")]
    public class SampleController : BaseController
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("FileUpload")]
        public IActionResult FileUpload()
        {
            return View();
        }

        [HttpPost]
        [Route("FileUpload")]
        public async Task<IActionResult> FileUploadPost()
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            var fileContent = HttpContext.Request.Form.Files["newImage"];
            if (fileContent == null)
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Invalid request";
                return BadRequest(apiResponseViewModel);
            }

            var result = await Mediator.Send(new CreateFileUploadCommand
            {
                File = fileContent,
                Id = "Some id that you need to pass in"
            });

            apiResponseViewModel.Message = "Succesfully uploaded";
            return Ok(apiResponseViewModel);
        }

        [HttpGet]
        [Route("SMS")]
        public IActionResult SMS()
        {
            return View();
        }

        [HttpPost]
        [Route("SMS")]
        public async Task<IActionResult> SMSPost(SendSMSCommand command)
        {
            var result = await Mediator.Send(command);

            if (result.Successful)
            {
                return Ok(result);
            }

            return BadRequest(result.Message);
        }

        [HttpGet]
        [Route("BlobList")]
        public async Task<IActionResult> BlobList()
        {
            return View(await Mediator.Send(new ListAzureBlobQuery()));
        }
    }
}