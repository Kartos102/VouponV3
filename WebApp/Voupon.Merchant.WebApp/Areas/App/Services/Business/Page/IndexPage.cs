using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.Areas.App.Services.Business.Page
{
    public class IndexPage : IRequest<IndexPageViewModel>
    {
    }

    public class IndexPageViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
     
    }
}
