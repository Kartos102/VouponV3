using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Voupon.Common.SMS.SMSS360
{
    public interface ISMSS360
    {
        Task<string> SendMessage(string recipient, string message, string customerReferenceId);
    }
}
