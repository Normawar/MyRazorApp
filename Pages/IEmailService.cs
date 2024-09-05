using System.Threading.Tasks;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetLink);
}
