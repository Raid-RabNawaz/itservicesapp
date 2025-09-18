namespace ITServicesApp.Application.Interfaces.Security
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hash);
        bool NeedsRehash(string hash);
    }
}
