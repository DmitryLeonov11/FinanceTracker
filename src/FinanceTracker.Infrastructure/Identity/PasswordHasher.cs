using FinanceTracker.Application.Common.Interfaces;

namespace FinanceTracker.Infrastructure.Identity;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    private static readonly Lazy<string> DummyHash = new(
        () => BCrypt.Net.BCrypt.HashPassword("timing-equalizer-dummy", WorkFactor));

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string? hash)
    {
        try
        {
            var matched = BCrypt.Net.BCrypt.Verify(password, hash ?? DummyHash.Value);
            return matched && hash is not null;
        }
        catch (BCrypt.Net.SaltParseException)
        {
            return false;
        }
    }
}
