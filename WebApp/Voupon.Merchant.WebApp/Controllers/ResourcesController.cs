using System.Threading.Tasks;
using Voupon.Merchant.WebApp.Services.Resources.Page;
using Microsoft.AspNetCore.Mvc;
using Voupon.Merchant.WebApp.Services.Resources.Page.BlogPost;

namespace Voupon.Merchant.WebApp.Controllers
{
    public class ResourcesController : BaseController
    {
        [HttpGet]
        [Route("resources")]
        public  async Task<IActionResult> Index()
        {
            var viewModel = await Mediator.Send(new BlogListPage());
            return View(viewModel);
        }

        [HttpGet]
        [Route("resources/{id}/{slug}")]
        public async Task<IActionResult> Article(long? id)
        {
            if (!id.HasValue)
            {
                return View();
            }

            var viewModel = await Mediator.Send(new BlogPostPage
            {
                Id = id.Value
            });
            return View(viewModel);
        }
    }
}