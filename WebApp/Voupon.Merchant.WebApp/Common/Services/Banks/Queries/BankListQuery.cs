using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Banks.Models;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Common.Services.Banks.Queries
{   
    public class BankListQuery : IRequest<ApiResponseViewModel>
    {
    }
    public class BankListQueryHandler : IRequestHandler<BankListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public BankListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(BankListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                var banks = await rewardsDBContext.Banks.Where(x => x.IsActivated == true).ToListAsync();
                List<BankModel> list = new List<BankModel>();
                foreach (var type in banks)
                {
                    BankModel bank = new BankModel();
                    bank.Id = type.Id;
                    bank.Name = type.Name;
                    list.Add(bank);
                }
                response.Successful = true;
                response.Message = "Get Bank List Successfully";
                response.Data = list;
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
