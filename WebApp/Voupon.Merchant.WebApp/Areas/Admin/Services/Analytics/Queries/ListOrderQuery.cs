using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.BaseTypes;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Areas.Admin.Services.Analytics.Queries.List
{
    public class ListOrderQuery : ListQueryRequest<ApiResponseViewModel>
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public class ListOrderQueryHandler : IRequestHandler<ListOrderQuery, ApiResponseViewModel>
        {
            VodusV2Context vodusV2Context;
            RewardsDBContext rewardsDBContext;
            public ListOrderQueryHandler(VodusV2Context vodusV2Context, RewardsDBContext rewardsDBContext)
            {
                this.vodusV2Context = vodusV2Context;
                this.rewardsDBContext = rewardsDBContext;
            }

            public async Task<ApiResponseViewModel> Handle(ListOrderQuery request, CancellationToken cancellationToken)
            {
                var apiResponseViewModel = new ApiResponseViewModel();
                try
                {
                    var orderPayments = await rewardsDBContext.OrderPayments.Where(x => x.CreatedAt >= request.From && x.CreatedAt < request.To.AddDays(1)).Select(x => x.RefNo).ToListAsync();

                    var orders = rewardsDBContext.Orders.Include(x => x.OrderPayments).Include(x => x.OrderItems).Include(x => x.OrderShopExternal).ThenInclude(x => x.OrderItemExternal).Where(x => orderPayments.Contains(x.Id));


                    var dates = Enumerable.Range(0, 1 + request.To.Subtract(request.From).Days).Select(offset => request.From.AddDays(offset)).ToArray();

                    var pointsUsedGroupByDate = orders.GroupBy(x => x.CreatedAt.Date);

                    var list = new List<OrdersViewModel>();

                    foreach (var item in dates)
                    {
                        var orderPaymentItem = orders.SelectMany(x => x.OrderPayments).Where(x => x.CreatedAt.Date == item.Date).Select(x => x.RefNo);
                        var ordersForDate = orders.Where(x => orderPaymentItem.Contains(x.Id));

                        var vodusItems = ordersForDate.Where(x => x.OrderShopExternal == null).SelectMany(x => x.OrderItems).ToList().GroupBy(x => x.ProductTitle).Count();
                        var aggregatorItems = ordersForDate.Where(x => x.OrderShopExternal != null).SelectMany(x => x.OrderShopExternal).SelectMany(x => x.OrderItemExternal).ToList().GroupBy(x => x.ProductTitle).Count();

                        list.Add(new OrdersViewModel
                        {
                            Date = item,
                            DateString = item.ToString("dddd, dd MMMM"),
                            TotalOrders = vodusItems + aggregatorItems
                        });
                    }

                    apiResponseViewModel.Successful = true;
                    apiResponseViewModel.Data = new ListView<OrdersViewModel>(list.Count(), request.PageSize, request.PageIndex)
                    {
                        Items = list
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


        public class OrdersViewModel
        {
            public DateTime Date { get; set; }
            public string DateString { get; set; }
            public int TotalOrders { get; set; }
        }
    }
}
