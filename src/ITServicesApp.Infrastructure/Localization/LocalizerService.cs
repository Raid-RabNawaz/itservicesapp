using ITServicesApp.Application.Abstractions;
using Microsoft.Extensions.Localization;

namespace ITServicesApp.Infrastructure.Localization
{
    public sealed class LocalizerService : ILocalizer
    {
        private readonly IStringLocalizer _localizer;
        public LocalizerService(IStringLocalizerFactory factory)
        {
            _localizer = factory.Create("Resources", AppDomain.CurrentDomain.FriendlyName);
        }
        public string this[string key] => _localizer[key];
        public string Translate(string key) => _localizer[key];
    }
}
