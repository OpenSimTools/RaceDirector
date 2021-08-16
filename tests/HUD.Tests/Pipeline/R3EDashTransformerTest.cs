using HUD.Tests.TestUtils;
using HUD.Tests.TestUtils.Arbitraries;
using RaceDirector.Pipeline.Telemetry;
using RaceDirector.Pipeline.Telemetry.V0;
using RaceDirector.Plugin.HUD.Pipeline;
using System.Text.Json;
using Xunit;
using Xunit.Categories;
using FsCheck.Xunit;

namespace HUD.Tests.Pipeline
{
    [UnitTest]
    [Properties(Arbitrary = new[] { typeof(TelemetryArbitraries), typeof(PhysicsArbitraries) })]
    public class R3EDashTransformerTest
    {
        [Property]
        public void ContainsVersionInformation(GameTelemetry gameTelemetry)
        {
            var result = ToR3EDash(gameTelemetry);

            Assert.Equal(2, result.Path("VersionMajor").GetInt32());
            Assert.Equal(10, result.Path("VersionMinor").GetInt32());
        }

        #region Test setup

        private static JsonDocument ToR3EDash(IGameTelemetry telemetry)
        {
            var bytes = R3EDashTransformer.ToR3EDash(telemetry);
            var jsonString = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonDocument.Parse(jsonString);
        }

        #endregion
    }
}
