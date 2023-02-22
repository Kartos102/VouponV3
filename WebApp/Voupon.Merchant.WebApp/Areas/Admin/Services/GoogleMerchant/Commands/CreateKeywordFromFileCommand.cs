using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Core.ExcelPackage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.GoogleMerchant.Commands
{
    public class CreateKeywordFromFileCommand : IRequest<ApiResponseViewModel>
    {
        public IFormFileCollection Files { get; set; }


        public class CreateKeywordFromFileCommandHandler : IRequestHandler<CreateKeywordFromFileCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext rewardsDBContext;


            public CreateKeywordFromFileCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(CreateKeywordFromFileCommand request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var list = new List<GoogleMerchantKeywords>();
                    //  TODO
                    /*
                    using (ExcelPackage excelPackage = new ExcelPackage(request.Files[0].OpenReadStream()))
                    {
                        //Get a WorkSheet by index. Note that EPPlus indexes are base 1, not base 0!
                        ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];

                        for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
                        {
                            if (i == 1)
                            {
                                continue;
                            }
                            var newKeyword = new GoogleMerchantKeywords
                            {
                                Id = Guid.NewGuid(),
                            };

                            //loop all columns in a row
                            for (int j = worksheet.Dimension.Start.Column; j <= worksheet.Dimension.End.Column; j++)
                            {
                                //add the cell data to the List
                                if (worksheet.Cells[i, j].Value != null)
                                {
                                    if (j == 1)
                                    {
                                        newKeyword.Keyword = worksheet.Cells[i, j].Value.ToString();
                                    }
                                    else if (j == 2)
                                    {
                                        newKeyword.TotalListing = int.Parse(worksheet.Cells[i, j].Value.ToString());
                                    }
                                    else if (j == 3)
                                    {
                                        newKeyword.Language = worksheet.Cells[i, j].Value.ToString().ToLower();
                                    }
                                    else if (j == 4)
                                    {
                                        newKeyword.SortBy = worksheet.Cells[i, j].Value.ToString().ToLower();
                                    }
                                }
                                list.Add(newKeyword);
                            }
                        }
                    }
                    */

                    if (list != null && list.Any())
                    {
                        await rewardsDBContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM GoogleMerchantKeywords");
                        await rewardsDBContext.GoogleMerchantKeywords.AddRangeAsync(list);
                        await rewardsDBContext.SaveChangesAsync();
                    }

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Message = "Successfully created keywords";
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = "Fail to create keyword";
                }

                return apiResponseViewModel;
            }
        }

    }
}
