using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity; // For PasswordHasher
using System.Data.SqlClient;

namespace MyRazorApp.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly ILogger<RegisterModel> _logger;
        private readonly PasswordHasher<RegisterModel> _passwordHasher;

        public RegisterModel(ILogger<RegisterModel> logger)
        {
            _logger = logger;
            _passwordHasher = new PasswordHasher<RegisterModel>();
        }

        [BindProperty]
        public string Username { get; set; } = string.Empty;

        [BindProperty]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        public void OnPost()
        {
            // Validate input
            if (string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError(nameof(Password), "Password is required.");
                return;
            }

            // Define the connection string
            string connectionString = "Server=mssql4c40.carrierzone.com;Database=ii0_dkchess_com;User Id=dbm_ii0_dkchess_com;Password=xhXmdDi3mr;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check if the email already exists
                    string checkEmailQuery = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

                    using (SqlCommand checkEmailCmd = new SqlCommand(checkEmailQuery, conn))
                    {
                        checkEmailCmd.Parameters.AddWithValue("@Email", Email ?? (object)DBNull.Value);
                        int emailCount = (int)checkEmailCmd.ExecuteScalar();

                        if (emailCount > 0)
                        {
                            // Handle case where email already exists
                            ModelState.AddModelError(string.Empty, "Email already registered.");
                            return;
                        }

                        // Hash the password
                        string hashedPassword = _passwordHasher.HashPassword(this, Password);

                        // Define the query for inserting a new user
                        string insertQuery = "INSERT INTO Users (Username, Password, Email) VALUES (@Username, @Password, @Email)";

                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                        {
                            // Add parameters to the command
                            insertCmd.Parameters.AddWithValue("@Username", Username);
                            insertCmd.Parameters.AddWithValue("@Password", hashedPassword);
                            insertCmd.Parameters.AddWithValue("@Email", Email ?? (object)DBNull.Value);

                            // Execute the insert command
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }

                // Redirect to sponsors.html after successful registration
                Response.Redirect("/sponsors.html");
            }
            catch (SqlException ex)
            {
                // Log the exception and add an error message
                _logger.LogError(ex, "An error occurred while connecting to the database.");
                ModelState.AddModelError(string.Empty, "An error occurred while connecting to the database.");
            }
        }
    }
}
