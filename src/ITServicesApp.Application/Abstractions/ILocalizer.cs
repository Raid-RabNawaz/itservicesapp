namespace ITServicesApp.Application.Abstractions
{
    public interface ILocalizer
    {
        string this[string key] { get; }
        string Translate(string key);
    }
}
