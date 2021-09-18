using System;
using System.Text.Json;

namespace RaceDirector.Plugin.HUD.Utils
{
    public static class JsonWriterExtensions
    {
        public static void WriteObject(this Utf8JsonWriter writer, String propertyName, Action<Utf8JsonWriter> f)
        {
            writer.WritePropertyName(propertyName);
            writer.WriteObject(f);
        }

        public static void WriteObject(this Utf8JsonWriter writer, Action<Utf8JsonWriter> f)
        {
            writer.WriteStartObject();
            f(writer);
            writer.WriteEndObject();
        }
    }
}
