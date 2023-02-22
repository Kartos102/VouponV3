using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Voupon.Rewards.WebApp.ViewModels;


namespace Voupon.Rewards.WebApp.Mapper.Profiles
{
    public class OrderItemExternalProfile : Profile
    {
        public OrderItemExternalProfile()
        {
            CreateMap<SearchProductViewModel, SearchProductViewModelFilterTitles>();
            CreateMap<SearchProductViewModelFilterTitles, SearchProductViewModel>();
        }
    }
}
 