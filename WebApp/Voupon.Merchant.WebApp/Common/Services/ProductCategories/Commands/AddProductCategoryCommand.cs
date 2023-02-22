using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductCategories.Commands
{
    public class AddProductCategoryCommand : IRequest<ApiResponseViewModel>
    {      
        public string Name { get; set; }   
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }

    }

    public class AddProductCategoryCommandHandler : IRequestHandler<AddProductCategoryCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public AddProductCategoryCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(AddProductCategoryCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();

            var newCategory= new Voupon.Database.Postgres.RewardsEntities.ProductCategories();
            try
            { 
                newCategory.Name = request.Name;
                newCategory.CreatedAt = request.CreatedAt;
                newCategory.CreatedByUserId = request.CreatedByUserId;
                await rewardsDBContext.ProductCategories.AddAsync(newCategory);
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Add Product Category Successfully";
            }
            catch(Exception ex)
            {
                response.Message = ex.Message;
            }              
            return response;
        }
    }
}
