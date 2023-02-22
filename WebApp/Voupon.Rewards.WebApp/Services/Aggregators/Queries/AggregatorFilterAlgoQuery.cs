using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Database.Postgres.RewardsEntities;
using Voupon.Database.Postgres.VodusEntities;
using Voupon.Rewards.WebApp.ViewModels;

namespace Voupon.Rewards.WebApp.Services.Aggregators.Queries
{

    public class AggregatorFilterAlgoQuery : IRequest<ApiResponseViewModel>
    {
        public string SearchQuery { get; set; }
        public List<int> PriceFilter { get; set; }
        public List<SearchProductViewModel> SearchProductModel { get; set; }

    }

    public class AggregatorFilterAlgoQueryHandler : IRequestHandler<AggregatorFilterAlgoQuery, ApiResponseViewModel>
    {
        private readonly VodusV2Context vodusV2Context;
        private readonly IMapper _mapper;


        public AggregatorFilterAlgoQueryHandler(VodusV2Context vodusV2Context, IMapper mapper)
        {
            this.vodusV2Context = vodusV2Context;
            this._mapper = mapper;
        }

        public async Task<ApiResponseViewModel> Handle(AggregatorFilterAlgoQuery request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();
            try
            {
                List<SearchProductViewModelFilterTitles> searchProductModelFilter = request.SearchProductModel.Select(x => _mapper.Map(x, new SearchProductViewModelFilterTitles() { FilterTitle = RemoveSpecialCharacters(x.Title).ToLower().Split(' ').ToList()})).ToList();

                foreach (var product in searchProductModelFilter)
                {
                    foreach(var toCompareProduct in searchProductModelFilter)
                    {
                        if (product.ExternalTypeId == toCompareProduct.ExternalTypeId)
                        {
                            continue;
                        }
                        else
                        {
                            byte matchingCount = 0;
                            foreach(var titlePart in product.FilterTitle)
                            {
                                foreach (var toComparetitlePart in product.FilterTitle)
                                {
                                    if(titlePart == toComparetitlePart)
                                    {
                                        matchingCount++;
                                        break;
                                    }
                                }
                            }
                            if(matchingCount > 6)
                            {

                            }
                        }
                    }
                }
                //    if (crawlerResult.Successful)
                //{
                apiResponseViewModel.Data = request.SearchProductModel;
                apiResponseViewModel.Successful = true;
                //}
            }
            catch (Exception ex)
            {
                var error = ex.ToString();
            }
            return apiResponseViewModel;
        }

        private string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') /*|| c == '.' || c == '_'*/)
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(' ');
                }
            }
            return sb.ToString();
        }
    }
}
