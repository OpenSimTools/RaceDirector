using System.Text.Json;

namespace HUD.Tests.TestUtils
{
    static class JsonExtensions
    {
        public static JsonElement Path(this JsonDocument jsonDoc, params string[] segments)
        {
            var jsonEl = jsonDoc.RootElement;
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
    }
}
