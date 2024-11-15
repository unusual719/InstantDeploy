using System.Net.Http.Headers;

namespace InstantDeploy.Extensions
{
    public static class HttpExtensions
    {
        public static IReadOnlyCollection<KeyValuePair<string, string>> GetHeaderParameters(this HttpHeaders httpHeaders)
            => httpHeaders.SelectMany(x => x.Value.Select(y => (x.Key, y)))
            .Select(x => KeyValuePair.Create(x.Key, x.y))
            .ToList();
    }
}