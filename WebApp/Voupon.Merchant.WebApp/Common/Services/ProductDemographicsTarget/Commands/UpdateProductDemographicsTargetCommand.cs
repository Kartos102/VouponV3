using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Enum;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models;
using Voupon.Merchant.WebApp.ViewModels;
using Voupon.Database.Postgres.VodusEntities;

namespace Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Command
{
    public class UpdateProductDemographicsTargetCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
        public List<ProductDemographicTargets> ProductDemographicTargetsModels { get; set; }
    }
    public class pdateProductDemographicsTargetCommandHandler : IRequestHandler<UpdateProductDemographicsTargetCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        VodusV2Context vodusV2Context;
        public pdateProductDemographicsTargetCommandHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductDemographicsTargetCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                if (request.ProductDemographicTargetsModels != null && request.ProductDemographicTargetsModels.Count > 0)
                {
                    var oldProductDemographicTargets = await rewardsDBContext.ProductDemographicsTarget.Where(x => x.ProductId == request.ProductId).ToListAsync();
                    if (oldProductDemographicTargets != null && oldProductDemographicTargets.Count > 0)
                        rewardsDBContext.ProductDemographicsTarget.RemoveRange(oldProductDemographicTargets);

                    foreach (var productDemographic in request.ProductDemographicTargetsModels)
                    {
                        Voupon.Database.Postgres.RewardsEntities.ProductDemographicsTarget productDemographicsTarget = new Voupon.Database.Postgres.RewardsEntities.ProductDemographicsTarget()
                        {
                            IsActive = true,
                            ProductId = request.ProductId,
                            CreatedAt = request.CreatedAt,
                            CreatedByUserId = request.CreatedByUserId,
                            DemographicTypeId = productDemographic.DemographicTypeId,
                            DemographicValue = productDemographic.DemographicValue
                        };
                        rewardsDBContext.ProductDemographicsTarget.Add(productDemographicsTarget);
                    }
                }

                rewardsDBContext.SaveChanges();
                List<int> selectedDemographicsTargets = new List<int>() { 1, 3, 8 };
                var demographics = await vodusV2Context.DemographicTypes.Select(y => new { y.Id, y.Name, DemographicValues = y.DemographicValues.Where(z => z.IsActive == true) }).Where(x => selectedDemographicsTargets.Contains(x.Id)).ToListAsync();
                var productDemographicTargets = await rewardsDBContext.ProductDemographicsTarget.Where(x => x.ProductId == request.ProductId && selectedDemographicsTargets.Contains(x.DemographicTypeId)).ToListAsync();
                int counter = 0;
                int skipDemo = 0;
                string productTargetsText = "";
                List<ProductDemographicTargets> productDemographicTargetsList = new List<ProductDemographicTargets>();
                foreach (var productDemographicTarget in productDemographicTargets)
                {
                    ProductDemographicTargets productDemographicTargetsModel = new ProductDemographicTargets();
                    productDemographicTargetsModel.Id = productDemographicTarget.Id;
                    productDemographicTargetsModel.DemographicTypeId = productDemographicTarget.DemographicTypeId;
                    productDemographicTargetsModel.DemographicValue = productDemographicTarget.DemographicValue;
                    productDemographicTargetsList.Add(productDemographicTargetsModel);
                    if (skipDemo == productDemographicTarget.DemographicTypeId)
                    {
                        continue;
                    }
                    if (productDemographicTargets.Where(x => x.DemographicTypeId == productDemographicTarget.DemographicTypeId).Count() == demographics.Where(x => x.Id == productDemographicTarget.DemographicTypeId).Select(x => x.DemographicValues.Count()).ToArray().Sum())
                    {
                        skipDemo = productDemographicTarget.DemographicTypeId;
                        if (counter != 0)
                        {
                            productTargetsText += ", ";
                        }
                        if (productDemographicTarget.DemographicTypeId == 1)
                            productTargetsText += "All Ages";
                        else if (productDemographicTarget.DemographicTypeId == 3)
                            productTargetsText += "Both Genders";
                        else if (productDemographicTarget.DemographicTypeId == 8)
                            productTargetsText += "All Races";

                        counter += productDemographicTargets.Where(x => x.DemographicTypeId == productDemographicTarget.DemographicTypeId).Count();
                        continue;
                    }

                    if (counter != 0)
                    {
                        productTargetsText += ", ";
                    }
                    productTargetsText += demographics.Where(x => x.Id == productDemographicTarget.DemographicTypeId).FirstOrDefault().DemographicValues.Where(x => x.Id == productDemographicTarget.DemographicValue).FirstOrDefault().DisplayValue;
                    counter++;
                }
                if (productDemographicTargets.Count() == demographics.Select(x => x.DemographicValues.Count()).ToArray().Sum())
                {
                    productTargetsText = "All Ages, Both Genders, All Races";
                }
                ProductDemographicsTargetModelForUpdate productDemographicsTargetModelForUpdate = new ProductDemographicsTargetModelForUpdate() { ProductTargetsText = productTargetsText, ProductDemographicTargets = productDemographicTargetsList };
                response.Data = productDemographicsTargetModelForUpdate;
                response.Successful = true;
                response.Message = "Update Product demographic targets Successfully";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
