using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.ErrorLog.Queries.List
{
    public class ListErrorQuery : ListQueryRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public class ListErrorQueryHandler : IRequestHandler<ListErrorQuery, ApiResponseViewModel>
        {
            RewardsDBContext rewardsDBContext;
            public ListErrorQueryHandler(RewardsDBContext rewardsDBContext)
            {
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ListErrorQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var errors = await rewardsDBContext.ErrorLogs.Where(x=> x.CreatedAt.Date >= request.From && x.CreatedAt.Date <= request.To).OrderByDescending(x => x.CreatedAt).Take(request.PageSize).ToListAsync();
                    apiResponseViewModel.Successful = true;
                        apiResponseViewModel.Data = new ListView<ErrorLogViewModel>(errors.Count(), request.PageSize, request.PageIndex)
                    {
                        Items = errors.Select(x => new ErrorLogViewModel
                        {
                            Id = x.Id,
                            Type = (x.TypeId == 1 ? "Controller" : "Service"),
                            ActionName = x.ActionName,
                            ActionRequest = x.ActionRequest,
                            CreatedAt = x.CreatedAt,
                            Email = x.Email,
                            MasterMemberProfileId = x.MasterProfileId,
                            MemberProfileId = x.MemberProfileId,
                            Errors = x.Errors
                        }).ToList()
                    };
                }
                catch (Exception ex)
                {
                    apiResponseViewModel.Successful = false;
                    apiResponseViewModel.Message = ex.Message;
                }
                return apiResponseViewModel;
            }
        }
    }

    public class ErrorLogViewModel
    {
        public int Id { get; set; }

        public string Type { get; set; }
        public string ActionName { get; set; }
        public string ActionRequest { get; set; }
        public string Errors { get; set; }
        public string Email { get; set; }
        public int? MemberProfileId { get; set; }
        public int? MasterMemberProfileId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
