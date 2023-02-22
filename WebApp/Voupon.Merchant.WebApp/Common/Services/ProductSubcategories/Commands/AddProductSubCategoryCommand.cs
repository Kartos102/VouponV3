using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductSubcategories.Commands
{
    public class AddProductSubCategoryCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductCategoryId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
    }

    public class AddProductSubCategoryCommandHandler : IRequestHandler<AddProductSubCategoryCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public AddProductSubCategoryCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(AddProductSubCategoryCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            ProductSubCategories newCategory= new ProductSubCategories();
            try
            { 
                newCategory.Name = request.Name;
                newCategory.CategoryId = request.ProductCategoryId;
                newCategory.CreatedAt = request.CreatedAt;
                newCategory.CreatedByUserId = request.CreatedByUserId;
                await rewardsDBContext.ProductSubCategories.AddAsync(newCategory);
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Add Product Sub-Category Successfully";
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }              
            return response;
        }
    }
}
