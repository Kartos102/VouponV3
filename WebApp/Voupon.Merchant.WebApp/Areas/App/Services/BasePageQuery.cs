using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.Encryption;

namespace Voupon.Merchant.WebApp.Areas.App.Services
{
    public class BasePageQuery : IRequest<BasePageQueryViewModel>
    {
        public string UserName { get; set; }

        public class BankAccountQueryHandler : IRequestHandler<BasePageQuery, BasePageQueryViewModel>
        {
            IOptions<AppSettings> appSettings;
            public BankAccountQueryHandler(IOptions<AppSettings> appSettings)
            {
                this.appSettings = appSettings;
            }

            public async Task<BasePageQueryViewModel> Handle(BasePageQuery request, CancellationToken cancellationToken)
            {

                var viewModel = new BasePageQueryViewModel
                {
                    ServerlessUrl = appSettings.Value.App.ServerlessUrl,
                    ChatAPIUrl = appSettings.Value.App.ChatAPIUrl
                };

                viewModel.ChatToken = new ChatToken
                {
                    Email = request.UserName,
                    CreatedAt = DateTime.Now

                }.ToChatTokenValue();

                return viewModel;
            }
        }
    }

    public class BasePageQueryViewModel
    {
        public string ServerlessUrl { get; set; }
        public string ChatAPIUrl { get; set; }
        public string ChatToken { get; set; }
    }
}
