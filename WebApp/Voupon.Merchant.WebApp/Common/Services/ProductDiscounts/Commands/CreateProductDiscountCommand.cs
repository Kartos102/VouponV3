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
    public class CreateProductDiscountCommand : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        public int DiscountTypeId { get; set; }
        public decimal PriceValue { get; set; }
        public decimal PercentageValue { get; set; }
        public int PointRequired { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
        public DateTime CreatedAt { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
    public class CreateProductDiscountCommandHandler : IRequestHandler<CreateProductDiscountCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CreateProductDiscountCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CreateProductDiscountCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var item = new Voupon.Database.Postgres.RewardsEntities.ProductDiscounts();
                item.ProductId = request.ProductId;
                item.Name = request.Name;
                item.DiscountTypeId = request.DiscountTypeId;
                item.PointRequired = request.PointRequired;
                item.PriceValue = request.PriceValue;
                item.PercentageValue = request.PercentageValue;
                item.StartDate = request.StartDate;
                item.EndDate = request.EndDate;              
                item.CreatedAt = request.CreatedAt;
                item.CreatedByUserId = request.CreatedByUserId;
                item.IsActivated = false;
                await rewardsDBContext.ProductDiscounts.AddAsync(item);
                rewardsDBContext.SaveChanges();             
                response.Successful = true;
                response.Message = "Create Product Discount Successfully";
                response.Data = item;
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }

            return response;
        }
    }
}
