using System.Security.Cryptography;

namespace EmployeeManagementSystem.Helpers
{
    public static class PasswordGenerator
    {
        private const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "0123456789";
        private const string Special = "@#$%&*!";

        private static readonly string AllCharacters =
            Uppercase + Lowercase + Numbers + Special;

        public static string Generate(int length = 10)
        {
            var password = new char[length];

            using var rng = RandomNumberGenerator.Create();

            byte[] randomBytes = new byte[length];

            rng.GetBytes(randomBytes);

            for (int i = 0; i < length; i++)
            {
                password[i] = AllCharacters[randomBytes[i] % AllCharacters.Length];
            }

            return new string(password);
        }
    }
}