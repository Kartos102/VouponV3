using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.Resources.Page
{
    public class BlogListPage : IRequest<BlogListViewModel>
    {
        public long Id { get; set; }
        private class BlogListHandler : IRequestHandler<BlogListPage, BlogListViewModel>
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private IOptions<AppSettings> _appSettings;
            public BlogListHandler(IOptions<AppSettings> appSettings)
            {
                //_httpClientFactory = httpClientFactory;
                _appSettings = appSettings;
            }

            public async Task<BlogListViewModel> Handle(BlogListPage request, CancellationToken cancellationToken)
            {
                var viewModel = new BlogListViewModel();
                var httpClient = new HttpClient();

                var limit = 40;
                var offset = 0;

                var result = await httpClient.GetAsync($"{_appSettings.Value.App.VouponAPIUrl}/v1/blogs/?category=merchant&limit={limit}&offset={offset}");
                if(result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = JsonConvert.DeserializeObject<BlogListResponseModel>(await result.Content.ReadAsStringAsync());

                    viewModel.Tags = response.Data.Tags;
                    viewModel.TotalPosts = response.Data.TotalPosts;
                    viewModel.Blogs = response.Data.Blogs;


                }
                return viewModel;
            }
        }
    }

    public class BlogListViewModel
    {
        public long TotalPosts { get; set; }
        public List<Blog> Blogs { get; set; }
        public List<string> Tags { get; set; }
    }

    public class Blog
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public DateTime Date { get; set; }
        public string Excerpt { get; set; }
        public string FeaturedImage { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Categories { get; set; }
    }

    public class BlogListResponseModel : ApiResponseViewModel
    {
        public new BlogListViewModel Data { get; set; }
    }
}

