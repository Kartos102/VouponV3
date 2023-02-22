using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Areas.Admin.Services.Merchants.ViewModels;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Locations.Commands.Create
{
    public class CreatePostcodeCommand : IRequest<ApiResponseViewModel>
    {
        public int DistrictId { get; set; }        
        public string Name { get; set; }
    }

    public class CreatePostcodeCommandHandler : IRequestHandler<CreatePostcodeCommand, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public CreatePostcodeCommandHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(CreatePostcodeCommand request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                PostCodes postcode = new PostCodes();
                postcode.DistrictId = request.DistrictId;
                postcode.Name = request.Name;
                postcode.CreatedAt = DateTime.Now;
                postcode.CreatedByUserId = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
                postcode.IsActivated = true;
                await rewardsDBContext.PostCodes.AddAsync(postcode);
                rewardsDBContext.SaveChanges();
                response.Successful = true;
                response.Message = "Add postcode Successfully";
            }
            catch(Exception ex)
            {
                response.Message = "Add postcode failed";
            }            
            return response;
        }
    }
}
