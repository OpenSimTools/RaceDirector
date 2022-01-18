﻿using System;
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

        public static void WriteArray(this Utf8JsonWriter writer, String propertyName, Action<Utf8JsonWriter> f)
        {
            writer.WriteStartArray(propertyName);
            f(writer);
            writer.WriteEndArray();
        }

        public static void WriteNumber(this Utf8JsonWriter writer, String propertyName, Double value, Int32 decimals)
        {
            var roundedValue = Decimal.Round(new Decimal(value), decimals);
            writer.WriteNumber(propertyName, roundedValue);
        }
    }
}