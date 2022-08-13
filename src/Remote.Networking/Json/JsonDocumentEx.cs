using System.Text;
using System.Text.Json;

namespace RaceDirector.Remote.Networking.Json;

public static class JsonDocumentEx
{
    public static JsonElement Path(this JsonDocument jsonDoc, params string[] segments)
    {
        return jsonDoc.RootElement.Path(segments);
    }

    public static JsonElement Path(this JsonElement jsonEl, params string[] segments) =>
        segments.Aggregate(jsonEl, (current, s) => current.GetProperty(s));

    public static bool IsNull(this JsonElement jsonEl) =>
        jsonEl.ValueKind == JsonValueKind.Null;

    public static string? GetBase64String(this JsonElement jsonEl)
    {
        var encodedName = jsonEl.GetString();
        return encodedName == null ? null : Encoding.UTF8.GetString(Convert.FromBase64String(encodedName));
    }
}