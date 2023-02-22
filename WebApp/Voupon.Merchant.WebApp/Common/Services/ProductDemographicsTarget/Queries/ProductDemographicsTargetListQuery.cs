using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.Common.Services.Postcodes.Models;
using Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductDemographicsTarget.Queries
{  
    public class ProductDemographicsTargetListQuery : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
    }
    public class ProductDemographicsTargetListQueryHandler : IRequestHandler<ProductDemographicsTargetListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        VodusV2Context vodusV2Context;

        public ProductDemographicsTargetListQueryHandler(RewardsDBContext rewardsDBContext, VodusV2Context vodusV2Context)
        {
            this.rewardsDBContext = rewardsDBContext;
            this.vodusV2Context = vodusV2Context;
        }

        public async Task<ApiResponseViewModel> Handle(ProductDemographicsTargetListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                List<int> selectedDemographicsTargets = new List<int>() { 1, 3, 8 };
                var demographics = await vodusV2Context.DemographicTypes.Select(y => new { y.Id, y.Name, DemographicValues = y.DemographicValues.Where(z => z.IsActive == true )}).Where(x => selectedDemographicsTargets.Contains(x.Id) ).ToListAsync();

                var items = await rewardsDBContext.ProductDemographicsTarget.Where(x => x.ProductId == request.ProductId && selectedDemographicsTargets.Contains(x.DemographicTypeId)).ToListAsync();

                ProductDemographicsTargetModel productDemographicsTargetModel = new ProductDemographicsTargetModel();
                productDemographicsTargetModel.ProductId = request.ProductId;
                productDemographicsTargetModel.ProductDemographicsList = new List<ProductDemographicsList>();
                productDemographicsTargetModel.ProductDemographicTargets = new List<ProductDemographicTargets>();

                foreach (var ProductDemographictarget in items)
                {
                    ProductDemographicTargets productDemographicTargetsModel = new ProductDemographicTargets();
                    productDemographicTargetsModel.Id = ProductDemographictarget.Id;
                    productDemographicTargetsModel.DemographicTypeId = ProductDemographictarget.DemographicTypeId;
                    productDemographicTargetsModel.DemographicValue = ProductDemographictarget.DemographicValue;

                    productDemographicsTargetModel.ProductDemographicTargets.Add(productDemographicTargetsModel);
                }

                foreach (var demographic in demographics)
                {
                    ProductDemographicsList ProductDemographic = new ProductDemographicsList();
                    ProductDemographic.Id = demographic.Id;
                    ProductDemographic.Name = demographic.Name;
                    ProductDemographic.DemographicsValues = new List<DemographicsValues>();
                    foreach (var demographicValue in demographic.DemographicValues)
                    {
                        DemographicsValues demographicsValueModel = new DemographicsValues
                        {
                            Id = demographicValue.Id,
                            Name = demographicValue.DisplayValue
                        };
                        ProductDemographic.DemographicsValues.Add(demographicsValueModel) ;
                    }

                        productDemographicsTargetModel.ProductDemographicsList.Add(ProductDemographic);
                }
                response.Successful = true;
                response.Data = productDemographicsTargetModel;
                response.Message = "Get Product demographic target List Successfully";
            }
            catch (Exception ex)
            {
                response.Successful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
