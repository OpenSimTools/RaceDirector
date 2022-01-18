using RaceDirector.Pipeline.Games.R3E;
using RaceDirector.Pipeline.Telemetry;
using Xunit;
using Xunit.Categories;

namespace RaceDirector.Tests.Pipeline.Games.R3E
{
    [UnitTest]
    public class TelemetryTest
    {
        [Fact]
        public void StatefulAid__NotAvailable()
        {
            var sa = StatefulAid.Generic();

            Assert.Null(sa.Update(-1));
        }

        [Fact]
        public void StatefulAid__Inactive()
        {
            var sa = StatefulAid.Generic();

            Assert.Equal(new Aid(0, false), sa.Update(0));
            Assert.Equal(new Aid(1, false), sa.Update(1));
            Assert.Equal(new Aid(2, false), sa.Update(2));
            Assert.Equal(new Aid(3, false), sa.Update(3));
            Assert.Equal(new Aid(4, false), sa.Update(4));
        }

        [Fact]
        public void StatefulAid__Active()
        {
            var sa = StatefulAid.Generic();

            Assert.Null(sa.Update(5));
            Assert.Equal(new Aid(2, false), sa.Update(2));
            Assert.Equal(new Aid(2, true), sa.Update(5));
            Assert.Equal(new Aid(3, false), sa.Update(3));
        }
    }
}
