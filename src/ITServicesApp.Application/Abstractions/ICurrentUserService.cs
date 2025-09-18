namespace ITServicesApp.Application.Abstractions
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }
        string? UserId { get; }     // raw claim (string)
        int UserIdInt { get; }       // parsed (0 if not authenticated or not an int)
        string? Email { get; }
        string? Role { get; }
    }
}
