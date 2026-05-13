namespace AuthService.Services
{
    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string plainPassword, string hashedPassword)
        {
            if(string.IsNullOrEmpty(plainPassword) || string.IsNullOrEmpty(hashedPassword))
                return false;
            return BCrypt.Net.BCrypt.Verify(plainPassword,hashedPassword);
        }
    }
}