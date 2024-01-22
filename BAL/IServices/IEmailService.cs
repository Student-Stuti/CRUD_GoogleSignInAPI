using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BAL.IServices
{
    public interface IEmailService
    {
        Task SendForgotPasswordEmailAsync(string email, string callbackUrl);
    }
}
