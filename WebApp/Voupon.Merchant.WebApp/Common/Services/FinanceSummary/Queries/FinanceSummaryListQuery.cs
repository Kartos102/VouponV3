using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Merchant.WebApp.Common.Services.Products.Models;
using Voupon.Merchant.WebApp.ViewModels;


namespace Voupon.Merchant.WebApp.Common.Services.FinanceSummary.Queries
{
    public class FinanceSummaryListQuery : IRequest<ApiResponseViewModel>
    {
    }
    public class FinanceSummaryListQueryHandler : IRequestHandler<FinanceSummaryListQuery, ApiResponseViewModel>
    {
        RewardsDBContext rewardsDBContext;
        public FinanceSummaryListQueryHandler(RewardsDBContext rewardsDBContext)
        {
            this.rewardsDBContext = rewardsDBContext;
        }

        public async Task<ApiResponseViewModel> Handle(FinanceSummaryListQuery request, CancellationToken cancellationToken)
        {
            ApiResponseViewModel response = new ApiResponseViewModel();
            try
            {
                DateTime current = DateTime.Now;
                DateTime currentDate = new DateTime(current.Year, current.Month, current.Day);
                var previousStartDate = new DateTime(2020, 1, 1);
                var currentDay = currentDate.Day;
                var StartDate = DateTime.Now;
                var EndDate = DateTime.Now;
                var finance = await rewardsDBContext.FinanceSummary.OrderByDescending(x => x.EndDate).FirstOrDefaultAsync();
                if (finance != null)
                {
                    previousStartDate = finance.StartDate.ToDateTime(TimeOnly.Parse("10:00 PM"));
                    if (finance.EndDate.Day == 15)
                    {
                        //Create next summary 
                        StartDate = new DateTime(finance.StartDate.Year, finance.StartDate.Month, 16);
                        EndDate = new DateTime(finance.EndDate.Year, finance.EndDate.Month, DateTime.DaysInMonth(finance.EndDate.Year, finance.EndDate.Month));

                    }
                    else
                    {
                        //Create next summary 
                        StartDate = new DateTime(finance.StartDate.AddMonths(1).Year, finance.StartDate.AddMonths(1).Month, 1);
                        EndDate = new DateTime(finance.EndDate.AddMonths(1).Year, finance.EndDate.AddMonths(1).Month, 15);
                    }
                }
                else
                {
                    if (currentDay > 15)
                    {
                        //Create same month summary 1 to 15
                        StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                        EndDate = new DateTime(currentDate.Year, currentDate.Month, 15);

                    }
                    else
                    {
                        //Create last month summary 16 to end of the month
                        var lastMonthDate = currentDate.AddMonths(-1);
                        StartDate = new DateTime(lastMonthDate.Year, lastMonthDate.Month, 16);
                        EndDate = new DateTime(lastMonthDate.Year, lastMonthDate.Month, DateTime.DaysInMonth(lastMonthDate.Year, lastMonthDate.Month));
                    }
                }
                if (DateTime.Compare(currentDate, EndDate) > 0)
                {
                    Voupon.Database.Postgres.RewardsEntities.FinanceSummary financeSummary = new Voupon.Database.Postgres.RewardsEntities.FinanceSummary();
                    financeSummary.StartDate = DateOnly.FromDateTime(StartDate);
                    financeSummary.EndDate = DateOnly.FromDateTime(EndDate);
                    financeSummary.TotalMerchant = 0;
                    financeSummary.TotalPayout = 0;
                    financeSummary.PayoutDate = null;
                    financeSummary.MerchantFinance = new List<MerchantFinance>();

                    //Create new Finance Summary
                    var InStore = await rewardsDBContext.InStoreRedemptionTokens.Where(x => x.MerchantId != 1 && x.IsRedeemed && x.RedeemedAt.HasValue && DateTime.Compare(x.RedeemedAt.Value, StartDate) < 0 && DateTime.Compare(x.RedeemedAt.Value, previousStartDate) >= 0).Include(x => x.Merchant).Include(x => x.Product).ToListAsync();
                    var Digital = await rewardsDBContext.DigitalRedemptionTokens.Where(x => x.MerchantId != 1 && x.IsRedeemed && x.RedeemedAt.HasValue && DateTime.Compare(x.RedeemedAt.Value, StartDate) < 0 && DateTime.Compare(x.RedeemedAt.Value, previousStartDate) >= 0).Include(x => x.Merchant).Include(x => x.Product).ToListAsync();
                    var Delivery = await rewardsDBContext.DeliveryRedemptionTokens.Where(x => x.MerchantId != 1 && x.IsRedeemed && x.RedeemedAt.HasValue && DateTime.Compare(x.RedeemedAt.Value, StartDate) < 0 && DateTime.Compare(x.RedeemedAt.Value, previousStartDate) >= 0).Include(x => x.Merchant).Include(x => x.Product).ToListAsync();
                    foreach (var item in InStore)
                    {
                        var merchantFinance = financeSummary.MerchantFinance.FirstOrDefault(x => x.MerchantId == item.MerchantId);
                        if (merchantFinance == null)
                        {
                            MerchantFinance newFinance = new MerchantFinance()
                            {
                                IsPaid = false,
                                MerchantId = item.MerchantId,
                                MerchantDisplayName = item.Merchant.DisplayName,
                                TotalTransaction = 0,
                                TotalPayout = 0,
                                PayoutDate = null,
                                Remarks = "",
                                Bank = "",
                                BankAccount = ""
                            };
                            var bannkAccount = await rewardsDBContext.BankAccounts.Include(x => x.Bank).FirstOrDefaultAsync(x => x.MerchantId == item.MerchantId);
                            if (bannkAccount != null)
                            {
                                newFinance.Bank = bannkAccount.Bank.Name;
                                newFinance.BankAccount = bannkAccount.AccountNumber;
                            }
                            financeSummary.MerchantFinance.Add(newFinance);
                            merchantFinance = financeSummary.MerchantFinance.FirstOrDefault(x => x.MerchantId == item.MerchantId);
                        }
                        FinanceTransaction newTransaction = new FinanceTransaction();
                        newTransaction.ProductId = item.ProductId;
                        newTransaction.OrderItemId = item.OrderItemId;
                        newTransaction.ProductTitle = item.ProductTitle;
                        newTransaction.TotalPrice = item.Revenue.Value;
                        newTransaction.DefaultCommission = item.Product.DefaultCommission.Value;
                        newTransaction.MerchantProfit = Math.Round(newTransaction.TotalPrice * ((100 - newTransaction.DefaultCommission) / 100),2);
                        newTransaction.AdminProfit = newTransaction.TotalPrice - newTransaction.MerchantProfit;// newTransaction.TotalPrice * newTransaction.DefaultCommission / 100;
                        merchantFinance.FinanceTransaction.Add(newTransaction);
                    }

                    foreach (var item in Digital)
                    {
                        var merchantFinance = financeSummary.MerchantFinance.FirstOrDefault(x => x.MerchantId == item.MerchantId);
                        if (merchantFinance == null)
                        {
                            MerchantFinance newFinance = new MerchantFinance()
                            {
                                IsPaid = false,
                                MerchantId = item.MerchantId,
                                MerchantDisplayName = item.Merchant.DisplayName,
                                TotalTransaction = 0,
                                TotalPayout = 0,
                                PayoutDate = null,
                                Remarks = "",
                                Bank = "",
                                BankAccount = ""
                            };
                            var bannkAccount = await rewardsDBContext.BankAccounts.Include(x => x.Bank).FirstOrDefaultAsync(x => x.MerchantId == item.MerchantId);
                            if (bannkAccount != null)
                            {
                                newFinance.Bank = bannkAccount.Bank.Name;
                                newFinance.BankAccount = bannkAccount.AccountNumber;
                            }
                            financeSummary.MerchantFinance.Add(newFinance);
                            merchantFinance = financeSummary.MerchantFinance.FirstOrDefault(x => x.MerchantId == item.MerchantId);
                        }
                        FinanceTransaction newTransaction = new FinanceTransaction();
                        newTransaction.ProductId = item.ProductId;
                        newTransaction.OrderItemId = item.OrderItemId;
                        newTransaction.ProductTitle = item.ProductTitle;
                        newTransaction.TotalPrice = item.Revenue.Value;
                        newTransaction.DefaultCommission = item.Product.DefaultCommission.Value;
                        newTransaction.MerchantProfit = newTransaction.TotalPrice * (100 - newTransaction.DefaultCommission / 100);
                        newTransaction.AdminProfit = newTransaction.TotalPrice * newTransaction.DefaultCommission / 100;
                        merchantFinance.FinanceTransaction.Add(newTransaction);
                    }

                    foreach (var item in Delivery)
                    {
                        var merchantFinance = financeSummary.MerchantFinance.FirstOrDefault(x => x.MerchantId == item.MerchantId);
                        if (merchantFinance == null)
                        {
                            MerchantFinance newFinance = new MerchantFinance()
                            {
                                IsPaid = false,
                                MerchantId = item.MerchantId,
                                MerchantDisplayName = item.Merchant.DisplayName,
                                TotalTransaction = 0,
                                TotalPayout = 0,
                                PayoutDate = null,
                                Remarks = "",
                                Bank = "",
                                BankAccount = ""
                            };
                            var bannkAccount = await rewardsDBContext.BankAccounts.Include(x => x.Bank).FirstOrDefaultAsync(x => x.MerchantId == item.MerchantId);
                            if (bannkAccount != null)
                            {
                                newFinance.Bank = bannkAccount.Bank.Name;
                                newFinance.BankAccount = bannkAccount.AccountNumber;
                            }
                            financeSummary.MerchantFinance.Add(newFinance);
                            merchantFinance = financeSummary.MerchantFinance.FirstOrDefault(x => x.MerchantId == item.MerchantId);
                        }
                        FinanceTransaction newTransaction = new FinanceTransaction();
                        newTransaction.ProductId = item.ProductId;
                        newTransaction.OrderItemId = item.OrderItemId;
                        newTransaction.ProductTitle = item.ProductTitle;
                        newTransaction.TotalPrice = item.Revenue.Value;
                        newTransaction.DefaultCommission = item.Product.DefaultCommission.Value;
                        newTransaction.MerchantProfit = newTransaction.TotalPrice * (100 - newTransaction.DefaultCommission / 100);
                        newTransaction.AdminProfit = newTransaction.TotalPrice * newTransaction.DefaultCommission / 100;
                        merchantFinance.FinanceTransaction.Add(newTransaction);
                    }

                    financeSummary.TotalMerchant = financeSummary.MerchantFinance.Count();
                    foreach (var item in financeSummary.MerchantFinance)
                    {

                        item.TotalTransaction = item.FinanceTransaction.Count();
                        item.TotalPayout = item.FinanceTransaction.Sum(x => x.MerchantProfit);
                        financeSummary.TotalPayout = financeSummary.TotalPayout + item.TotalPayout;
                    }
                    await rewardsDBContext.FinanceSummary.AddAsync(financeSummary);
                    await rewardsDBContext.SaveChangesAsync();
                }
                List<Voupon.Database.Postgres.RewardsEntities.FinanceSummary> summarryList = await rewardsDBContext.FinanceSummary.ToListAsync();
                response.Successful = true;
                response.Message = "Get Finance Summary List Successfully";
                response.Data = summarryList;
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
