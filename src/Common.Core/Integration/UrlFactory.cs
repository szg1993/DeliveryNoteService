using System.Web;

namespace Medusa.Enap.Common.Core.Integration;

public static class UrlFactory
{
    public static string Create(string baseUrl, IEnumerable<KeyValuePair<string, string?>> queryParams)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new ArgumentNullException(nameof(baseUrl), "Base URL cannot be null or empty string");

        if (queryParams is null || !queryParams.Any())
            return baseUrl;

        var query = $"{string.Join("&",
            queryParams
                .Where(p => !string.IsNullOrWhiteSpace(p.Value))
                .Select(p => $"{HttpUtility.UrlEncode(p.Key)}={HttpUtility.UrlEncode(p.Value)}"))}";

        return $"{baseUrl}?{query}";
    }
}
