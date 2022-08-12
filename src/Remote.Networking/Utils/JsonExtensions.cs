using System.Text;
using System.Text.Json;

namespace RaceDirector.Remote.Networking.Utils;

public static class JsonExtensions
{
    public static JsonElement Path(this JsonDocument jsonDoc, params string[] segments)
    {
        return jsonDoc.RootElement.Path(segments);
    }

    public static JsonElement Path(this JsonElement jsonEl, params string[] segments)
    {
        foreach (var s in segments)
        {
            jsonEl = jsonEl.GetProperty(s);
        }
        return jsonEl;
    }

    public static bool IsNull(this JsonElement jsonEl)
    {
        return jsonEl.ValueKind == JsonValueKind.Null;
    }

    public static String? GetBase64String(this JsonElement jsonEl)
    {
        var encodedName = jsonEl.GetString();
        if (encodedName == null)
            return null;
        return Encoding.UTF8.GetString(Convert.FromBase64String(encodedName));
    }
}