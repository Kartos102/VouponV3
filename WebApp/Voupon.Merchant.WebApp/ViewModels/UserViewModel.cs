using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Voupon.Merchant.WebApp.ViewModels
{
    public class UserViewModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Id { get; set; }
        public string Token { get; set; }
        public DateTime LoggedInAt { get; set; }
        public string Role { get; set; }
    }
}
