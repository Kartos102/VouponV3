using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.ThirdParty.Pages
{
    public class ThirdPartyPage : IRequest<ApiResponseViewModel>
    {
        public int ProductId { get; set; }
        private class ThirdPartyProductPageHandler : IRequestHandler<ThirdPartyPage, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ThirdPartyProductPageHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ThirdPartyPage request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                var thirdPartyTypes = rewardsDBContext.ThirdPartyTypes.Include(x => x.ThirdPartyProducts).ToListAsync().Result.Select(x => new ThirdPartyTypeViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Status = x.Status,
                    Products = x.ThirdPartyProducts.Select(z => new ThirdPartyProductViewModel
                    {
                        Id = z.Id,
                        ExternalId = z.ExternalId,
                        Name = z.Name,
                        Status = z.Status,
                        ThirdPartyTypeId = z.ThirdPartyTypeId
                    }).ToList()
                }).ToList();

                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Data = thirdPartyTypes;
                return apiResponseViewModel;
            }
        }
    }

    public class ThirdPartyTypeViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public short Status { get; set; }

        public List<ThirdPartyProductViewModel> Products { get; set; }
    }

    public class ThirdPartyProductViewModel
    {
        public Guid Id { get; set; }
        public Guid ThirdPartyTypeId { get; set; }
        public string ExternalId { get; set; }
        public string Name { get; set; }
        public short Status { get; set; }

    }

}
