using System.Security.Cryptography;
using System.Text;
using ITServicesApp.Application.Interfaces.Security;
using ITServicesApp.Domain.Entities;
using ITServicesApp.Domain.Interfaces;
using ITServicesApp.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ITServicesApp.Infrastructure.Security
{
    /// <summary>
    /// Implements password reset with:
    /// - Random base64url token returned to caller (emailed to user)
    /// - Token stored as HASH ONLY (never raw)
    /// - TTL and one-time use enforcement
    /// </summary>
    public sealed class PasswordResetService : IPasswordResetService
    {
        private readonly ApplicationDbContext _db;
        private readonly IPasswordResetRepository _tokens;
        private readonly IConfiguration _cfg;

        // Optional: secret pepper (add to appsettings: "Security": { "PasswordResetPepper": "your-long-secret" })
        private string Pepper => _cfg["Security:PasswordResetPepper"] ?? string.Empty;

        public PasswordResetService(ApplicationDbContext db, IPasswordResetRepository tokens, IConfiguration cfg)
        {
            _db = db;
            _tokens = tokens;
            _cfg = cfg;
        }

        public async Task<string> GenerateAndStoreTokenAsync(User user, int ttlMinutes, CancellationToken ct)
        {
            // 1) Invalidate any previous active tokens for this user (optional, but tidy)
            var now = DateTime.UtcNow;
            var existing = await _db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.UsedAtUtc == null && t.ExpiresAtUtc > now)
                .ToListAsync(ct);

            foreach (var e in existing) e.ExpiresAtUtc = now; // expire them
            if (existing.Count > 0) await _db.SaveChangesAsync(ct);

            // 2) Generate a new random token (raw) to send to the user
            var rawToken = GenerateBase64UrlToken(32); // 256 bits

            // 3) Hash for storage
            var hash = HashToken(rawToken);

            // 4) Store hash + TTL
            var entity = new PasswordResetToken
            {
                UserId = user.Id,
                Token = hash,                 // store the HASH, not the raw token
                CreatedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(ttlMinutes),
                UsedAtUtc = null
            };

            await _tokens.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);

            // 5) Return the RAW token to caller (to email the user)
            return rawToken;
        }

        public async Task<bool> VerifyAndConsumeAsync(User user, string token, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var hash = HashToken(token);

            // Repo is implemented to compare against stored Token (which we store as hash)
            var row = await _tokens.FindValidAsync(user.Id, hash, ct);
            if (row == null) return false;

            row.UsedAtUtc = now;
            // Optional safety: also expire any other active tokens for the user
            var others = await _db.PasswordResetTokens
                .Where(t => t.UserId == user.Id && t.Id != row.Id && t.UsedAtUtc == null && t.ExpiresAtUtc > now)
                .ToListAsync(ct);
            foreach (var o in others) o.ExpiresAtUtc = now;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public string HashToken(string token)
        {
            // SHA256(token + pepper) -> Base64
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token + Pepper);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        // ---- helpers ----
        private static string GenerateBase64UrlToken(int numBytes)
        {
            var bytes = RandomNumberGenerator.GetBytes(numBytes);
            var b64 = Convert.ToBase64String(bytes);
            // Base64Url (RFC 4648): replace chars and trim '='
            return b64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
