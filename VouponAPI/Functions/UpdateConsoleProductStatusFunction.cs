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
    public class UpdateConsoleProductStatusFunction
    {
        private readonly RewardsDBContext _rewardsDBContext;
        private readonly IAzureBlobStorage _azureBlobStorage;

        public UpdateConsoleProductStatusFunction(RewardsDBContext rewardsDBContext, IAzureBlobStorage azureBlobStorage)
        {
            _rewardsDBContext = rewardsDBContext;
            _azureBlobStorage = azureBlobStorage;
        }

        [FunctionName("UpdateConsoleProductStatusFunction")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "PUT", Route = "console/product/status")] HttpRequest req, ILogger log)
        {
            var response = new ApiResponseViewModel
            {
            };

            try
            {
                var request = HttpRequestHelper.DeserializeModel<UpdateProductStatusRequestModel>(req);

                var existingProductJSON = await _rewardsDBContext.ConsoleProductJSON.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (existingProductJSON == null)
                {
                    response.ErrorMessage = "Invalid product";
                    return new BadRequestObjectResult(response);

                }
                existingProductJSON.StatusId = request.StatusId;
                existingProductJSON.LastUpdatedAt = DateTime.Now;
                _rewardsDBContext.ConsoleProductJSON.Update(existingProductJSON);
                await _rewardsDBContext.SaveChangesAsync();


                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                //  log
                response.ErrorMessage = "Fail to create product json" + ex.ToString();
                return new BadRequestObjectResult(response);

            }
        }


        private class UpdateProductStatusRequestModel
        {
            public Guid Id { get; set; }
            public byte StatusId { get; set; }
            public string JSON { get; set; }

        }

    }
}
