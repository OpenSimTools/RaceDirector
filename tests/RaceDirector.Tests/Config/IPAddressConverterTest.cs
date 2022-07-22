using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using Microsoft.Extensions.Configuration;
using RaceDirector.Config;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Config;

[UnitTest]
public class IPAddressConverterTest
{
    private TypeConverter _converter = new IPAddressConverter();
    private IPAddress _ip = IPAddress.Any;

    [Fact]
    public void CanConvertIPs()
    {
        Assert.True(_converter.CanConvertFrom(null, typeof(string)));
        Assert.Equal(_ip, _converter.ConvertFrom(null, null, _ip.ToString()));
    }

    [Fact]
    public void CanConvertPaddedStrings()
    {
        Assert.Equal(_ip, _converter.ConvertFrom(null, null, $" {_ip} "));
    }

    [Fact]
    public void ThrowsWhenConversionFails()
    {
        Assert.Throws<FormatException>(() =>
            _converter.ConvertFrom(null, null, "not an IP address")
        );
    }

    [Fact]
    public void RegistersToConvertConfig()
    {
        var ip = IPAddress.Loopback;
        var testConfig = new TestConfig { Ip = ip };
        var configurationRoot = testConfig.AsIConfiguration();

        Assert.Null(configurationRoot.Get<TestConfig>().Ip);

        IPAddressConverter.Register();

        Assert.Equal(ip, configurationRoot.Get<TestConfig>().Ip);
    }

    private class TestConfig
    {
        public IPAddress? Ip { get; set; }

        public IConfigurationRoot AsIConfiguration()
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { nameof(Ip), Ip.ToString() }
                })
                .Build();
        }
    }
}