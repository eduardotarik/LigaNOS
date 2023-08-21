using Azure;
using System.Threading.Tasks;

namespace LigaNOS.Helpers
{
    public interface IMailHelper
    {
        Task<Response> SendEmail(string to, string subject, string body);
    }
}
