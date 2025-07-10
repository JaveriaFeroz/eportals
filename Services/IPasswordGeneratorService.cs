using System;
using System.Security.Cryptography;
using System.Text;

namespace ProcureToPay.Services
{
    public interface IPasswordGeneratorService
    {
        string GenerateTemporaryPassword(int length = 12);
    }

    public class PasswordGeneratorService : IPasswordGeneratorService
    {
        private readonly ILogger<PasswordGeneratorService> _logger;

        public PasswordGeneratorService(ILogger<PasswordGeneratorService> logger)
        {
            _logger = logger;
        }

        public string GenerateTemporaryPassword(int length = 12)
        {
            // Ensure minimum length for security
            if (length < 8)
            {
                length = 8;
                _logger.LogWarning("Password length was too short, defaulting to 8 characters");
            }

            // Define character sets
            const string uppercaseChars = "ABCDEFGHJKLMNOPQRSTUVWXYZ"; // Removed I to avoid confusion with 1
            const string lowercaseChars = "abcdefghijkmnopqrstuvwxyz"; // Removed l to avoid confusion with 1
            const string digitChars = "23456789"; // Removed 0 and 1 to avoid confusion with O and l
            const string specialChars = "!@#$%^&*()_-+=<>?";

            // Combine all character sets
            var allChars = new StringBuilder();
            allChars.Append(uppercaseChars);
            allChars.Append(lowercaseChars);
            allChars.Append(digitChars);
            allChars.Append(specialChars);

            // Create a byte array to hold random values
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Build the password
            var password = new StringBuilder(length);

            // Ensure at least one of each character type for complexity
            password.Append(uppercaseChars[randomBytes[0] % uppercaseChars.Length]);
            password.Append(lowercaseChars[randomBytes[1] % lowercaseChars.Length]);
            password.Append(digitChars[randomBytes[2] % digitChars.Length]);
            password.Append(specialChars[randomBytes[3] % specialChars.Length]);

            // Fill the rest of the password with random characters
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[randomBytes[i] % allChars.Length]);
            }

            // Shuffle the password characters to avoid predictable patterns
            var shuffledPassword = ShuffleString(password.ToString(), randomBytes);

            _logger.LogInformation("Generated temporary password");
            return shuffledPassword;
        }

        private string ShuffleString(string input, byte[] randomBytes)
        {
            var characters = input.ToCharArray();

            // Fisher-Yates shuffle
            for (int i = characters.Length - 1; i > 0; i--)
            {
                int j = randomBytes[i % randomBytes.Length] % (i + 1);
                var temp = characters[i];
                characters[i] = characters[j];
                characters[j] = temp;
            }

            return new string(characters);
        }
    }
}