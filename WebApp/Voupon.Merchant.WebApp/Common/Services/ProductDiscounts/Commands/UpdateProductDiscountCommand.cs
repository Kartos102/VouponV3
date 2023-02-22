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
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.ProductDiscounts.Command
{
    public class UpdateProductDiscountCommand : IRequest<ApiResponseViewModel>
    {
        public int Id { get; set; }
        public int DiscountTypeId { get; set; }
        public decimal PriceValue { get; set; }
        public decimal PercentageValue { get; set; }
        public int PointRequired { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public DateTime LastUpdatedAt { get; set; }
        public Guid LastUpdatedByUserId { get; set; }
    }
    public class UpdateProductDiscountCommandHandler : IRequestHandler<UpdateProductDiscountCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public UpdateProductDiscountCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(UpdateProductDiscountCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
               var item= await rewardsDBContext.ProductDiscounts.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if(item!=null)
                {                  
                    item.Name = request.Name;
                    item.DiscountTypeId = request.DiscountTypeId;
                    item.PointRequired = request.PointRequired;
                    item.PriceValue = request.PriceValue;
                    item.PercentageValue = request.PercentageValue;
                    item.StartDate = request.StartDate;
                    item.EndDate = request.EndDate;
                    item.LastUpdatedAt = request.LastUpdatedAt;
                    item.LastUpdatedByUserId = request.LastUpdatedByUserId;
                    await rewardsDBContext.SaveChangesAsync();
                    response.Successful = true;
                    response.Message = "Update Product Discount Successfully";
                    response.Data = item;
                }
                else
                {
                    response.Successful = false;
                    response.Message = "Product Discount not found";
                    response.Data = null;
                }                          
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
