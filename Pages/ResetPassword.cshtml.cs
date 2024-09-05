using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace MyRazorApp.Pages
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ILogger<ResetPasswordModel> _logger;
        private readonly PasswordHasher<ResetPasswordModel> _passwordHasher;

        public ResetPasswordModel(ILogger<ResetPasswordModel> logger)
        {
            _logger = logger;
            _passwordHasher = new PasswordHasher<ResetPasswordModel>();
        }

        [BindProperty]
        public string? Token { get; set; }

        [BindProperty]
        public string? NewPassword { get; set; }

        [BindProperty]
        public string? ConfirmPassword { get; set; }

        public IActionResult OnGet(string token)
        {
            Token = token;
            if (string.IsNullOrEmpty(Token))
            {
                ModelState.AddModelError(string.Empty, "Token is required.");
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check for null values and passwords match
            if (string.IsNullOrEmpty(NewPassword))
            {
                ModelState.AddModelError(string.Empty, "New password is required.");
                return Page();
            }

            if (string.IsNullOrEmpty(ConfirmPassword))
            {
                ModelState.AddModelError(string.Empty, "Confirm password is required.");
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return Page();
            }

            string connectionString = "Server=mssql4c40.carrierzone.com;Database=ii0_dkchess_com;User Id=dbm_ii0_dkchess_com;Password=xhXmdDi3mr;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    // Verify token and update password
                    string updateQuery = "UPDATE Users SET Password = @Password, ResetToken = NULL, TokenExpiry = NULL WHERE ResetToken = @Token AND TokenExpiry > @Now";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        string hashedPassword = _passwordHasher.HashPassword(this, NewPassword ?? string.Empty);

                        updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                        updateCmd.Parameters.AddWithValue("@Token", Token ?? string.Empty); // Ensure Token is not null
                        updateCmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);

                        int rowsAffected = await updateCmd.ExecuteNonQueryAsync();

                        if (rowsAffected == 0)
                        {
                            ModelState.AddModelError(string.Empty, "Invalid or expired token.");
                            return Page();
                        }
                    }

                    return RedirectToPage("/Login");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while resetting the password.");
                ModelState.AddModelError(string.Empty, "An error occurred while resetting the password.");
                return Page();
            }
        }
    }
}
