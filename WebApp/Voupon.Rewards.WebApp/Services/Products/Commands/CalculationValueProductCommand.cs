using MediatR;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Products.Commands
{
    public class CalculationValueProductCommand : IRequest<ApiResponseViewModel>
    {
        public int DiscountId { get; set; }
        public bool IsVariant { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public int VariantId { get; set; }


        public class CalculatedValueViewModel { 
            public AdditionalDiscountViewModel AdditionalDiscount { get; set; }

            public decimal SubTotal { get; set; }

            public decimal TotalPrice { get; set; }

            public decimal DiscountedPrice { get; set; }

            public decimal Price { get; set; }

        }

        public class AdditionalDiscountViewModel { 
            public int Id { get; set; }
            public int Type { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
            public int PointsRequired { get; set; }
            public decimal VpointsMultiplier { get; set; }
            public decimal VPointsMultiplierCap { get; set; }


        }
        public class CalculationValueProductCommandHandler : IRequestHandler<CalculationValueProductCommand, ApiResponseViewModel>
        {
            private readonly RewardsDBContext _rewardsDBContext;

            public CalculationValueProductCommandHandler(RewardsDBContext rewardsDBContext)
            {
                this._rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(CalculationValueProductCommand request, CancellationToken cancellationToken)
            {
                ApiResponseViewModel response = new ApiResponseViewModel();
                var additionalDiscount = new AdditionalDiscountViewModel();

                CalculateFromPrice cfp = new CalculateFromPrice();
                CalcutalteObject calcutalteObject = new CalcutalteObject();
                try
                {
                    var discount = await _rewardsDBContext.ProductDiscounts.Where(x => x.Id == request.DiscountId).FirstOrDefaultAsync();
                    if (discount == null)
                    {
                        response.Successful = false;
                        response.Message = "Invalid discount id";
                        return response;
                    }

                    additionalDiscount.Id = discount.Id;
                    additionalDiscount.Type = discount.DiscountTypeId;
                    additionalDiscount.PointsRequired = discount.PointRequired;
                    additionalDiscount.Name = discount.Name;
                    if (discount.DiscountTypeId == 1)
                    {
                        additionalDiscount.Value = discount.PercentageValue;
                    }
                    else
                    {
                        additionalDiscount.Value = discount.PriceValue;
                    }

                    var appConfig = await _rewardsDBContext.AppConfig.FirstOrDefaultAsync();
                    var product = await _rewardsDBContext.Products.Where(x => x.Id == request.ProductId).FirstOrDefaultAsync();

                    additionalDiscount.VpointsMultiplier = appConfig.VPointsMultiplier;
                    additionalDiscount.VPointsMultiplierCap = appConfig.VPointsMultiplierCap;

                    if (request.IsVariant)
                    {
                        var variant = await _rewardsDBContext.ProductVariation.Where(x => x.Id == request.VariantId).FirstOrDefaultAsync();
                        if (discount != null)
                        {
                            if (product != null)
                            {

                                if (discount.DiscountTypeId == 1)
                                {
                                    cfp.SetCalculationtrategy(new PercentageCalculateStrategy());
                                    if (product.DealTypeId == 1)
                                    {
                                        calcutalteObject = cfp.Calculate(product.ActualPriceForVpoints.Value, request.Quantity, additionalDiscount.Value);
                                    }
                                    else
                                    {
                                        calcutalteObject = cfp.Calculate(variant.DiscountedPrice.Value, request.Quantity, additionalDiscount.Value);
                                    }
                                }
                                else
                                {
                                    cfp.SetCalculationtrategy(new ValueCalculateStrategy());
                                    if (product.DealTypeId == 1)
                                    {
                                        calcutalteObject = cfp.Calculate(product.ActualPriceForVpoints.Value, request.Quantity, additionalDiscount.Value);
                                    }
                                    else
                                    {
                                        calcutalteObject = cfp.Calculate(variant.DiscountedPrice.Value, request.Quantity, additionalDiscount.Value);
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (product.DealTypeId == 3)
                            {
                                cfp.SetCalculationtrategy(new PointCalculateStrategy());
                                calcutalteObject = cfp.Calculate(variant.DiscountedPrice.Value, request.Quantity, product.PointsRequired.Value);
                            }

                            if (product.DealTypeId == 1)
                            {
                                cfp.SetCalculationtrategy(new ValueCalculateStrategy());
                                calcutalteObject = cfp.Calculate(product.ActualPriceForVpoints.Value, request.Quantity, 0);
                            }
                            else
                            {
                                cfp.SetCalculationtrategy(new ValueCalculateStrategy());
                                calcutalteObject = cfp.Calculate(variant.DiscountedPrice.Value, request.Quantity, 0);
                            }

                        }
                        if (calcutalteObject.SubTotal < 0)
                            calcutalteObject.SubTotal = 0;
                        if (calcutalteObject.TotalPrice < 0)
                        {
                            calcutalteObject.TotalPrice = 0;
                            calcutalteObject.Price = calcutalteObject.TotalPrice;
                        }
                    }
                    else
                    {
                        if (discount != null)
                        {
                            if (product != null)
                            {

                                if (discount.DiscountTypeId == 1)
                                {
                                    cfp.SetCalculationtrategy(new PercentageCalculateStrategy());
                                    if (product.DealTypeId == 1)
                                    {
                                        calcutalteObject = cfp.Calculate(product.ActualPriceForVpoints.Value, request.Quantity, additionalDiscount.Value);
                                    }
                                    else
                                    {
                                        calcutalteObject = cfp.Calculate(product.DiscountedPrice.Value, request.Quantity, additionalDiscount.Value);
                                    }
                                }
                                else
                                {
                                    cfp.SetCalculationtrategy(new ValueCalculateStrategy());
                                    if (product.DealTypeId == 1)
                                    {
                                        calcutalteObject = cfp.Calculate(product.ActualPriceForVpoints.Value, request.Quantity, additionalDiscount.Value);
                                    }
                                    else
                                    {
                                        calcutalteObject = cfp.Calculate(product.DiscountedPrice.Value, request.Quantity, additionalDiscount.Value);
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (product.DealTypeId == 3)
                            {
                                cfp.SetCalculationtrategy(new PointCalculateStrategy());
                                calcutalteObject = cfp.Calculate(product.DiscountedPrice.Value, request.Quantity, product.PointsRequired.Value);
                            }

                            if (product.DealTypeId == 1)
                            {
                                cfp.SetCalculationtrategy(new ValueCalculateStrategy());
                                calcutalteObject = cfp.Calculate(product.ActualPriceForVpoints.Value, request.Quantity, 0);
                            }
                            else
                            {
                                cfp.SetCalculationtrategy(new ValueCalculateStrategy());
                                calcutalteObject = cfp.Calculate(product.DiscountedPrice.Value, request.Quantity, 0);
                            }

                        }
                        if (calcutalteObject.SubTotal < 0)
                            calcutalteObject.SubTotal = 0;
                        if (calcutalteObject.TotalPrice < 0)
                        {
                            calcutalteObject.TotalPrice = 0;
                            calcutalteObject.Price = calcutalteObject.TotalPrice;
                        }
                    }
                    var data = new CalculatedValueViewModel();
                    data.AdditionalDiscount = additionalDiscount;
                    data.SubTotal = calcutalteObject.SubTotal;
                    data.TotalPrice = calcutalteObject.TotalPrice;
                    data.DiscountedPrice = calcutalteObject.DiscountedPrice;
                    data.Price = calcutalteObject.Price;

                    response.Successful = true;
                    response.Data = data;

                    return response;
                }

                catch (Exception ex) {
                    response.Successful = false;
                    response.Message = ex.Message;
                    return response;
                }
            }




            public class CalcutalteObject { 
                public decimal SubTotal { get; set; }
                public decimal TotalPrice { get; set; }

                public decimal DiscountedPrice { get; set; }

                public decimal Price { get; set; }

            }

            public abstract class CalculateStrategy
            {
                public abstract CalcutalteObject Calculate(decimal discountedPrice,int quantity,decimal additionalDiscount);
            }

            public class PointCalculateStrategy : CalculateStrategy {

                public override CalcutalteObject Calculate(decimal price, int quantity, decimal pointRequired)
                {
                    CalcutalteObject obj = new CalcutalteObject();
                    obj.SubTotal = price * quantity;
                    obj.TotalPrice = price * quantity;
                    obj.Price = pointRequired * quantity;
                    return obj;
                }
            }
            public class PercentageCalculateStrategy : CalculateStrategy
            {
                public override CalcutalteObject Calculate(decimal price, int quantity, decimal additionalDiscount)
                {
                    CalcutalteObject obj = new CalcutalteObject();
                    obj.SubTotal = price * quantity;
                    obj.TotalPrice = Math.Round(price - Math.Round(price * (additionalDiscount / 100),2) * quantity, 2);
                    obj.DiscountedPrice = price;
                    obj.Price = Math.Round(price - Math.Round(price * (additionalDiscount / 100), 2) * quantity, 2);
                    return obj;
                }
            }

            public class ValueCalculateStrategy : CalculateStrategy
            {
                public override CalcutalteObject Calculate(decimal price, int quantity, decimal additionalDiscount)
                {
                    CalcutalteObject obj = new CalcutalteObject();
                    obj.SubTotal = price * quantity;
                    obj.TotalPrice = (price - additionalDiscount) * quantity;
                    obj.DiscountedPrice = price;
                    obj.Price = (price - additionalDiscount) * quantity;
                    return obj;
                }
            }

            public class CalculateFromPrice
            {
                private CalculateStrategy _strategy;
                public void SetCalculationtrategy(CalculateStrategy strategy)
                {
                    this._strategy = strategy;
                }
                public CalcutalteObject Calculate(decimal price, int quantity, decimal additionalDiscount)
                {
                    return _strategy.Calculate(price,quantity,additionalDiscount);

                }
            }




        }
    }
}
