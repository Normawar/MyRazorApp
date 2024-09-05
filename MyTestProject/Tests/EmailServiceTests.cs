using Microsoft.Extensions.Options;
using Moq;
using MyRazorApp.Services;
using System.Threading.Tasks;
using Xunit;

namespace MyTestProject
{
    public class EmailServiceTests
    {
        private readonly EmailService _emailService;
        private readonly Mock<IOptions<SmtpSettings>> _mockOptions;

        public EmailServiceTests()
        {
            // Mock the SmtpSettings
            _mockOptions = new Mock<IOptions<SmtpSettings>>();
            _mockOptions.Setup(o => o.Value).Returns(new SmtpSettings
            {
                Host = "mail.dkchess.com",
                Port = 1025,
                Username = "register@dkchess.com",
                Password = "Zeferino07",
                UseSSL = false
            });

            // Initialize the EmailService with mocked SmtpSettings
            _emailService = new EmailService(_mockOptions.Object);
        }

        [Fact]
        public async Task SendEmailAsync_ShouldSendEmail()
        {
            // Arrange
            var to = "recipient@example.com";
            var subject = "Test Email";
            var body = "This is a test email.";

            // Act
            await _emailService.SendEmailAsync(to, subject, body);

            // Assert
            // Add assertions to verify email sending behavior
        }
    }
}
