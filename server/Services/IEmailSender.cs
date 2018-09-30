using System.Threading.Tasks;

namespace DocumentAnnotation.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}