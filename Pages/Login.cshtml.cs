using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MyRazorApp.Pages
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;
        private readonly PasswordHasher<LoginModel> _passwordHasher;
        private readonly EmailService _emailService;

        public LoginModel(ILogger<LoginModel> logger, EmailService emailService)
        {
            _logger = logger;
            _passwordHasher = new PasswordHasher<LoginModel>();
            _emailService = emailService;
        }

        [BindProperty]
        public string? Email { get; set; }

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string? ResetEmail { get; set; }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                return Page();
            }

            string connectionString = "Server=mssql4c40.carrierzone.com;Database=ii0_dkchess_com;User Id=dbm_ii0_dkchess_com;Password=xhXmdDi3mr;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if the email exists
                    string checkEmailQuery = "SELECT Password FROM Users WHERE Email = @Email";
                    using (SqlCommand checkEmailCmd = new SqlCommand(checkEmailQuery, conn))
                    {
                        checkEmailCmd.Parameters.AddWithValue("@Email", (object)Email ?? DBNull.Value);

                        object? result = checkEmailCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            ModelState.AddModelError(string.Empty, "Email not found.");
                            return Page();
                        }

                        string storedHashedPassword = (string)result;

                        // Verify the password
                        var verificationResult = _passwordHasher.VerifyHashedPassword(this, storedHashedPassword, Password);

                        if (verificationResult == PasswordVerificationResult.Success)
                        {
                            return Redirect("/sponsors.html");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid password.");
                            return Page();
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "An error occurred while connecting to the database.");
                ModelState.AddModelError(string.Empty, "An error occurred while connecting to the database.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostRequestResetAsync()
        {
            if (string.IsNullOrEmpty(ResetEmail))
            {
                ModelState.AddModelError(string.Empty, "Email is required.");
                return Page();
            }

            string connectionString = "Server=mssql4c40.carrierzone.com;Database=ii0_dkchess_com;User Id=dbm_ii0_dkchess_com;Password=xhXmdDi3mr;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Generate reset token
                    string resetToken = Guid.NewGuid().ToString();
                    DateTime tokenExpiry = DateTime.UtcNow.AddHours(1);

                    // Update the database with the reset token and expiry
                    string updateQuery = "UPDATE Users SET ResetToken = @ResetToken, TokenExpiry = @TokenExpiry WHERE Email = @Email";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@ResetToken", resetToken);
                        updateCmd.Parameters.AddWithValue("@TokenExpiry", tokenExpiry);
                        updateCmd.Parameters.AddWithValue("@Email", (object)ResetEmail ?? DBNull.Value);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            ModelState.AddModelError(string.Empty, "Email not found.");
                            return Page();
                        }
                    }

                    // Send email with reset link
                    var routeValues = new { token = resetToken };
                    string? resetLink = Url.Page("/ResetPassword", pageHandler: null, values: routeValues, protocol: Request.Scheme);

                    if (resetLink != null)
                    {
                        string subject = "Password Reset Request";
                        string body = $"Please reset your password by clicking <a href='{resetLink}'>here</a>.";
                        
                        await _emailService.SendEmailAsync(ResetEmail, subject, body);

                        return RedirectToPage("/ResetPasswordConfirmation");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Failed to generate reset link.");
                        return Page();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while requesting a password reset.");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your request.");
                return Page();
            }
        }
    }
}
