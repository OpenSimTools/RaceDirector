using System;
using System.ComponentModel;
using System.Globalization;
using System.Net;

namespace RaceDirector.Config;

public class IPAddressConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is not string stringValue)
            return base.ConvertFrom(context, culture, value);
        var trimmedValue = stringValue.Trim();
        return IPAddress.Parse(trimmedValue);
    }

    public static void Register()
    {
        TypeDescriptor.AddAttributes(
            typeof(IPAddress),
            new TypeConverterAttribute(typeof(IPAddressConverter))
        );
    }
}