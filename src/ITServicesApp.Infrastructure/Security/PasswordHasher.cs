using System;
using System.Security.Cryptography;
using System.Text;
using ITServicesApp.Application.Interfaces.Security;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace ITServicesApp.Infrastructure.Security
{
    /// <summary>
    /// Versioned PBKDF2 hasher. Format: v1.{iterations}.{saltBase64}.{subkeyBase64}
    /// </summary>
    public sealed class PasswordHasher : IPasswordHasher
    {
        private const string Prefix = "v1";
        private const int SaltSize = 16;     // 128-bit salt
        private const int KeySize = 32;     // 256-bit subkey
        private const int Iterations = 100_000; // tune as needed

        public string Hash(string password)
        {
            if (password is null) throw new ArgumentNullException(nameof(password));

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] subkey = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            var saltB64 = Convert.ToBase64String(salt);
            var keyB64 = Convert.ToBase64String(subkey);
            return $"{Prefix}.{Iterations}.{saltB64}.{keyB64}";
        }

        public bool Verify(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            if (!TryParse(hash, out var iterations, out var salt, out var expectedSubkey))
                return false; // unknown/legacy format → treat as not verified

            byte[] actual = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: iterations,
                numBytesRequested: expectedSubkey.Length);

            return CryptographicOperations.FixedTimeEquals(actual, expectedSubkey);
        }

        public bool NeedsRehash(string hash)
        {
            // Rehash if format not recognized or iteration count lower than current.
            if (!TryParse(hash, out var iterations, out _, out _)) return true;
            return iterations < Iterations;
        }

        private static bool TryParse(string hash, out int iterations, out byte[] salt, out byte[] subkey)
        {
            iterations = 0; salt = Array.Empty<byte>(); subkey = Array.Empty<byte>();

            var parts = hash.Split('.', 4, StringSplitOptions.None);
            if (parts.Length != 4) return false;
            if (!string.Equals(parts[0], Prefix, StringComparison.Ordinal)) return false;
            if (!int.TryParse(parts[1], out iterations) || iterations <= 0) return false;

            if (!TryBase64(parts[2], out salt)) return false;
            if (!TryBase64(parts[3], out subkey)) return false;

            // basic sanity
            if (salt.Length < 8 || subkey.Length < 16) return false;

            return true;
        }

        private static bool TryBase64(string s, out byte[] bytes)
        {
            try { bytes = Convert.FromBase64String(s); return true; }
            catch { bytes = Array.Empty<byte>(); return false; }
        }
    }
}
