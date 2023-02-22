using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Voupon.Common.SMS.SMSS360;
using Voupon.Merchant.WebApp.ViewModels;

namespace Voupon.Merchant.WebApp.Services.Sample.Create.Commands
{
    public class SendSMSCommand : IRequest<ApiResponseViewModel>
    {
        public string Recipient { get; set; }
    }

    public class SendSMSCommandHandler : IRequestHandler<SendSMSCommand, ApiResponseViewModel>
    {
        private readonly ISMSS360 smss360;

        public SendSMSCommandHandler(ISMSS360 smss360)
        {
            this.smss360 = smss360;
        }

        public async Task<ApiResponseViewModel> Handle(SendSMSCommand request, CancellationToken cancellationToken)
        {
            var apiResponseViewModel = new ApiResponseViewModel();

            var message = string.Format(SMSS360.Templates.Template1, "123456");
            var result = await smss360.SendMessage(request.Recipient, message, Guid.NewGuid().ToString());

            if (result == "0" || result == "1606")
            {
                apiResponseViewModel.Successful = true;
                apiResponseViewModel.Message = "Successfully sent SMS";
            }
            else
            {
                apiResponseViewModel.Successful = false;
                apiResponseViewModel.Message = "Failed to sent SMS";
            }

            return apiResponseViewModel;
        }
    }
}
