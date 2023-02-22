using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.Resources.Page.BlogPost
{
    public class BlogPostPage : IRequest<BlogPostViewModel>
    {
        public long Id { get; set; }
        private class BlogPostPageHandler : IRequestHandler<BlogPostPage, BlogPostViewModel>
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private IOptions<AppSettings> _appSettings;
            public BlogPostPageHandler(IOptions<AppSettings> appSettings)
            {
                //_httpClientFactory = httpClientFactory;
                _appSettings = appSettings;
            }

            public async Task<BlogPostViewModel> Handle(BlogPostPage request, CancellationToken cancellationToken)
            {
                var viewModel = new BlogPostViewModel();
                var httpClient = new HttpClient();

                var result = await httpClient.GetAsync($"{_appSettings.Value.App.VouponAPIUrl}/v1/blogs/post/?id={request.Id}");
                if(result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var response = JsonConvert.DeserializeObject<BlogPostResponseModel>(await result.Content.ReadAsStringAsync());

                    viewModel.Blog = response.Data.Blog;
                    viewModel.Related = response.Data.Related;


                }
                return viewModel;
            }
        }
    }

    public class BlogPostViewModel
    {
        public Blog Blog { get; set; }
        public List<Related> Related { get; set; }
    }

    public class Blog
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public string FeaturedImage { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Categories { get; set; }
    }

    public class Related
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string FeaturedImage { get; set; }
        public string Slug { get; set; }
        public DateTime Date { get; set; }
    }

    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }


    public class BlogPostResponseModel : ApiResponseViewModel
    {
        public new BlogPostViewModel Data { get; set; }
    }
}

